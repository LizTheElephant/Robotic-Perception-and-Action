using UnityEngine;
using System.Collections;
/*
public enum Direction
{
    C = 0,
    N  = 1,
    NE = 2,
    E  = 3,
    SE = 4,
    S  = 5,
    SW = 6,
    W  = 7,
    NW = 8

}*/

public class Node : IHeapItem<Node>
{

    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int movementPenalty;

    public int gCost; //distance from start
    public int hCost; //distance to target
    public Node parent;
    public Node exploredFrom;
    int heapIndex;
    public bool isactive;

    public int exploredIndex;
    //    public Direction exploredFrom;

    public GameObject token;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty, GameObject _token)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
        exploredIndex = 0;


        isactive = false;

        _token.transform.position = _worldPos;
        _token.SetActive(false);
        token = _token;

    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }

    public void ExploreNode()
    {
        isactive = true;
        token.SetActive(true);
        ++exploredIndex;
    }
    public void ExploreFrom(Node parent)
    {
        exploredFrom = parent;
        /*
        float delta_x = (parent.worldPosition.x - worldPosition.x);
        float delta_y = (parent.worldPosition.z - worldPosition.z);

        if (delta_x > 0)
        {
            if (delta_y > 0)
            {
                exploredFrom = Direction.NE;
            }
            else if (delta_y < 0)
            {
                exploredFrom = Direction.SE;
            }
            else
            {
                exploredFrom = Direction.E;
            }
        }
        else if (delta_x < 0)
        {
            if (delta_y > 0)
            {
                exploredFrom = Direction.NW;
            }
            else if (delta_y < 0)
            {
                exploredFrom = Direction.SW;
            }
            else
            {
                exploredFrom = Direction.W;
            }
        }
        else
        {

            if (delta_y > 0)
            {
                exploredFrom = Direction.N;
            }
            else if (delta_y < 0)
            {
                exploredFrom = Direction.S;
            }
            else
            {
                exploredFrom = Direction.C;
            }
        }
        */
    }
}
