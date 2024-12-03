using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject buildingPrefab;
    private GameObject currentBuilding;
    private bool isDragging = false;

    [SerializeField] private LayerMask tilemapLayer;

    private Dictionary<Vector2Int, TileState> gridState = new Dictionary<Vector2Int, TileState>();

    public enum TileState
    {
        Empty,
        Occupied,
        Blocked
    }

    private void Start()
    {
        mainCamera = Camera.main;

        int largeGridWidth = 30;  // Example: 30 world units wide
        int largeGridHeight = 15; // Example: 15 world units high

        // Calculate the small grid dimensions based on small tile size
        float smallTileWidth = 0.5f;
        float smallTileHeight = 0.25f;
        int smallGridWidth = Mathf.CeilToInt(largeGridWidth / smallTileWidth);
        int smallGridHeight = Mathf.CeilToInt(largeGridHeight / smallTileHeight);

        // Initialize the grid based on calculated dimensions
        InitializeGrid(smallGridWidth, smallGridHeight);
    }

    public void StartDragging(GameObject prefab)
    {
        buildingPrefab = prefab;
        isDragging = true;

        // Hide cursor
        Cursor.visible = false;

        // Create building placeholder
        currentBuilding = Instantiate(buildingPrefab);
        currentBuilding.SetActive(false);
    }

    private void Update()
    {
        if (isDragging)
        {
            MoveBuildingWithMouse();

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
            }
        }
    }

    private void MoveBuildingWithMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector3 snappedPosition = SnapToIsometricGrid(mousePosition);
        currentBuilding.transform.position = snappedPosition;

        // Check if the tile is valid for placement
        Vector2Int tilePosition = WorldToGrid(snappedPosition);
        int buildingSize = 5; // Assuming a 5x5 building size
        if (CanPlaceBuilding(tilePosition, buildingSize))
        {
            currentBuilding.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            currentBuilding.GetComponent<Renderer>().material.color = Color.red;
        }

        currentBuilding.SetActive(true);
    }

    private void PlaceBuilding()
    {
        Vector3 position = currentBuilding.transform.position;
        Vector3 snappedPosition = SnapToIsometricGrid(position);

        // Convert snapped position to grid coordinates
        Vector2Int tilePosition = WorldToGrid(snappedPosition);

        // Check if the tile is buildable
        if (gridState.ContainsKey(tilePosition) && gridState[tilePosition] == TileState.Empty)
        {
            // Mark the tile as occupied
            gridState[tilePosition] = TileState.Occupied;

            // Instantiate the building
            Instantiate(buildingPrefab, snappedPosition, Quaternion.identity);

            // Reward XP for placing a building
            EventManager.Instance.QueueEvent(new XPAddedGameEvent(50)); // Add 50 XP

            // Reset state
            isDragging = false;
            Cursor.visible = true; // Restore cursor
            Destroy(currentBuilding);
        }
        else
        {
            Debug.Log("Cannot place building here!");
        }
    }

    // private void PlaceBuilding()
    // {
    //     Vector3 position = currentBuilding.transform.position;
    //     Vector3 snappedPosition = SnapToIsometricGrid(position);

    //     Vector2Int tilePosition = WorldToGrid(snappedPosition);
    //     int buildingSize = 5; // Assuming a 5x5 building size

    //     if (CanPlaceBuilding(tilePosition, buildingSize))
    //     {
    //         // Mark all occupied tiles
    //         for (int x = -buildingSize / 2; x <= buildingSize / 2; x++)
    //         {
    //             for (int y = -buildingSize / 2; y <= buildingSize / 2; y++)
    //             {
    //                 Vector2Int tile = new Vector2Int(tilePosition.x + x, tilePosition.y + y);
    //                 gridState[tile] = TileState.Occupied;
    //             }
    //         }

    //         Instantiate(buildingPrefab, snappedPosition, Quaternion.identity);

    //         // Reward XP for placing a building
    //         EventManager.Instance.QueueEvent(new XPAddedGameEvent(50)); // Add 50 XP

    //         // Reset state
    //         isDragging = false;
    //         Cursor.visible = true; // Restore cursor
    //         Destroy(currentBuilding);
    //     }
    //     else
    //     {
    //         Debug.Log("Cannot place building here!");
    //     }
    // }

    private bool CanPlaceBuilding(Vector2Int center, int size)
    {
        for (int x = -size / 2; x <= size / 2; x++)
        {
            for (int y = -size / 2; y <= size / 2; y++)
            {
                Vector2Int tile = new Vector2Int(center.x + x, center.y + y);
                if (!gridState.ContainsKey(tile) || gridState[tile] != TileState.Empty)
                {
                    return false; // If any tile is not empty, placement fails
                }
            }
        }
        return true;
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / 0.5f); // Small tile width
        int y = Mathf.FloorToInt(worldPosition.y / 0.25f); // Small tile height
        return new Vector2Int(x, y);
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * 0.5f, gridPosition.y * 0.25f, 0f); // Convert back to world position
    }

    private Vector3 SnapToIsometricGrid(Vector3 position)
    {
        float x = Mathf.Floor(position.x / 0.5f) * 0.5f; // Small tile width
        float y = Mathf.Floor(position.y / 0.25f) * 0.25f; // Small tile height
        return new Vector3(x, y, 0f);
    }

    public void InitializeGrid(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int tilePosition = new Vector2Int(x, y);

                // Check for road or water using the Tilemap
                if (IsRoadTile(tilePosition))
                {
                    gridState[tilePosition] = TileState.Blocked;
                }
                else
                {
                    gridState[tilePosition] = TileState.Empty;
                }
            }
        }
    }

    private bool IsRoadTile(Vector2Int position)
    {
        Vector3 worldPosition = GridToWorld(position);

        // Check for collider at position
        Collider2D hit = Physics2D.OverlapPoint(worldPosition, tilemapLayer);

        if (hit != null)
        {
            Debug.Log($"Tile at {position} has tag {hit.tag}");
            return hit.CompareTag("IsRoadOrWater");
        }

        return false;
    }

    public void FreeTile(Vector2Int tilePosition)
    {
        if (gridState.ContainsKey(tilePosition))
            gridState[tilePosition] = TileState.Empty;
    }
}