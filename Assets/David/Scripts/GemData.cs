using UnityEngine;

[CreateAssetMenu(fileName = "New Gem", menuName = "Loot/Gem")]
public class GemData : ScriptableObject
{
    public string gemName;
    public GameObject prefab; // Model drahokamu, který se spawne
    public int value;         // Cena drahokamu
    
}