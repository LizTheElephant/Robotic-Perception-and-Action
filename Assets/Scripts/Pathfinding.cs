using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding
{

    public static void BreadthFirstSearch(WorldGrid grid, PathRequest request, Action<PathResult> callback)
    {
        Debug.Log("BreadthFirstSearch");
        Dictionary<int, List<Node>> exploredDictionary = new Dictionary<int, List<Node>>();

        List<Node> waypoints = new List<Node>();
        bool pathSuccess = false;

        Node start = grid.NodeFromWorldPoint(request.pathStart);
        Node target = grid.NodeFromWorldPoint(request.pathEnd);
        start.parent = start;

        int iterator = 0;
        if (target.walkable)
        {

            Heap<Node> queue = new Heap<Node>(grid.MaxSize);
            queue.Add(start);
            while (queue.Count > 0)
            {
                List<Node> explored = new List<Node>();
                Node current = queue.RemoveFirst();

                if (current == target)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(current))
                {
                    if (neighbour.walkable && !queue.Contains(neighbour))
                    {
                        queue.Add(neighbour);
                        explored.Add(neighbour);
                    }
                }
                exploredDictionary.Add(iterator, new List<Node>(explored));
                explored.Clear();
                iterator++;
                
            }
        }
        if (pathSuccess)
        {
            waypoints = RetracePath(start, target);
            pathSuccess = waypoints.Count > 0;
        }
        callback(new PathResult(waypoints, exploredDictionary, pathSuccess, request.callback));
    }

    public static void AStar(WorldGrid grid, PathRequest request, Action<PathResult> callback)
    {
        UnityEngine.Debug.Log("A* Search");
        Dictionary<int, List<Node>> exploredDictionary = new Dictionary<int, List<Node>>();

        List<Node> waypoints = new List<Node>();
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
        startNode.parent = startNode;

        int iterator = 0;
        if (/*startNode.walkable && */targetNode.walkable)
        {
            UnityEngine.Debug.Log("Target walkable");
        
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);
            while (openSet.Count > 0)
            {

                List<Node> exploredSet = new List<Node>();
                Node currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);
                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + grid.GetMovementPenalty(neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                            exploredSet.Add(neighbour);
                        }

                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
                UnityEngine.Debug.Log("Iteration done");
                exploredDictionary.Add(iterator, new List<Node>(exploredSet));
                exploredSet.Clear();
                iterator++;
                
            }
        }
        UnityEngine.Debug.Log("Finished");
        if (pathSuccess)
        {
            UnityEngine.Debug.Log("Path found");
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Count > 0;
        }
        callback(new PathResult(waypoints, exploredDictionary, pathSuccess, request.callback));
    }

    static List<Node> RetracePath(Node start, Node endNode)
    {
        List<Node> path = new List<Node>();
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

    static Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

}