using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour
{
    
    public enum Algorithm
    { 
        BreadthFirstSearch, 
        Dijkstra, 
        AStar
    };



    WorldGrid grid;
    
    private Queue<List<Node>> exploredArea = new Queue<List<Node>>();
    private HashSet<Node> toReset = new HashSet<Node>();
    private bool finishedExploring = false;

    Coroutine drawPathCoroutine = null;
    Coroutine showExploredAreaCoroutine = null;
    
    void Awake()
    {
        this.grid = this.gameObject.GetComponent<WorldGrid>();
    }

    public void FindPath(Algorithm algorithm, PathRequest request)
    {
        Reset();
        switch (algorithm)
        {
            case Algorithm.BreadthFirstSearch:
                FindPath(request, (i, j) => 1 , (i, j) => 0);
                break;
            case Algorithm.Dijkstra:
                FindPath(request, TerrainGCost, (i, j) => 0);
                break;
            case Algorithm.AStar:
                FindPath(request, TerrainGCost, GetDistance);
                break;
        }
    }

    private void Reset() {
        if (showExploredAreaCoroutine != null)
            StopCoroutine(showExploredAreaCoroutine);
        if (drawPathCoroutine != null)
            StopCoroutine(drawPathCoroutine);
        foreach (Node n in toReset)
            n.Reset();
        toReset.Clear();
        exploredArea.Clear();
        finishedExploring = false;
    }

    private void FindPath(PathRequest request, Func<Node, Node, int> gCost, Func<Node, Node, int> hCost)
    {
        showExploredAreaCoroutine = StartCoroutine("ShowExploredArea");
        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
        
        startNode.parent = startNode;
        bool pathSuccess = false;

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

            exploredArea.Enqueue(exploredSet);
        }
        if (pathSuccess)
        {
            List<Node> path = RetracePath(startNode, targetNode);
            IEnumerator drawPath = DrawPath(path, request.callback);
            drawPathCoroutine = StartCoroutine(drawPath);
        }
    }

    private List<Node> RetracePath(Node start, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node current = endNode;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Add(start);
        return path;
    }


    IEnumerator ShowExploredArea()
    {
        while (exploredArea.Count == 0)
            yield return null;
        while (exploredArea.Count > 0)
        {
            foreach (Node n  in exploredArea.Dequeue())
            {
                n.ExploreNode();
                toReset.Add(n);
            }
            yield return null;
        }
        finishedExploring = true;
    }

    IEnumerator DrawPath(List<Node> path, Action<List<Node>, bool> callback)
    {
        while (!finishedExploring)
            yield return null;

        //draw path from targetPoint back to target
        foreach (Node n in path)
        {
            n.ChooseAsPath();
            toReset.Add(n);
            yield return new WaitForSeconds(0.1f);
        }
        path.Reverse();
        callback(path, path.Count > 0);
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