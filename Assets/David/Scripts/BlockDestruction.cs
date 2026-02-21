using UnityEngine;
using System.Collections.Generic;

public class BlockDestruction : MonoBehaviour
{
    [Header("Peníze")]
    public int minMoney = 10;
    public int maxMoney = 50;

    [Header("Drop Table - Drahokamy")]
    public GemData[] cheapGems;
    public GemData[] midGems;
    public GemData[] rareGems;

    [Header("Spawn Point")]
    public Transform dropPoint; // Místo, kde se vìci objeví

    // Metoda volaná pøi znièení bloku
    public void OnBlockDestroyed()
    {
        DropMoney();
        RollForGem();

        Destroy(gameObject); // Nakonec znièíme samotný blok
    }

    private void DropMoney()
    {
        int amount = Random.Range(minMoney, maxMoney + 1);
        Debug.Log($"Vypadlo {amount} penìz!");
        // Zde bys pøidal logiku pro pøiètení penìz hráèi nebo spawnutí mince
    }

    private void RollForGem()
    {
        float roll = Random.Range(0f, 100f);
        GemData selectedGem = null;

        if (roll < 50)
        {
            Debug.Log("Nepadlo nic.");
            return;
        }
        else if (roll >= 50 && roll < 75)
        {
            selectedGem = GetRandomGem(cheapGems);
            Debug.Log("Padl levný drahokam!");
        }
        else if (roll >= 75 && roll < 90)
        {
            selectedGem = GetRandomGem(midGems);
            Debug.Log("Padl støední drahokam!");
        }
        else if (roll >= 90 && roll <= 100)
        {
            selectedGem = GetRandomGem(rareGems);
            Debug.Log("Padl vzácný drahokam!");
        }

        if (selectedGem != null)
        {
            SpawnGem(selectedGem);
        }
    }

    private GemData GetRandomGem(GemData[] gemList)
    {
        if (gemList.Length == 0) return null;
        return gemList[Random.Range(0, gemList.Length)];
    }

    private void SpawnGem(GemData data)
    {
        if (data.prefab != null)
        {
            Instantiate(data.prefab, dropPoint.position, Quaternion.identity);
            
        }
    }
}