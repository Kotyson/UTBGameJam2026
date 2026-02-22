using UnityEngine;

[System.Serializable]
public class GemDrop
{
    public GameObject gem;
    [Range(0f, 100f)] public float chance; // �ance v procentech, nap�. 10 = 10%
}

[CreateAssetMenu(fileName = "New DropTable", menuName = "Loot/Drop Table")]
public class DropTable : ScriptableObject
{
    [Header("Pen�ze")]
    public int minMoney = 10;
    public int maxMoney = 50;

    [Header("Gemy (ka�d� m� vlastn� �anci)")]
    public GemDrop[] gems;

    /// <summary>
    /// Vr�t� n�hodn� gem podle �anc�, nebo null pokud ��dn� nevypadne.
    /// </summary>
    public GameObject RollGem()
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