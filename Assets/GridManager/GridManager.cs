using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Settings")]
    public LevelDataSO levelData;
    public float gridSize = 1f;
    public float floorY = 0f;
    
    [Header("Editor Brush")]
    public int selectedTypeIndex = 0;

    [Header("Gameplay Settings")]
    public float caveRespawnTime = 3f;

    // Tracks the physical 3D models in the scene
    private Dictionary<Vector3Int, GameObject> instances = new Dictionary<Vector3Int, GameObject>();
    
    // NEW: Tracks the ACTIVE logic state during gameplay (what is currently alive)
    private Dictionary<Vector3Int, int> runtimeGrid = new Dictionary<Vector3Int, int>();

    [SerializeField] private bool debugSimulateMining = false;
    
    private Vector3Int[] directions = new Vector3Int[]
    {
        new Vector3Int(0, 0, 1),  // N
        new Vector3Int(1, 0, 0),  // E
        new Vector3Int(0, 0, -1), // S
        new Vector3Int(-1, 0, 0)  // W
    };

    private void Start()
    {
        if (levelData != null)
        {
            LoadLevelFromBlueprint();
        }
    }

    private void LoadLevelFromBlueprint()
    {
        // 1. Clear everything
        foreach (var go in instances.Values) Destroy(go);
        instances.Clear();
        runtimeGrid.Clear();

        // 2. Copy the blueprint into our active Runtime State
        foreach (var tile in levelData.placedTiles)
        {
            runtimeGrid.Add(tile.position, tile.typeIndex);
        }

        // 3. Spawn the visuals based on the Runtime State
        foreach (var pos in runtimeGrid.Keys)
        {
            UpdateVisuals(pos);
        }
    }

    void Update()
    {
        if (levelData == null) return;

        // --- EDITOR MODE: Painting the Blueprint ---
        if (!debugSimulateMining) 
        {
            if (Input.GetMouseButtonDown(0)) HandleEditorClick(true);
            if (Input.GetMouseButtonDown(1)) HandleEditorClick(false);
        }
        // --- GAMEPLAY MODE: Player interaction (Simulated) ---
        else 
        {
            // Simulating a player attacking a block with left click
            if (Input.GetMouseButtonDown(0)) SimulatePlayerBreakingBlock();
        }
    }

    // ==========================================
    // GAMEPLAY LOGIC (Destruction & Respawn)
    // ==========================================

    private void SimulatePlayerBreakingBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int gridPos = WorldToGrid(hit.collider.transform.position);
            
            // If the block exists in our runtime grid
            if (runtimeGrid.TryGetValue(gridPos, out int typeIndex))
            {
                // Let's assume Type 0 is Cave (Destructible) and Type 1 is Boundary (Indestructible)
                if (typeIndex == 0) 
                {
                    StartCoroutine(DestroyAndRespawnTile(gridPos, typeIndex));
                }
                else
                {
                    Debug.Log("Hit a boundary! Cannot destroy.");
                }
            }
        }
    }

    public void RemoveBlock(Vector3Int pos)
    {
        StartCoroutine(DestroyAndRespawnTile(pos, 0));
    }

    private IEnumerator DestroyAndRespawnTile(Vector3Int pos, int typeIndex)
    {
        // 1. Remove from active runtime state
        runtimeGrid.Remove(pos);
        
        // 2. Destroy the physical model
        if (instances.ContainsKey(pos))
        {
            Destroy(instances[pos]);
            instances.Remove(pos);
        }

        // 3. Tell neighbors to update their bitmasks (cave changes shape)
        RefreshNeighbors(pos);

        // 4. Wait for respawn timer
        yield return new WaitForSeconds(caveRespawnTime);

        // 5. Check if something is blocking the spawn (Optional, but good for gameplay)
        // If a player is standing exactly here, you might want to wait! 

        // 6. Respawn! Add back to runtime state
        runtimeGrid.Add(pos, typeIndex);
        UpdateVisuals(pos);
        RefreshNeighbors(pos);
    }


    // ==========================================
    // EDITOR LOGIC (Painting the Template)
    // ==========================================

    private void HandleEditorClick(bool isAdding)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane p = new Plane(Vector3.up, new Vector3(0, floorY, 0));

        if (p.Raycast(ray, out float enter))
        {
            Vector3Int gridPos = WorldToGrid(ray.GetPoint(enter));

            if (isAdding) AddTileToBlueprint(gridPos);
            else RemoveTileFromBlueprint(gridPos);
        }
    }

    private void AddTileToBlueprint(Vector3Int pos)
    {
        if (levelData.GetTypeAt(pos) != -1) return;

        levelData.placedTiles.Add(new LevelDataSO.TileEntry { position = pos, typeIndex = selectedTypeIndex });
        
        // Temporarily add to runtime grid just so we can see it while editing
        runtimeGrid[pos] = selectedTypeIndex; 
        UpdateVisuals(pos);
        RefreshNeighbors(pos);
    }

    private void RemoveTileFromBlueprint(Vector3Int pos)
    {
        int index = levelData.placedTiles.FindIndex(t => t.position == pos);
        if (index == -1) return;

        levelData.placedTiles.RemoveAt(index);
        runtimeGrid.Remove(pos);
        
        if (instances.ContainsKey(pos))
        {
            Destroy(instances[pos]);
            instances.Remove(pos);
        }
        
        RefreshNeighbors(pos);
    }


    // ==========================================
    // VISUALS & BITMASKING (Reads from Runtime Grid)
    // ==========================================

    private void RefreshNeighbors(Vector3Int pos)
    {
        foreach (var dir in directions) UpdateVisuals(pos + dir);
    }

    private void UpdateVisuals(Vector3Int pos)
    {
        // Check if it exists in the ACTIVE game state, not the blueprint
        if (!runtimeGrid.TryGetValue(pos, out int myType)) return; 

        // Calculate Bitmask
        int mask = 0;
        if (GetRuntimeTypeAt(pos + directions[0]) == myType) mask += 1;
        if (GetRuntimeTypeAt(pos + directions[1]) == myType) mask += 2;
        if (GetRuntimeTypeAt(pos + directions[2]) == myType) mask += 4;
        if (GetRuntimeTypeAt(pos + directions[3]) == myType) mask += 8;

        if (instances.ContainsKey(pos))
        {
            Destroy(instances[pos]);
            instances.Remove(pos);
        }

        GameObject prefab = levelData.availableTileSets[myType].prefabs[mask];
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab, GridToWorld(pos), prefab.transform.rotation, transform);
            instances.Add(pos, go);
        }
    }

    // Helper for Bitmasking to check runtime state
    private int GetRuntimeTypeAt(Vector3Int pos)
    {
        if (runtimeGrid.TryGetValue(pos, out int type)) return type;
        return -1;
    }

    private Vector3Int WorldToGrid(Vector3 v) => new Vector3Int(Mathf.RoundToInt(v.x / gridSize), 0, Mathf.RoundToInt(v.z / gridSize));
    private Vector3 GridToWorld(Vector3Int v) => new Vector3(v.x * gridSize, floorY, v.z * gridSize);
}