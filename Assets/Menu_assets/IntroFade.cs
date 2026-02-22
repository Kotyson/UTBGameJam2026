using System.Collections;
using UnityEngine;

public class IntroFade : MonoBehaviour
{
    public CanvasGroup bubble1;
    public CanvasGroup bubble2;
    public CanvasGroup bubble3;

    public float fadeDuration = 1f;
    public float delayBetween = 1f;

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        yield return StartCoroutine(FadeIn(bubble1));
        yield return new WaitForSeconds(delayBetween);

        yield return StartCoroutine(FadeIn(bubble2));
        yield return new WaitForSeconds(delayBetween);

        yield return StartCoroutine(FadeIn(bubble3));
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