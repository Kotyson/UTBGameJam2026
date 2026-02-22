using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    // The static instance that other scripts can access
    public static SceneTransitionManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton Pattern: If an instance already exists, destroy this one.
        // This prevents multiple managers from spawning when you reload scenes.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        
        // This tells Unity: "Do not kill this object when a new scene loads."
        DontDestroyOnLoad(gameObject);
    }

    // Call this from anywhere: SceneTransitionManager.Instance.GoToScene("Level1");
    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}