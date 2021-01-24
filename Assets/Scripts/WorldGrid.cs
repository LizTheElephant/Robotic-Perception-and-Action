using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    public bool displayGridGizmos;
    public bool displayGridWeights;

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    int obstacleProximityPenalty = 10;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    LayerMask walkableMask;


    public enum PathPlanningPriority { ShortestDistance, ShortestTime, SmallestFuelConsumption, SafeDistanceFromObstacles };

    public PathPlanningPriority Priority;

    Node[,] grid; //collection of each node in the grid
    public List<Node> exploredNodes;

    float nodeDiameter = 1f;
    int gridSizeX = 100;
    int gridSizeY = 100;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        GameObject prefab = Resources.Load("Prefabs/Token") as GameObject;

        grid = new Node[gridSizeX, gridSizeY];
        exploredNodes = new List<Node>();
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        
        Array.Sort(walkableRegions, delegate(TerrainType x,TerrainType y) { return -x.priority.CompareTo(y.priority); });

        foreach (TerrainType region in walkableRegions)
        {
            int layerIndex = (int)Mathf.Log(region.terrainMask.value, 2);
            Debug.Log("Layer " + LayerMask.LayerToName(layerIndex) + " is walkable");
            
            walkableMask |= region.terrainMask;
            
            int penalty = 1;
            if (Priority == PathPlanningPriority.SmallestFuelConsumption)
            {
                penalty = region.terrainFuelConsumption;
            }    
            else if (Priority == PathPlanningPriority.ShortestTime) 
            {
                penalty = region.terrainPenalty;
            }

            walkableRegionsDictionary.Add(layerIndex, penalty);
        }
        
        //collision check
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                
                //collision with objects inside the unwalkable layer?
                bool collision = Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                int movementPenalty = 0;

                RaycastHit hit;
                Debug.DrawRay(worldPoint + (1000 * Vector3.up), Vector3.down, Color.cyan);
                
                foreach(TerrainType region in walkableRegions) {
                    if (Physics.Raycast(worldPoint + (1000 * Vector3.up), Vector3.down, out hit, Mathf.Infinity, region.terrainMask))  {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        Debug.LogWarning("Movement penalty : " + movementPenalty);
                        worldPoint.y += (0.5f + hit.point.y);
                        break;
                    }
                }
                

                movementPenalty += collision ? obstacleProximityPenalty : 0;
                GameObject token = Instantiate(prefab, worldPoint, Quaternion.identity) as GameObject;
                token.name = "Token: " + x + ", " + y;
                
                grid[x, y] = new Node(!collision, worldPoint, x, y, movementPenalty, token);
            }
        }

        // BlurPenaltyMap(2);

    }

    //smooth weights
    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (kernelSize - 1) / 2;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
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
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;
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
                    continue; //current node

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                //if inside the grid
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
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

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }


    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
        public int terrainFuelConsumption;
        public int priority;

    }


}
