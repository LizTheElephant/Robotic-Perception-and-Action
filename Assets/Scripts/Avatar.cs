using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Avatar : MonoBehaviour
{

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;
    const int exploredTokenOffset = 5;

    //public Transform target;
    Vector3 target;
    public float speed = 15;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;

    private Path path;
    private bool followingPath;
    private GameObject star;

    void Start()
    {
        star = GameObject.Find("Star");
    }

    void Update()
    {
        if ((Input.GetMouseButtonDown(0)))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                target = hit.point;
                star.SetActive(true);
                star.transform.position = new Vector3(target[0], star.transform.position.y, target[2]);
                Debug.Log("Avatar moving to " + target);

                StartCoroutine("UpdatePath");
            } else {
                Debug.LogWarning("Cannot walk here");
            }
        }
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Collision triggered with " + collider.gameObject.name + ".");
        if (collider.gameObject.CompareTag("Respawn") || collider.gameObject == star)
        {
            star.SetActive(false);
            // immediately dissolve remaining tokens
            if (path != null)
            {
                Token script;
                foreach (Node n in path.lookPoints) {
                        script = n.token.GetComponent<Token>();
                        script.Dissolve();
                }
            }
        }
    }

    public void OnPathFound(List<Node> waypoints, Dictionary<int, List<Node>> exploredSet, bool pathSuccessful)
    {

        if (pathSuccessful)
        {
            StopCoroutine("FollowPath");
            StopCoroutine("ShowExploredArea");
            StopCoroutine("DrawPath");
            StopCoroutine("DissolveSurrounding");
            
            ResetAllNodes();
            path = new Path(waypoints, exploredSet, transform.position, turnDst, stoppingDst);
            followingPath = false;
            StartCoroutine("ShowExploredArea");
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
        PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));
                targetPosOld = target;
            }
        }
    }

    IEnumerator FollowPath()
    {
        followingPath = true;
        int pathIndex = 0;
        float speedPercent = 1;
        Token script;
        transform.LookAt(path.lookPoints[0].worldPosition);

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
                    if (speedPercent < 0.03f)
                    {
                        followingPath = false;
                    }
                }
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex].worldPosition - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }
            yield return null;
        }
    }

    IEnumerator ShowExploredArea()
    {
        foreach (KeyValuePair<int, List<Node>> entry in path.exploredSet)
        {
            foreach (Node n in entry.Value)
            {
                n.ExploreNode();
            }
            yield return null;
        }
        StartCoroutine("DrawPath");
        yield return null;
    }

    IEnumerator DrawPath()
    {
        //draw path from target back to start
        for (int i = path.lookPoints.Count - 1; i >= 0; i--)
        {
            path.lookPoints[i].ChooseAsPath();
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine("FollowPath");
        yield return null;
    }

    private void ResetAllNodes() {
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
    }
}
