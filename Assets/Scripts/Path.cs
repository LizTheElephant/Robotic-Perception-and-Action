using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{

    public readonly List<Node> lookPoints;
    public readonly List<Node> exploredSet;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public Path(List<Node> waypoints, List<Node> _exploredSet, Vector3 startPos, float turnDst, float stoppingDst)
    {
        lookPoints = waypoints;
        exploredSet = _exploredSet;
        turnBoundaries = new Line[lookPoints.Count];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);
        for (int i = 0; i < lookPoints.Count; i++)
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i].worldPosition);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
            previousPoint = turnBoundaryPoint;
        }



        float dstFromEndPoint = 0;
        for (int i = lookPoints.Count - 1; i > 0; i--)
        {
            dstFromEndPoint += Vector3.Distance(lookPoints[i].worldPosition, lookPoints[i - 1].worldPosition);
            if (dstFromEndPoint > stoppingDst)
            {
                slowDownIndex = i;
                break;
            }
        }
    }

    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public void DrawWithGizmos()
    {
        /*
            Debug.Log("Exploring");
            foreach (Node p in exploredSet)
            {
            if (p.isactive == true)
            {
                Gizmos.DrawCube(p.worldPosition + Vector3.up + Vector3.up, Vector3.one * (2 - 0.2f));

                Vector3 direction = p.worldPosition + Vector3.up + Vector3.up;
                Vector3 pos = p.worldPosition;
                Gizmos.DrawRay(pos, direction);

                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

                Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
                Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
            }
            }
        if (followingPath)
        {

            Gizmos.color = Color.blue;

            Debug.Log("LookPoint" + lookPoints.Length);
            foreach (Vector3 p in lookPoints)
            {
                Gizmos.DrawCube(p + Vector3.up + Vector3.up, Vector3.one * (2 - 0.2f));
            }
        }
        //actual selected
       

        //explored
        Gizmos.color = Color.green;
        foreach (Line l in turnBoundaries) {
            l.DrawWithGizmos (10);
        }

        Debug.Log("ExploredPoints: " + exploredSet.Count);

        */

    }

}
