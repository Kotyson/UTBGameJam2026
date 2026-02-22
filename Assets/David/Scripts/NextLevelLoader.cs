using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelLoader : MonoBehaviour
{
    [Header("Nastavení")]
    public string nextLevelName; // Název scény dalšího levelu (napø. "Level2")

    public void LoadNextLevel()
    {
        // 1. VYRESETUJEME SKÓRE ve statické tøídì
        LevelData.Player1Score = 0;
        LevelData.Player2Score = 0;

        Debug.Log("Skóre vyresetováno. Naèítám další level...");

        // 2. Naèteme další level
        SceneManager.LoadScene(nextLevelName);
    }
}