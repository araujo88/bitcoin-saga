using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public SpriteRenderer[] scenes;

    void Start()
    {
        // Disable all scenes at start
        foreach (var scene in scenes)
        {
            scene.enabled = true;  // Enable the SpriteRenderer to change its color.
            scene.color = new Color(scene.color.r, scene.color.g, scene.color.b, 0); // Set alpha to 0
        }

        StartCoroutine(StartDialoguesSequentially());
    }

    private IEnumerator StartDialoguesSequentially()
    {
        float fadeDuration = 1.0f; // Set the fade duration

        for (int i = 0; i < scenes.Length; i++)
        {
            // Fade in the current sprite
            yield return StartCoroutine(FadeIn(scenes[i], fadeDuration));

            // Start the dialogue
            dialogueSystem.StartDialogueFromExternal("Intro/dialogue" + i + ".json", null, null, 0.05f);

            // Now wait until the dialogue ends
            yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete);

            // Fade out the current sprite
            yield return StartCoroutine(FadeOut(scenes[i], fadeDuration));
        }

        SceneManager.LoadScene(1);
    }

    private IEnumerator FadeIn(SpriteRenderer spriteRenderer, float duration)
    {
        float currentTime = 0f;
        while (currentTime < duration)
        {
            float alpha = currentTime / duration;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            currentTime += Time.deltaTime;
            yield return null; // wait for the next frame
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1); // ensure full opacity
    }

    private IEnumerator FadeOut(SpriteRenderer spriteRenderer, float duration)
    {
        float currentTime = 0f;
        while (currentTime < duration)
        {
            float alpha = 1 - (currentTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            currentTime += Time.deltaTime;
            yield return null; // wait for the next frame
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0); // ensure fully transparent
    }
}
