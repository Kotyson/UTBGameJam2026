using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 1f;

    [Tooltip("Array of 16 prefabs. Index corresponds to the bitmask value (0-15).")]
    public GameObject[] tilePrefabs = new GameObject[16];

    // Dictionary stores the grid position and the current GameObject occupying it
    private Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int, GameObject>();

    // The 4 neighbor directions (assuming a flat ground plane on X and Z)
    private Vector3Int[] directions = new Vector3Int[]
    {
        new Vector3Int(0, 0, 1),  // North (Value: 1)
        new Vector3Int(1, 0, 0),  // East  (Value: 2)
        new Vector3Int(0, 0, -1), // South (Value: 4)
        new Vector3Int(-1, 0, 0)  // West  (Value: 8)
    };

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Remove block if we hit one
            Vector3Int gridPos = WorldToGrid(hit.collider.transform.position);
            RemoveBlock(gridPos);
        }
        else
        {
            // Add block on the ground plane
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3Int gridPos = WorldToGrid(ray.GetPoint(enter));
                AddBlock(gridPos);
            }
        }
    }

    private void AddBlock(Vector3Int gridPos)
    {
        if (grid.ContainsKey(gridPos)) return;

        // Temporarily put null in the dictionary so neighbors know a block is here
        grid.Add(gridPos, null); 
        
        // Calculate the correct shape and spawn it
        Debug.Log("----------");
        UpdateTile(gridPos);
        Debug.Log("----------");
        // Tell the 4 neighbors to update themselves
        UpdateNeighbors(gridPos);
    }

    private void RemoveBlock(Vector3Int gridPos)
    {
        if (!grid.ContainsKey(gridPos)) return;

        // Destroy the game object
        Destroy(grid[gridPos]);
        
        // Remove from the dictionary so neighbors know it's gone
        grid.Remove(gridPos);

        // Tell the 4 neighbors to update themselves
        UpdateNeighbors(gridPos);
    }

    // --- The Bitmask Auto-Tiling Logic ---

    private void UpdateNeighbors(Vector3Int gridPos)
    {
        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborPos = gridPos + dir;
            if (grid.ContainsKey(neighborPos))
            {
                UpdateTile(neighborPos);
            }
        }
    }

    private void UpdateTile(Vector3Int gridPos)
    {
        // 1. Calculate the bitmask value based on neighbors
        int maskValue = 0;
        if (grid.ContainsKey(gridPos + directions[0])) maskValue += 1; // North
        if (grid.ContainsKey(gridPos + directions[1])) maskValue += 2; // East
        if (grid.ContainsKey(gridPos + directions[2])) maskValue += 4; // South
        if (grid.ContainsKey(gridPos + directions[3])) maskValue += 8; // West

        // 2. Destroy the old block if it exists
        if (grid[gridPos] != null)
        {
            Destroy(grid[gridPos]);
        }

        // 3. Spawn the new block based on the mask value
        Vector3 worldPos = GridToWorld(gridPos);
        GameObject newBlock = Instantiate(tilePrefabs[maskValue], worldPos, tilePrefabs[maskValue].transform.rotation, this.transform);
        
        // 4. Save it back to the dictionary
        Debug.Log("Position: " + maskValue);
        grid[gridPos] = newBlock;
    }

    // --- Helpers ---
    private Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPos.x / gridSize),
            Mathf.RoundToInt(worldPos.y / gridSize),
            Mathf.RoundToInt(worldPos.z / gridSize)
        );
    }

    private Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, gridPos.z * gridSize);
    }
}