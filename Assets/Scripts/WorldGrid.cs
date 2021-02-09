using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldGrid : MonoBehaviour
{
    public enum PathPlanningPriority
    { 
        ShortestDistance, 
        ShortestTime, 
        SmallestFuelConsumption, 
        SafeDistanceFromObstacles
    };

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask layerMask;
        public int terrainPenalty;
        public int terrainFuelConsumption;
        public int countPenaltyIdx;
    }

    public PathPlanningPriority priority;
    public TerrainType[] walkableRegions;
    public LayerMask unwalkableMask;
    public GameObject prefab;
    
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public int obstacleProximityPenalty = 10;
    public int blurSize = 1;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;

    void Awake()
    {
        Debug.Log("WorldGrid Awake");
        Debug.Assert(gridWorldSize.x > 0 && gridWorldSize.y > 0, "Grid must be larger than 0 in both dimensions.");
        Debug.Assert(nodeRadius > 0, "Radius must be larger than 0.");
        Debug.Assert(prefab != null, "Token must be set.");

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        grid = new Node[gridSizeX, gridSizeY];

        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        Array.Sort(walkableRegions, delegate(TerrainType x,TerrainType y) { return x.countPenaltyIdx.CompareTo(y.countPenaltyIdx); });

        // populate grid
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                grid[x, y] = CreateNodeAtIndex(worldBottomLeft, x, y);
            }
        }
        BlurPenaltyMap();
    }
    
    private Node CreateNodeAtIndex(Vector3 worldBottomLeft, int x, int y)
    {
        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

        bool collision = Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
        // proximity penalty will effect neighboring nodes after blurring the map
        int movementPenalty = collision ? obstacleProximityPenalty : 0;

        RaycastHit hit;
        TerrainType terrain = null;
        // count only movement penalty of layer with lowest countPenaltyIdx
        foreach(TerrainType i in walkableRegions) {
            if (Physics.Raycast(worldPoint + (1000 * Vector3.up), Vector3.down, out hit, Mathf.Infinity, i.layerMask))  {
                terrain = i;
                worldPoint.y += (0.5f + hit.point.y);
                break;
            }
        }
        GameObject token = Instantiate(prefab, worldPoint, Quaternion.identity) as GameObject;
        token.name =  $"Token: {x}, {y}";
        
        return new Node(!collision, worldPoint, x, y, movementPenalty, terrain, token, nodeRadius);
    }

    //smooth weights
    private void BlurPenaltyMap()
    {
        if (blurSize <= 0)
        {
            return;
        }
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (kernelSize - 1) / 2;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].basePenalty;
            }

            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].basePenalty + grid[addIndex, y].basePenalty;
            }
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].basePenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].basePenalty = blurredPenalty;
            }
        }

    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    // current node
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    if (grid[checkX, checkY].walkable)
                        neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
       
        int x = Mathf.RoundToInt((gridSizeX - 1) * Mathf.Clamp01(percentX));
        int y = Mathf.RoundToInt((gridSizeY - 1) * Mathf.Clamp01(percentY));
       
        return grid[x, y];
    }

    public Vector2 findFCostDimensions()
    {
        int maxFCost = 0, minFCost = int.MaxValue;
        foreach (Node n in grid)
        {
            if (n.fCost > maxFCost)
            {
                maxFCost = n.fCost;
            } else if (n.fCost < minFCost)
            {
                minFCost = n.fCost;
            }
        }
        return new Vector2(minFCost, maxFCost);
    }

    public int GetMovementPenalty(Node n) {
        int penalty = 0;
        switch (priority)
        {
            case PathPlanningPriority.SmallestFuelConsumption:
                penalty += n.terrain.terrainFuelConsumption;
                break;
            case PathPlanningPriority.ShortestTime:
                penalty +=  + n.terrain.terrainPenalty;
                break;
            default:
                penalty +=  + 1;
                break;
        }
        return penalty += n.basePenalty;
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }
    public int WalkableMask
    {
        get
        {
            return ~(unwalkableMask);
        }
    }
    public Node[,] NodeGrid
    {
        get
        {
            return grid;
        }
    }
}
