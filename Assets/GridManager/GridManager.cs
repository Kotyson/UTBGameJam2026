using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("The 1x1x1 cube prefab to spawn")]
    public GameObject cubePrefab; 
    public float gridSize = 1f;

    // This dictionary stores the grid position as the key, and the spawned block as the value.
    private Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int, GameObject>();

    void Update()
    {
        // Left Click to Add/Remove
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        // Cast a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // If we hit an existing block...
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Remove the block
            RemoveBlock(hit.collider.gameObject);
        }
        else
        {
            // If we didn't hit anything, let's place a block at a default distance
            // (For a top-down view, you might want to raycast against a mathematical plane instead)
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldHitPoint = ray.GetPoint(enter);
                AddBlock(worldHitPoint);
            }
        }
    }

    private void AddBlock(Vector3 worldPosition)
    {
        // Convert world position to grid coordinates
        Vector3Int gridPos = WorldToGrid(worldPosition);

        // Check if a block already exists here
        if (!grid.ContainsKey(gridPos))
        {
            // Calculate exact world position center for the grid cell
            Vector3 exactWorldPos = GridToWorld(gridPos);
            
            // Spawn the cube
            GameObject newBlock = Instantiate(cubePrefab, exactWorldPos, Quaternion.identity, this.transform);
            
            // Add to our dictionary
            grid.Add(gridPos, newBlock);
            
            Debug.Log($"Added block at grid: {gridPos}");
        }
    }

    private void RemoveBlock(GameObject blockToRemove)
    {
        // Find the grid position of the clicked block
        Vector3Int gridPos = WorldToGrid(blockToRemove.transform.position);

        if (grid.ContainsKey(gridPos))
        {
            // Remove from dictionary and destroy the object
            grid.Remove(gridPos);
            Destroy(blockToRemove);
            
            Debug.Log($"Removed block at grid: {gridPos}");
        }
    }

    // --- Helper Methods for Coordinate Conversion ---

    private Vector3Int WorldToGrid(Vector3 worldPos)
    {
        // Round the world coordinates to the nearest grid size multiple
        int x = Mathf.RoundToInt(worldPos.x / gridSize);
        int y = Mathf.RoundToInt(worldPos.y / gridSize);
        int z = Mathf.RoundToInt(worldPos.z / gridSize);
        
        return new Vector3Int(x, y, z);
    }

    private Vector3 GridToWorld(Vector3Int gridPos)
    {
        // Convert back to world space
        return new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, gridPos.z * gridSize);
    }
}