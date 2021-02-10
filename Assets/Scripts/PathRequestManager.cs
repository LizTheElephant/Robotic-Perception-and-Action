using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour
{

    static PathRequestManager instance;
    Pathfinding pathfinding;
    public Pathfinding.Algorithm algorithm = Pathfinding.Algorithm.BreadthFirstSearch;

    void Awake()
    {
        instance = this;
        pathfinding = this.gameObject.GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 start, Vector3 end, Action<List<Node>, bool> callback)
    {
        var request = new PathRequest(start, end, callback);
        instance.pathfinding.FindPath(instance.algorithm, request);

    }
}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<List<Node>, bool> callback;

    public PathRequest(Vector3 _start, Vector3 _end, Action<List<Node>, bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }

}
