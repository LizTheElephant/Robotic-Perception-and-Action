using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Transform target;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;

    Path path;
    const int exploredTokenOffset = 5;
    bool followingPath;
    bool exploringPath;
    /*

    public float PercentHead = 0.2f;
    private LineRenderer cachedLineRenderer;
    */
    void Start()
    {

        Debug.Log("Started Unit");
        StartCoroutine(UpdatePath());
    }

    public void OnPathFound(Vector3[] waypoints, List<Node> exploredSet, bool pathSuccessful)
    {

        if (pathSuccessful)
        {
            path = new Path(waypoints, exploredSet, transform.position, turnDst, stoppingDst);
            print("path found: " + exploredSet.Count);

            StopCoroutine("FollowPath");
            followingPath = false;
            /*if (cachedLineRenderer == null)
            {
                cachedLineRenderer = this.GetComponent<LineRenderer>();
            }*/

            StartCoroutine("ShowExploredArea");
            //StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpdatePath()
    {

        //the first few frames in unity can have large delta time values.
        //therefore the followpath accurracy is very low right after hitting play
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            //print (((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
        }
    }

    IEnumerator FollowPath()
    {

        followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            //in case we pass multiple turn boundaries per frame
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {

                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }
            //wait one frame
            yield return null;

        }
    }

    IEnumerator ShowExploredArea()
    {
        exploringPath = true;
        foreach (Node p in path.exploredSet)
        {
            p.ExploreNode();

            /*
            Vector3 ArrowOrigin = p.exploredFrom.worldPosition + Vector3.up;
            Vector3 ArrowTarget = p.worldPosition + Vector3.up;
            cachedLineRenderer.widthCurve = new AnimationCurve(new Keyframe(0, 1f)
                , new Keyframe(0.999f - PercentHead, 1f)  // neck of arrow
                , new Keyframe(1 - PercentHead, 1f)  // max width of arrow head
                , new Keyframe(1, 0f));  // tip of arrow

            cachedLineRenderer.SetPositions(new Vector3[] {
              ArrowOrigin, Vector3.Lerp(ArrowOrigin, ArrowTarget, 0.999f - PercentHead), Vector3.Lerp(ArrowOrigin, ArrowTarget, 1 - PercentHead), ArrowTarget });
              */
            //p.token.SetActive(true);
            //Instantiate(Resources.Load("ExploredToken"), p.worldPosition + Vector3.up * exploredTokenOffset, Quaternion.identity);
            yield return new WaitForSeconds(0.1f); // time delay for 2 seconds
        }

        exploringPath = false;
        StartCoroutine("FollowPath");

        //wait one frame
        yield return null;
    }

    public void OnDrawGizmos()
    {

        if (path != null)
            path.DrawWithGizmos();
    }
}
