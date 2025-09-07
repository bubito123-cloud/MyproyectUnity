using UnityEngine;
using TMPro;
using System.Collections;

// The filename MUST be EmotionalDisplay.cs
public class EmotionalDisplay : MonoBehaviour
{
    [Header("Target Agent")]
    public EmotionalCore agentEmotionalCore; // Assign the agent's EmotionalCore in the inspector

    [Header("UI References")]
    public TextMeshProUGUI satisfactionText;
    public TextMeshProUGUI frustrationText;
    public TextMeshProUGUI narrationText; // Renamed from displayText for clarity

    [Header("Animation Settings")]
    public float charactersPerSecond = 50f;
    public float lingerTime = 3f;

    private Coroutine currentDisplayCoroutine;

    void Update()
    {
        // Continuously update the emotional state display
        if (agentEmotionalCore != null)
        {
            EmotionalState currentState = agentEmotionalCore.GetCurrentState();
            if (satisfactionText != null)
            {
                satisfactionText.text = $"Satisfaction: {currentState.satisfaction:F1}";
            }
            if (frustrationText != null)
            {
                frustrationText.text = $"Frustration: {currentState.frustration:F1}";
            }
        }
    }

    /// <summary>
    /// Public method that the NarratorEngine can call.
    /// It starts the text animation process for narration.
    /// </summary>
    public void ShowNarration(string textToShow)
    {
        if (narrationText == null) return;

        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }
        currentDisplayCoroutine = StartCoroutine(AnimateNarration(textToShow));
    }

    private IEnumerator AnimateNarration(string text)
    {
        narrationText.text = "";
        float timePerChar = 1f / charactersPerSecond;

        foreach (char c in text)
        {
            narrationText.text += c;
            yield return new WaitForSeconds(timePerChar);
        }

        yield return new WaitForSeconds(lingerTime);

        narrationText.text = ""; // Clear text after linger time
    }
}
