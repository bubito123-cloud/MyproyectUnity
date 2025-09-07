using UnityEngine;

/// <summary>
/// A data class to hold the agent's emotional state, including base emotions and emergent meta-emotions.
/// This version includes the necessary constructors and utility methods for the cognitive architecture.
/// </summary>
[System.Serializable]
public class EmotionalState
{
    // --- Base Emotions ---
    [Header("Base Emotions")]
    [Range(0, 100)] public float satisfaction = 50f;
    [Range(0, 100)] public float frustration = 0f;
    [Range(0, 100)] public float curiosity = 0f;
    [Range(0, 100)] public float motivation = 50f; // Added from your version

    // --- Meta-Emotions ---
    [Header("Meta-Emotions (Emergent)")]
    [Range(0, 100)] public float anxiety = 0f;
    [Range(0, 100)] public float focus = 0f;
    [Range(0, 100)] public float composure = 50f;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public EmotionalState() { }

    /// <summary>
    /// Copy constructor to create a snapshot of another state.
    /// </summary>
    public EmotionalState(EmotionalState other)
    {
        if (other != null)
        {
            this.satisfaction = other.satisfaction;
            this.frustration = other.frustration;
            this.curiosity = other.curiosity;
            this.motivation = other.motivation;
            this.anxiety = other.anxiety;
            this.focus = other.focus;
            this.composure = other.composure;
        }
    }

    /// <summary>
    /// Calculates the difference between two states.
    /// </summary>
    public static EmotionalState GetDifference(EmotionalState newState, EmotionalState oldState)
    {
        EmotionalState difference = new EmotionalState();
        if (newState != null && oldState != null)
        {
            difference.satisfaction = newState.satisfaction - oldState.satisfaction;
            difference.frustration = newState.frustration - oldState.frustration;
            difference.curiosity = newState.curiosity - oldState.curiosity;
            difference.motivation = newState.motivation - oldState.motivation;
        }
        return difference;
    }
}
