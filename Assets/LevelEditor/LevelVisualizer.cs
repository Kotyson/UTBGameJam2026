using UnityEngine;

public class LevelVisualizer : MonoBehaviour
{
    public LevelDataSO levelData;
    public float gridSize = 1f;

    private void OnDrawGizmos()
    {
        if (levelData == null) return;

        foreach (var tile in levelData.placedTiles)
        {
            // Color code the gizmos based on type
            Gizmos.color = (tile.typeIndex == 0) ? Color.cyan : Color.green;
            
            Vector3 pos = new Vector3(tile.position.x * gridSize, 0, tile.position.z * gridSize);
            Gizmos.DrawWireCube(pos, Vector3.one * gridSize);
            
            // Draw a semi-transparent floor so you can see where to put props
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.2f);
            Gizmos.DrawCube(pos, new Vector3(gridSize, 0.1f, gridSize));
        }
    }
}