using UnityEngine;
using UnityEngine.UI;

public class RopeTimer : MonoBehaviour
{
    [Header("Nastavení")]
    public float totalTime = 30f;
    public Color fireColor1 = new Color(1f, 0.6f, 0f);
    public Color fireColor2 = Color.red;

    [Header("Reference")]
    public RectTransform ropeRect; // Pivot X = 1
    public RectTransform fireRect; // Samostatný objekt (není Child)

    private Image fireImage;
    private float timeLeft;

    void Start()
    {
        timeLeft = totalTime;
        fireImage = fireRect.GetComponent<Image>();

        // Nastavení pivotu provazu na pravý konec (aby odhoøíval zleva)
        ropeRect.pivot = new Vector2(1f, 0.5f);
    }

    void Update()
    {
        if (timeLeft <= 0) return;

        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0, timeLeft);

        float progress = timeLeft / totalTime;

        // 1. Zmenšení provazu (smìrem k pravému pivotu)
        ropeRect.localScale = new Vector3(progress, 1, 1);

        // 2. SLEDOVÁNÍ OKRAJE:
        // Získáme rohy provazu ve svìtových souøadnicích
        Vector3[] corners = new Vector3[4];
        ropeRect.GetWorldCorners(corners);

        // corners[0] je vlevo dole, corners[1] je vlevo nahoøe
        // Pozice ohnì bude pøesnì mezi nimi (levý støed)
        Vector3 leftEdgeCenter = (corners[0] + corners[1]) / 2f;
        fireRect.position = leftEdgeCenter;

        // 3. EFEKTY (Teï už budou vidìt, protože fireRect je samostatný)
        float pulse = 0.3f + Mathf.Sin(Time.time * 15f) * 0.05f;
        fireRect.localScale = new Vector3(pulse, pulse, 1);

        

        if (timeLeft <= 0) OnTimerEnd();
    }

    void OnTimerEnd()
    {
        fireRect.gameObject.SetActive(false);
        ropeRect.gameObject.SetActive(false);
    }
}