using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Level Editor/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [System.Serializable]
    public class TileSet
    {
        public string name;
        [Tooltip("The 16 prefabs for this type (0-15 bitmask)")]
        public GameObject[] prefabs = new GameObject[16];
    }

    [System.Serializable]
    public struct TileEntry
    {
        public Vector3Int position;
        public int typeIndex;
    }

    [Header("Visual Definitions")]
    public TileSet[] availableTileSets;

    [Header("Level Layout")]
    public List<TileEntry> placedTiles = new List<TileEntry>();

    // Helper to find a tile type at a specific coordinate
    public int GetTypeAt(Vector3Int pos)
    {
        foreach (var tile in placedTiles)
        {
            if (tile.position == pos) return tile.typeIndex;
        }
        return -1; // -1 means empty
    }
}