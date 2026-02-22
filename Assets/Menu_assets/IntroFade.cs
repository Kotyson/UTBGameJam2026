using System.Collections;
using UnityEngine;

public class IntroFade : MonoBehaviour
{
    public CanvasGroup bubble1;
    public CanvasGroup bubble2;
    public CanvasGroup bubble3;

    public float fadeDuration = 1f;
    public float delayBetween = 1f;

    [Header("Nastavení konce scény")]
    public float waitAfterLastBubble = 5f; // Doba èekání na konci
    public string nextSceneName;          // Název scény, kterou chceš naèíst

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        // 1. Bublina
        yield return StartCoroutine(FadeIn(bubble1));
        yield return new WaitForSeconds(delayBetween);

        // 2. Bublina
        yield return StartCoroutine(FadeIn(bubble2));
        yield return new WaitForSeconds(delayBetween);

        // 3. Bublina
        yield return StartCoroutine(FadeIn(bubble3));

        // --- Tady se provede tvùj nový požadavek ---

        // Poèkáme 5 vteøin (nebo kolik si nastavíš v Inspectoru)
        yield return new WaitForSeconds(waitAfterLastBubble);

        // Zavoláme tvùj SceneTransitionManager
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.GoToScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Chyba: SceneTransitionManager.Instance nebyl nalezen! Máš ho ve scénì?");
        }
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        float time = 0;

        while (time < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(0, 1, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        cg.alpha = 1;
    }
}