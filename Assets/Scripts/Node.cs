using UnityEngine;
using System.Collections;



public class Node : IHeapItem<Node>
{
    static int fCostMax = 0;
    static int fCostMin = int.MaxValue;

    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int basePenalty;
    public WorldGrid.TerrainType terrain;

    public int gCost; //distance from start
    public int hCost; //distance to target
    public Node parent;
    public Node exploredFrom;
    int heapIndex;


    Token script;

    public GameObject token;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty, WorldGrid.TerrainType _terrain, GameObject _token, float _diameter)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        basePenalty = _penalty;
        terrain = _terrain;

        _token.transform.position = _worldPos;
        _token.SetActive(false);
        token = _token;
        script = _token.GetComponent<Token>();

        token.transform.localScale *= _diameter;
    }

    public int fCost
    {
        get
        {
            int newfCost = gCost + hCost;
            
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

    public void Explore()
    {
        token.SetActive(true);
        script.SetColorCode(fCost, fCostMin, fCostMax);
        script.RotateTowardsParent(parent.token);        
        script.DissolveSurrounding();
    }

    public void SetInvalid()
    {
        token.SetActive(true);
        script.SetInvalid();
        script.Dissolve();
    }

    public void ChooseAsPath()
    {
        token.SetActive(true);
        script.SetAsChosenPath();
    }

    public void Reset()
    {
        script.Reset();
        gCost = int.MaxValue;
        hCost = 0;
    }

}
