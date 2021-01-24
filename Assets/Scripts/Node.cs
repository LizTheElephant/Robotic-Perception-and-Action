using UnityEngine;
using System.Collections;



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

    static int fCostMax = 0;
    static int fCostMin = int.MaxValue;

    Token script;

    public GameObject token;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty, GameObject _token)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;

        _token.transform.position = _worldPos;
        _token.SetActive(false);
        token = _token;
    }

    public int fCost
    {
        get
        {
            int newfCost = gCost + hCost;
            if (newfCost > fCostMax)
            {
                fCostMax = newfCost;
            } else if (newfCost < fCostMin)
            {
                fCostMin = newfCost;
            }
            return newfCost;
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
        token.SetActive(true);
        if (script == null)
            script = token.GetComponent<Token>();
        script.SetColorCode(fCost, fCostMin, fCostMax);
        script.RotateTowardsParent(parent.token);        
        script.DissolveSurrounding();
    }

    public void ChooseAsPath()
    {
        if (script == null)
            script = token.GetComponent<Token>();
        script.SetAsChosenPath();
    }

    public void Reset()
    {
        if (script == null)
          script = token.GetComponent<Token>();
        script.Reset();
    }

}
