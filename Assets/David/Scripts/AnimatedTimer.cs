using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Přidáno pro přepínání scén

public class RopeTimer : MonoBehaviour
{
    [Header("Nastavení času")]
    public float totalTime = 30f;
    public Color fireColor1 = new Color(1f, 0.6f, 0f);
    public Color fireColor2 = Color.red;

    [Header("Reference na UI")]
    public RectTransform ropeRect; // Pivot X = 1
    public RectTransform fireRect; // Samostatný objekt

    [Header("Ukládání skóre & Scéna")]
    public string nextSceneName = "Ending_Level"; // Název scény, která se má načíst
    public Chest player1Chest; // Přetáhni truhlu hráče 1 z inspektoru
    public Chest player2Chest; // Přetáhni truhlu hráče 2 z inspektoru

    private Image fireImage;
    private float timeLeft;
    
    public UnityEvent OnTimerEndEvent;

    void Start()
    {
        timeLeft = totalTime;
        fireImage = fireRect.GetComponent<Image>();

        // Nastavení pivotu provazu na pravý konec
        ropeRect.pivot = new Vector2(1f, 0.5f);
    }

    void Update()
    {
        if (timeLeft <= 0) return;

        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0, timeLeft);

        float progress = timeLeft / totalTime;

        // 1. Zmenšení provazu
        ropeRect.localScale = new Vector3(progress, 1, 1);

        // 2. Sledování okraje
        Vector3[] corners = new Vector3[4];
        ropeRect.GetWorldCorners(corners);
        Vector3 leftEdgeCenter = (corners[0] + corners[1]) / 2f;
        fireRect.position = leftEdgeCenter;

        // 3. Efekty ohně
        float pulse = 0.6f + Mathf.Sin(Time.time * 15f) * 0.05f;
        fireRect.localScale = new Vector3(pulse, pulse, 1);

        if (timeLeft <= 0) OnTimerEnd();
    }

    void OnTimerEnd()
    {
        fireRect.gameObject.SetActive(false);
        ropeRect.gameObject.SetActive(false);

        // --- NOVÁ LOGIKA PRO PŘEPNUTÍ ---

        // 1. Uložíme body do statické třídy LevelData
        if (player1Chest != null) LevelData.Player1Score = player1Chest.totalPoints;
        if (player2Chest != null) LevelData.Player2Score = player2Chest.totalPoints;

        Debug.Log("Čas vypršel. Skóre uloženo. Načítám scénu: " + nextSceneName);

        // 2. Spustíme event (pokud tam máš ještě jiné věci, co se mají stát)
        OnTimerEndEvent.Invoke();

        // 3. Načteme novou scénu
        SceneManager.LoadScene(nextSceneName);
    }
}