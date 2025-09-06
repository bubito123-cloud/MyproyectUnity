using UnityEngine;

/// <summary>
/// A data class to hold the agent's emotional state, including base emotions and emergent meta-emotions.
/// </summary>
[System.Serializable]
public class EmotionalState
{
    // --- Base Emotions (Directly influenced by events) ---
    [Header("Base Emotions")]
    [Range(0, 100)] public float motivation = 50f;
    [Range(0, 100)] public float satisfaction = 50f;
    [Range(0, 100)] public float frustration = 0f;
    [Range(0, 100)] public float curiosity = 0f;

    // --- Meta-Emotions (Calculated from Base Emotions) ---
    [Header("Meta-Emotions (Emergent)")]
    [Tooltip("High anxiety leads to survival-focused, imprecise behavior. Arises from high frustration and low satisfaction.")]
    [Range(0, 100)] public float anxiety = 0f;
    
    [Tooltip("High focus leads to task-oriented, efficient behavior. Arises from high motivation and low frustration.")]
    [Range(0, 100)] public float focus = 0f;

    [Tooltip("Represents the agent's ability to remain calm and manage stress. Increased by successful regulation.")]
    [Range(0, 100)] public float composure = 50f;
}
