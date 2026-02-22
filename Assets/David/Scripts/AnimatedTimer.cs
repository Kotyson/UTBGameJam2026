using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RopeTimer : MonoBehaviour
{
    [Header("Nastaven�")]
    public float totalTime = 30f;
    public Color fireColor1 = new Color(1f, 0.6f, 0f);
    public Color fireColor2 = Color.red;

    [Header("Reference")]
    public RectTransform ropeRect; // Pivot X = 1
    public RectTransform fireRect; // Samostatn� objekt (nen� Child)

    private Image fireImage;
    private float timeLeft;
    
    public UnityEvent OnTimerEndEvent;

    void Start()
    {
        timeLeft = totalTime;
        fireImage = fireRect.GetComponent<Image>();

        // Nastaven� pivotu provazu na prav� konec (aby odho��val zleva)
        ropeRect.pivot = new Vector2(1f, 0.5f);
    }

    void Update()
    {
        if (timeLeft <= 0) return;

        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0, timeLeft);

        float progress = timeLeft / totalTime;

        // 1. Zmen�en� provazu (sm�rem k prav�mu pivotu)
        ropeRect.localScale = new Vector3(progress, 1, 1);

        // 2. SLEDOV�N� OKRAJE:
        // Z�sk�me rohy provazu ve sv�tov�ch sou�adnic�ch
        Vector3[] corners = new Vector3[4];
        ropeRect.GetWorldCorners(corners);

        // corners[0] je vlevo dole, corners[1] je vlevo naho�e
        // Pozice ohn� bude p�esn� mezi nimi (lev� st�ed)
        Vector3 leftEdgeCenter = (corners[0] + corners[1]) / 2f;
        fireRect.position = leftEdgeCenter;

        // 3. EFEKTY (Te� u� budou vid�t, proto�e fireRect je samostatn�)
        float pulse = 0.3f + Mathf.Sin(Time.time * 15f) * 0.05f;
        fireRect.localScale = new Vector3(pulse, pulse, 1);

        

        if (timeLeft <= 0) OnTimerEnd();
    }

    void OnTimerEnd()
    {
        fireRect.gameObject.SetActive(false);
        ropeRect.gameObject.SetActive(false);
        OnTimerEndEvent.Invoke();
    }
}