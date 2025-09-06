using UnityEngine;
using TMPro;
using System.Collections;

// The filename MUST be EmotionalDisplay.cs
public class EmotionalDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI displayText;

    [Header("Animation Settings")]
    public float charactersPerSecond = 50f;
    public float lingerTime = 3f;

    private Coroutine currentDisplayCoroutine;

    /// <summary>
    /// Public method that the NarratorEngine can call.
    /// It starts the text animation process.
    /// </summary>
    public void ShowText(string textToShow)
    {
        if (displayText == null) return;

        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }
        currentDisplayCoroutine = StartCoroutine(AnimateText(textToShow));
    }

    private IEnumerator AnimateText(string text)
    {
        displayText.text = "";
        float timePerChar = 1f / charactersPerSecond;

        foreach (char c in text)
        {
            displayText.text += c;
            yield return new WaitForSeconds(timePerChar);
        }

        yield return new WaitForSeconds(lingerTime);

        displayText.text = ""; // Clear text after linger time
    }
}
