using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Una clase de datos simple para mantener el estado emocional.
/// No es un MonoBehaviour, por lo que puede ser creada e intercambiada fácilmente.
/// </summary>
[System.Serializable]
public class EmotionalState
{
    [Range(0, 100)] public float motivation = 50f;
    [Range(0, 100)] public float satisfaction = 50f;
    [Range(0, 100)] public float frustration = 0f;
    [Range(0, 100)] public float curiosity = 70f;

    public void UpdateEmotion(string emotion, float delta)
    {
        switch (emotion.ToLower())
        {
            case "motivation": motivation = Mathf.Clamp(motivation + delta, 0f, 100f); break;
            case "satisfaction": satisfaction = Mathf.Clamp(satisfaction + delta, 0f, 100f); break;
            case "frustration": frustration = Mathf.Clamp(frustration + delta, 0f, 100f); break;
            case "curiosity": curiosity = Mathf.Clamp(curiosity + delta, 0f, 100f); break;
        }
    }

    public EmotionalState Copy()
    {
        return new EmotionalState
        {
            motivation = this.motivation,
            satisfaction = this.satisfaction,
            frustration = this.frustration,
            curiosity = this.curiosity
        };
    }
}
