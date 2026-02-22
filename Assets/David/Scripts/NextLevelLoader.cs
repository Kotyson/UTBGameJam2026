using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelLoader : MonoBehaviour
{
    public void LoadNextLevel()
    {
        // 1. Resetujeme body
      //  LevelData.Player1Score = 0;
       // LevelData.Player2Score = 0;

        // 2. Rozhodneme, kam jít dál
        string sceneToLoad = "";

        switch (LevelData.LastSceneName)
        {
            case "Level1":
                sceneToLoad = "Level2";
                break;
            case "Level2":
                sceneToLoad = "Level3";
                break;
            case "Level3":
                sceneToLoad = "Menu"; // Po levelu 3 jdeme do menu
                break;
            default:
                sceneToLoad = "Menu"; // Pojistka
                break;
        }

        Debug.Log($"Pøišli jsme z {LevelData.LastSceneName}, pokraèujeme do {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}