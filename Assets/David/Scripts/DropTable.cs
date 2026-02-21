using UnityEngine;

[System.Serializable]
public class GemDrop
{
    public GemData gem;
    [Range(0f, 100f)] public float chance; // šance v procentech, napø. 10 = 10%
}

[CreateAssetMenu(fileName = "New DropTable", menuName = "Loot/Drop Table")]
public class DropTable : ScriptableObject
{
    [Header("Peníze")]
    public int minMoney = 10;
    public int maxMoney = 50;

    [Header("Gemy (každý má vlastní šanci)")]
    public GemDrop[] gems;

    /// <summary>
    /// Vrátí náhodný gem podle šancí, nebo null pokud žádný nevypadne.
    /// </summary>
    public GemData RollGem()
    {
        if (gems == null || gems.Length == 0) return null;

        foreach (GemDrop drop in gems)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < drop.chance)
            {
                return drop.gem;
            }
        }

        return null;
    }

    public int RollMoney()
    {
        return Random.Range(minMoney, maxMoney + 1);
    }
}