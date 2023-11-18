using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image fadePanel;
    public float fadeDuration = 2f;

    void Awake()
    {
        if (fadePanel == null)
        {
            fadePanel = GetComponent<Image>();
        }
    }

    public bool IsReady()
    {
        // Example check: ensure fadePanel is assigned
        return fadePanel != null;
    }    

    public void FadeIn()
    {
        StartCoroutine(DoFade(1, 0)); // Fade from opaque to transparent
    }

    public void FadeOut()
    {
        StartCoroutine(DoFade(0, 1)); // Fade from transparent to opaque
    }

    IEnumerator DoFade(float startAlpha, float endAlpha)
    {
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            Color newColor = new Color(0, 0, 0, alpha);
            fadePanel.color = newColor;
            yield return null;
        }
    }
}
