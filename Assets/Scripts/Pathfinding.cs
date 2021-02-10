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

    public Pathfinding.Algorithm algorithm = Pathfinding.Algorithm.BreadthFirstSearch;
    private WorldGrid grid;
    private HashSet<Node> toReset = new HashSet<Node>();
    Coroutine findPathCoroutine;

    
    void Awake()
    {
        this.grid = this.gameObject.GetComponent<WorldGrid>();
    }

    public void FindPath(Vector3 start, Vector3 end, Action<List<Node>, bool> callback)
    {
        Reset();
        IEnumerator findPath;
        switch (algorithm)
        {
            case Algorithm.BreadthFirstSearch:
                findPath = FindPath(start, end, callback, (i, j) => 1 , (i, j) => 0);
                break;
            case Algorithm.Dijkstra:
                findPath = FindPath(start, end, callback, TerrainGCost, (i, j) => 0);
                break;
            default:
                findPath = FindPath(start, end, callback, TerrainGCost, GetDistance);
                break;
        }
        findPathCoroutine = StartCoroutine(findPath);
    }

    private void Reset() {
        if (findPathCoroutine != null)
            StopCoroutine(findPathCoroutine);
        foreach (Node n in toReset)
            n.Reset();
        toReset.Clear();
    }

    private IEnumerator FindPath(
        Vector3 start, 
        Vector3 end, 
        Action<List<Node>, bool> callback, 
        Func<Node, Node, int> gCost, 
        Func<Node, Node, int> hCost)
    {
        Node startNode = grid.NodeFromWorldPoint(start);
        Node targetNode = grid.NodeFromWorldPoint(end);

        var openSet = new Heap<Node>(grid.MaxSize);
        var closedSet = new HashSet<Node>();

        startNode.parent = startNode;
        openSet.Enqueue(startNode);
        Node currentNode;

        for (int i = 0; openSet.Count > 0; i++)
        {
            currentNode = openSet.Dequeue();
            if (currentNode == targetNode)
            {
                List<Node> path = RetracePath(startNode, targetNode);

                //draw path from targetPoint back to target
                foreach (Node n in path)
                {
                    n.ChooseAsPath();
                    toReset.Add(n);
                    yield return new WaitForSeconds(0.1f);
                }
                path.Reverse();
                callback(path, path.Count > 0);
                yield break;
            }
            closedSet.Add(currentNode);

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
                            openSet.Enqueue(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                        
                        neighbour.Explore();
                        toReset.Add(neighbour);
                    }
                }
            }
            //speed things up a bit
            if (i % 10 == 0)
                yield return null;
        }
        foreach (Node n in toReset)
        {
            n.SetInvalid();
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
}