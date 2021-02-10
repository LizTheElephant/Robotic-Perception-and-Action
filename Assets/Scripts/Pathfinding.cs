using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding
{
    
    public enum Algorithm
    { 
        BreadthFirstSearch, 
        Dijkstra, 
        AStar
    };

    WorldGrid grid;

    public Pathfinding(WorldGrid grid)
    {
        this.grid = grid;
    }

    public void FindPath(Algorithm algorithm, PathRequest request, Action<PathResult> callback)
    {
        switch (algorithm)
        {
            case Algorithm.BreadthFirstSearch:
                FindPath(request, callback, (i, j) => 1 , (i, j) => 0);
                break;
            case Algorithm.Dijkstra:
                FindPath(request, callback, TerrainGCost, (i, j) => 0);
                break;
            case Algorithm.AStar:
                FindPath(request, callback, TerrainGCost, GetDistance);
                break;
        }
    }

    private void FindPath(PathRequest request, Action<PathResult> callback, Func<Node, Node, int> gCost, Func<Node, Node, int> hCost)
    {
        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
        
        startNode.parent = startNode;

        bool pathSuccess = false;
        
        // Store explored nodes per iteration to later visualize the progess
        var exploredDictionary = new Dictionary<int, List<Node>>();
        var openSet = new Heap<Node>(grid.MaxSize);
        var closedSet = new HashSet<Node>();

        openSet.Enqueue(startNode);
        for (int i = 0; openSet.Count > 0; i++)
        {
            Node currentNode = openSet.Dequeue();
            var exploredSet = new List<Node>();

            closedSet.Add(currentNode);
            if (currentNode == targetNode)
            {
                pathSuccess = true;
                break;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (neighbour.walkable && !closedSet.Contains(neighbour)) {
                    int tentativeGCost = currentNode.gCost + gCost(currentNode, neighbour);
                    if (tentativeGCost < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = tentativeGCost;
                        neighbour.hCost = hCost(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Enqueue(neighbour);
                            exploredSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
            exploredDictionary.Add(i, new List<Node>(exploredSet));
            exploredSet.Clear();
        }
        
        var waypoints = new List<Node>();
        if (pathSuccess)
        {
            UnityEngine.Debug.Log("Found path");
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Count > 0;
        }
        callback(new PathResult(waypoints, exploredDictionary, pathSuccess, request.callback));
    }


    private static List<Node> RetracePath(Node start, Node endNode)
    {
        var path = new List<Node>();
        Node current = endNode;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

    private static Vector3[] SimplifyPath(List<Node> path)
    {
        var waypoints = new List<Vector3>();
        var directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            var directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    private int TerrainGCost(Node nodeA, Node nodeB)
    {
        return grid.GetMovementPenalty(nodeB) + GetDistance(nodeA, nodeB);
    }

    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private struct CostFunc
    {
        public Func<Node, Node, int> gCost;
        public Func<Node, Node, int> hCost;

        public CostFunc(Func<Node, Node, int> gCost, Func<Node, Node, int> hCost)
        {
            this.gCost = gCost;
            this.hCost = hCost;
        }
    }
}