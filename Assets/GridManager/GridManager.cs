using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Settings")] public LevelDataSO levelData;
    public float gridSize = 1f;
    public float floorY = 0f;

    [Header("Editor Brush")] public int selectedTypeIndex = 0;

    [SerializeField] private bool PaintLevel = false;

    [Header("Gameplay & Spawning Settings")]
    [Tooltip("The index of the brush that acts as the 'Spawner Zone' (e.g., your cyan brush)")]
    public int spawnerZoneIndex = 0;

    [Tooltip("The actual TileSet indexes it can randomly spawn (e.g., 0=Cave, 2=Chest, 3=Rock)")]
    public int[] randomSpawnPool;

    [Header("Respawn Visuals")]
    public GameObject indicatorPrefab;
    public float warningTime = 2f; // How long it blinks before falling
    public float fallHeight = 10f;
    public float fallSpeed = 15f;
    public LayerMask playerLayer;
    public float caveRespawnTime = 3f;

    private Dictionary<Vector3Int, GameObject> instances = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, int> runtimeGrid = new Dictionary<Vector3Int, int>();

    private Vector3Int[] directions = new Vector3Int[]
    {
        new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 0), new Vector3Int(0, 0, -1), new Vector3Int(-1, 0, 0)
    };

    private void Start()
    {
        if (levelData != null) LoadLevelFromBlueprint();
    }

    private void LoadLevelFromBlueprint()
    {
        foreach (var go in instances.Values) Destroy(go);
        instances.Clear();
        runtimeGrid.Clear();

        foreach (var tile in levelData.placedTiles)
        {
            int typeToSpawn = tile.typeIndex;

            // If the blueprint says this is a "Spawner Zone" (Cyan), randomize it!
            if (typeToSpawn == spawnerZoneIndex && randomSpawnPool.Length > 0)
            {
                typeToSpawn = randomSpawnPool[Random.Range(0, randomSpawnPool.Length)];
            }

            runtimeGrid.Add(tile.position, typeToSpawn);
        }

        foreach (var pos in runtimeGrid.Keys) UpdateVisuals(pos);
    }

    void Update()
    {
        if (levelData == null) return;

        if (PaintLevel)
        {
            if (Input.GetMouseButtonDown(0)) HandleEditorClick(true);
            if (Input.GetMouseButtonDown(1)) HandleEditorClick(false);
        }
        else
        {
            // Simulate hitting a block with 1 damage
            if (Input.GetMouseButtonDown(0)) SimulatePlayerHittingBlock();
        }
    }

    // ==========================================
    // GAMEPLAY LOGIC (Health & Damage)
    // ==========================================

    private void SimulatePlayerHittingBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Try to find the DestructibleBlock script on the object we clicked
            DestructibleBlock block = hit.collider.GetComponentInParent<DestructibleBlock>();
            if (block != null)
            {
                block.TakeDamage(1f); // Deal 1 damage
            }
        }
    }

    // Call this the moment health hits 0. Neighbors will update immediately.
    public void InitiateDestruction(Vector3Int pos)
    {
        if (runtimeGrid.ContainsKey(pos))
        {
            runtimeGrid.Remove(pos);
            RefreshNeighbors(pos); 
        }
    }

    // --- PHASE 2: PHYSICAL CLEANUP & RESPAWN ---
    // Call this after the "shrink" animation is finished.
    public void FinishDestruction(Vector3Int pos)
    {
        if (instances.ContainsKey(pos))
        {
            GameObject instance = instances[pos];
            instances.Remove(pos);
            Destroy(instance);
        }
    
        // Start the timer to bring a new block back
        StartCoroutine(RespawnTimer(pos));
    }

    // private IEnumerator RespawnTimer(Vector3Int pos)
    // {
    //     yield return new WaitForSeconds(caveRespawnTime);
    //
    //     // Roll for a new block from the pool
    //     int newType = randomSpawnPool[Random.Range(0, randomSpawnPool.Length)];
    //     runtimeGrid.Add(pos, newType);
    //
    //     UpdateVisuals(pos);
    //     RefreshNeighbors(pos);
    // }

    private IEnumerator RespawnTimer(Vector3Int pos)
    {
        // 1. Wait until it's time to show the warning
        yield return new WaitForSeconds(Mathf.Max(0, caveRespawnTime - warningTime));

        // 2. Spawn Blinking Indicator
        Vector3 worldPos = GridToWorld(pos);
        GameObject indicator = Instantiate(indicatorPrefab, worldPos + Vector3.up * 0.05f, indicatorPrefab.transform.rotation);
        
        // Optional: Add a simple blinking effect if the prefab doesn't have one
        StartCoroutine(BlinkIndicator(indicator));

        yield return new WaitForSeconds(warningTime);

        // 3. Spawn the Falling Block (Visual only)
        int newType = randomSpawnPool[Random.Range(0, randomSpawnPool.Length)];
        // We pick the "All sides closed" prefab (mask 0 or 15 depending on your set) or just the first prefab
        GameObject visualPrefab = levelData.availableTileSets[newType].prefabs[0]; 
        
        GameObject fallingBlock = Instantiate(visualPrefab, worldPos + Vector3.up * fallHeight, Quaternion.identity);

        // Set the boolean on the script already attached to the prefab
        DestructibleBlock db = fallingBlock.GetComponent<DestructibleBlock>();
        if (db != null)
        {
            db.isFallingHazard = true;
        }

        // Ensure it has a trigger collider for the player to detect
        if (fallingBlock.GetComponent<Collider>() != null) 
            fallingBlock.GetComponent<Collider>().isTrigger = true;
        Destroy(indicator); // Remove indicator when block starts falling

        // 4. Animate Fall
        float currentY = fallHeight;
        while (currentY > floorY)
        {
            currentY -= fallSpeed * Time.deltaTime;
            fallingBlock.transform.position = new Vector3(worldPos.x, currentY, worldPos.z);
            yield return null;
        }

        // 5. Land & Check for Player
        fallingBlock.transform.position = worldPos; // Snap to ground
        CheckForCrushedPlayer(worldPos);

        // 6. Finalize Logic
        Destroy(fallingBlock); // Remove the "fake" falling block
        runtimeGrid.Add(pos, newType); // Add to real logic
        UpdateVisuals(pos); // Spawn the "real" interactive block
        RefreshNeighbors(pos);
    }

    private void CheckForCrushedPlayer(Vector3 position)
    {
        // Check in a small box at the landing site
        Collider[] hitColliders = Physics.OverlapBox(position, Vector3.one * (gridSize * 0.45f), Quaternion.identity, playerLayer);
        
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("<color=red>PLAYER DEAD: Crushed by falling block!</color>");
                // Trigger your Player Death UI/Logic here
            }
        }
    }

    private IEnumerator BlinkIndicator(GameObject indicator)
    {
        Renderer rend = indicator.GetComponentInChildren<Renderer>();
        while (indicator != null)
        {
            rend.enabled = !rend.enabled;
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    // EDITOR LOGIC & VISUALS
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

    private void RefreshNeighbors(Vector3Int pos)
    {
        foreach (var dir in directions) UpdateVisuals(pos + dir);
    }

    private void UpdateVisuals(Vector3Int pos)
    {
        if (!runtimeGrid.TryGetValue(pos, out int myType)) return; 

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

            // Hook up the DestructibleBlock script so it knows its position!
            DestructibleBlock destructible = go.GetComponent<DestructibleBlock>();
            if (destructible != null)
            {
                destructible.Initialize(this, pos);
            }
        }
    }

    private int GetRuntimeTypeAt(Vector3Int pos) => runtimeGrid.TryGetValue(pos, out int type) ? type : -1;
    private Vector3Int WorldToGrid(Vector3 v) => new Vector3Int(Mathf.RoundToInt(v.x / gridSize), 0, Mathf.RoundToInt(v.z / gridSize));
    private Vector3 GridToWorld(Vector3Int v) => new Vector3(v.x * gridSize, floorY, v.z * gridSize);
}