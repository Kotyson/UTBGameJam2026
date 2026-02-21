using UnityEngine;

[CreateAssetMenu(menuName = "Blocks/Block Data")]
public class BlockData : ScriptableObject
{
    public string blockName;
    public float maxHealth = 5f;
}