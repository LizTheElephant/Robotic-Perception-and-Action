using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using System.Collections.Generic;



public class Avatar : MonoBehaviour
{

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;
    const int exploredTokenOffset = 5;

    public GameObject target;
    public float speed = 15;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;

    private Path path;
    private Vector3 targetPoint;
    private bool followingPath;

    private Pathfinding pathfinder;
    private AudioSource collision;
    
    void Start()
    {
        target.SetActive(false);
        pathfinder = GameObject.Find("Grid").GetComponent<Pathfinding>();
        collision = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.collider.gameObject.name);
                targetPoint = new Vector3(hit.point[0], 0, hit.point[2]);
                target.transform.position = hit.point;
                target.SetActive(true);
                Debug.Log("Avatar moving to " + targetPoint);
                StartCoroutine("UpdatePath");
            } else {
                Debug.LogWarning("Cannot walk here");
            }
        }
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Collision with " + collider.gameObject.name + ".");
        if (collider.gameObject.CompareTag("Respawn") || collider.gameObject == target)
        {
            collision.Play();
            target.SetActive(false);
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

    public void Stop()
    {
            StopCoroutine("FollowPath");
    }


    public void OnPathFound(List<Node> waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            Stop();
            path = new Path(waypoints, transform.position, turnDst, stoppingDst);
            followingPath = false;
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpdatePath()
    {
        Stop();
        //the first few frames in unity can have large delta time values.
        //therefore the followpath accurracy is very low right after hitting play
        if (Time.timeSinceLevelLoad > .5f)
            pathfinder.FindPath(transform.position, targetPoint, OnPathFound);
        yield return new WaitForSeconds(.5f);
        
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
                Quaternion targetPointRotation = Quaternion.LookRotation(path.lookPoints[pathIndex].worldPosition - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetPointRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }
            yield return null;
        }
    }
}
