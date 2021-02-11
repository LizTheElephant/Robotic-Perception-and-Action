using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
public class Pathfinding : MonoBehaviour
{
    
    public enum Algorithm
    { 
        BreadthFirstSearch, 
        Dijkstra, 
        AStar
    };


    private HashSet<Node> toReset = new HashSet<Node>();
    private Coroutine findPathCoroutine;
    private WorldGrid grid;
    private AudioSource invalidTarget;
    private AudioSource validTarget;

    private Func<Node, Node, int> gCost;
    private Func<Node, Node, int> hCost;
    
    
    void Awake()
    {
        this.grid = GetComponent<WorldGrid>();
        AudioSource[] audioSources = GetComponents<AudioSource>();
        invalidTarget = audioSources[0];
        validTarget = audioSources[1];
        gCost = DefaultGCost;
        hCost = DefaultHCost;
    }

    public Algorithm algorithm
    {
        set
        {
            switch (value)
            {
                case Algorithm.BreadthFirstSearch:
                    gCost = DefaultGCost; 
                    hCost = DefaultHCost;
                    break;
                case Algorithm.Dijkstra:
                    gCost = TerrainGCost; 
                    hCost = DefaultHCost;
                    break;
                default:
                    gCost = TerrainGCost;
                    hCost = GetDistance;
                    break;
            }
        }
    }

    public void FindPath(Vector3 start, Vector3 end, Action<List<Node>, bool> callback)
    {
        Reset();
        Node startNode = grid.NodeFromWorldPoint(start);
        Node targetNode = grid.NodeFromWorldPoint(end);

        if (targetNode.MarkTarget())
        {
            validTarget.Play();
            IEnumerator findPath = FindPathEnum(startNode, targetNode, callback);
            findPathCoroutine = StartCoroutine(findPath);
        }
        else {
            invalidTarget.Play();
        }
    }

    private void Reset() {
        if (findPathCoroutine != null)
            StopCoroutine(findPathCoroutine);
        foreach (Node n in toReset)
            n.Reset();
        toReset.Clear();
    }

    private IEnumerator FindPathEnum(Node startNode, Node targetNode, Action<List<Node>, bool> callback)
    {
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
                    int tentativeGCost = gCost(currentNode, neighbour);
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
            n.SetInvalid();
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
        return nodeA.gCost + grid.GetMovementPenalty(nodeB) + GetDistance(nodeA, nodeB);
    }

    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private int DefaultGCost(Node nodeA, Node nodeB)
    {
        return nodeA.gCost + 1;
    }

    private int DefaultHCost(Node nodeA, Node nodeB)
    {
        return 0;
    }
}