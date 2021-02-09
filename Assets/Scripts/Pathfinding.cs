using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding
{
    public static void BreadthFirstSearch(WorldGrid grid, PathRequest request, Action<PathResult> callback)
    {
        UnityEngine.Debug.Log("Breadth First Search");
        Dictionary<int, List<Node>> exploredDictionary = new Dictionary<int, List<Node>>();

        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
        startNode.parent = startNode;

        int iterator = 0;
        if (targetNode.walkable)
        {
            UnityEngine.Debug.Log("Target walkable");
        
            var openSet = new Queue<Node>(grid.MaxSize);
            var closedSet = new HashSet<Node>();
            openSet.Enqueue(startNode);
            while (openSet.Count > 0)
            {

                List<Node> exploredSet = new List<Node>();
                Node currentNode = openSet.Dequeue();

                closedSet.Add(currentNode);
                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;

                    if (!openSet.Contains(neighbour))
                    {
                        neighbour.parent = currentNode;
                        openSet.Enqueue(neighbour);
                        exploredSet.Add(neighbour);
                    }
                }
                UnityEngine.Debug.Log("Iteration done");
                exploredDictionary.Add(iterator, new List<Node>(exploredSet));
                exploredSet.Clear();
                iterator++;
                
            }
        }
        
        List<Node> waypoints = new List<Node>();
        if (pathSuccess)
        {
            UnityEngine.Debug.Log("Path found");
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Count > 0;
        }
        callback(new PathResult(waypoints, exploredDictionary, pathSuccess, request.callback));
    }

    public static void AStar(WorldGrid grid, PathRequest request, Action<PathResult> callback) {
        AStar(grid, request, callback, false);
    }

    public static void Dijkstra(WorldGrid grid, PathRequest request, Action<PathResult> callback) {
        AStar(grid, request, callback, true);
    }


    private static void AStar(WorldGrid grid, PathRequest request, Action<PathResult> callback, bool djikstra)
    {
        UnityEngine.Debug.Log("A* Search");
        var exploredDictionary = new Dictionary<int, List<Node>>();
        
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
        
        startNode.parent = startNode;

        int iterator = 0;
        if (targetNode.walkable)
        {
            UnityEngine.Debug.Log("Target walkable");
        
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Enqueue(startNode);
            while (openSet.Count > 0)
            {
                List<Node> exploredSet = new List<Node>();
                Node currentNode = openSet.Dequeue();

                closedSet.Add(currentNode);
                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                     //gCost - distance from start, hCost - distance from Goal
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;

                    int tentativeGCost = currentNode.gCost + GetDistance(currentNode, neighbour) + grid.GetMovementPenalty(neighbour);
                    if (tentativeGCost < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = tentativeGCost;
                        neighbour.hCost = djikstra ? 0 : GetDistance(neighbour, targetNode);
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
                UnityEngine.Debug.Log("Iteration done");
                exploredDictionary.Add(iterator, new List<Node>(exploredSet));
                exploredSet.Clear();
                iterator++;
            }
        }
        
        List<Node> waypoints = new List<Node>();
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