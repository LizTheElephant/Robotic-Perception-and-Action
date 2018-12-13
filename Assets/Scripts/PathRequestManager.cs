using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour
{

    Queue<PathResult> results = new Queue<PathResult>();

    static PathRequestManager instance;
    Pathfinding pathfinding;

    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock (results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.exploredPoints, result.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequest request)
    {
        print("Path: " + request.pathStart + ", " + request.pathEnd);

        ThreadStart threadStart = delegate {
            instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {
        lock (results)
        {
            results.Enqueue(result);
        }
    }



}

public struct PathResult
{
    public List<Node> path;
    public Dictionary<int, List<Node>> exploredPoints;
    public bool success;
    public Action<List<Node>, Dictionary<int, List<Node>>, bool> callback;

    public PathResult(List<Node> path, Dictionary<int, List<Node>> exploredPoints, bool success, Action<List<Node>, Dictionary<int, List<Node>>, bool> callback)
    {
        this.path = path;
        this.exploredPoints = exploredPoints;
        this.success = success;
        this.callback = callback;
    }

}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<List<Node>, Dictionary<int, List<Node>>, bool> callback;

    public PathRequest(Vector3 _start, Vector3 _end, Action<List<Node>, Dictionary<int, List<Node>>, bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }

}
