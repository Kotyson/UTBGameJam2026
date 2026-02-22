using UnityEngine;
using UnityEngine.SceneManagement; // Dùležité pro naèítání scén!

public class LevelEnder : MonoBehaviour
{
    [Header("Reference na truhly")]
    public Chest chestPlayer1;
    public Chest chestPlayer2;

    [Header("Nastavení scény")]
    public string scoreSceneName = "Ending_Level"; // Zde zadej pøesný název tvé scény se skóre

    // Tuto metodu napojíme na tvùj RopeTimer
    public void FinishLevelAndSaveScores()
    {
        // 1. Získáme body z truhel a uložíme je do statické tøídy
        if (chestPlayer1 != null) LevelData.Player1Score = chestPlayer1.totalPoints;
        if (chestPlayer2 != null) LevelData.Player2Score = chestPlayer2.totalPoints;

        Debug.Log($"Level konèí! Ukládám skóre - P1: {LevelData.Player1Score}, P2: {LevelData.Player2Score}");

        // 2. Pøepneme do scény s výsledky
        SceneManager.LoadScene(scoreSceneName);
    }
}