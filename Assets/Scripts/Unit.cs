using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Unit : MonoBehaviour
{

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    //public Transform target;
    Vector3 target;
    public float speed = 15;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;


    Path path;
    const int exploredTokenOffset = 5;
    bool followingPath;
    GameObject star;

    void Start()
    {
        star = GameObject.Find("Star");
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetMouseButtonDown(0)))
        {
            RaycastHit hit;
            Ray ray;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Debug.LogWarning("MousePosition" + ray) ;

            if (Physics.Raycast(ray, out hit))
            {
                target = hit.point;
                star.SetActive(true);
                star.transform.position = target;
                Debug.Log("Unit moving to " + target);
                StartCoroutine("UpdatePath");
            }

        }
        if (!Mathf.Approximately(gameObject.transform.position.magnitude, target.magnitude))
        {
            Debug.Log("Unit Update: Starting..");
        }

    }

    public void OnPathFound(List<Node> waypoints, Dictionary<int, List<Node>> exploredSet, bool pathSuccessful)
    {

        if (pathSuccessful)
        {
            Debug.Log("Unit found Path");
            StopCoroutine("FollowPath");
            StopCoroutine("ShowExploredArea");
            StopCoroutine("DrawPath");
            StopCoroutine("DissolveSurrounding");

            if (path != null)
            {
                foreach (KeyValuePair<int, List<Node>> entry in path.exploredSet)
                {
                    foreach (Node n in entry.Value)
                    {
                        n.Reset();
                    }
                }
                foreach (Node n in path.lookPoints)
                {
                    n.Reset();
                }

            }


            path = new Path(waypoints, exploredSet, transform.position, turnDst, stoppingDst);
            print("path found: " + exploredSet.Count);

            followingPath = false;
            StartCoroutine("ShowExploredArea");
        }
    }

    IEnumerator UpdatePath()
    {

        Debug.Log("Unit Updated Path");
        //the first few frames in unity can have large delta time values.
        //therefore the followpath accurracy is very low right after hitting play
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target;

        while (true)
        {


            yield return new WaitForSeconds(minPathUpdateTime);
            //print (((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));
                targetPosOld = target;
            }
        }
    }


    IEnumerator FollowPath()
    {

        Debug.Log("Unit follows Path");
        followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0].worldPosition);

        float speedPercent = 1;

        Token script;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            //in case we pass multiple turn boundaries per frame
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                script = path.lookPoints[pathIndex].token.GetComponent<Token>();
                script.Dissolve();

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
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex].worldPosition - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);


            }
            //wait one frame
            yield return null;

        }
    }

    IEnumerator ShowExploredArea()
    {

        Debug.Log("Unit shows Explored Area");

        foreach (KeyValuePair<int, List<Node>> entry in path.exploredSet)
        {
            foreach (Node n in entry.Value)
            {
                n.ExploreNode();
            }
            yield return null; //new WaitForSeconds(0.03f);
        }
        StartCoroutine("DrawPath");
        yield return null;
    }

    IEnumerator DrawPath()
    {

        Debug.Log("Unit draws Path");

        for (int i = path.lookPoints.Count - 1; i >= 0; i--)
        {
            path.lookPoints[i].ChooseAsPath();
            yield return new WaitForSeconds(0.1f); // time delay for 2 seconds
        }

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
