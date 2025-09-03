using UnityEngine;

/// <summary>
/// Gestiona las emociones del agente con transiciones suaves (smoothing) y decaimiento natural.
/// </summary>
public class EmotionalCore : MonoBehaviour
{
    [Header("Emotional Settings")]
    public float emotionalSmoothingSpeed = 2f;
    public float naturalDecaySpeed = 0.5f;
    public EmotionalState currentEmotions = new EmotionalState();

    private EmotionalState targetEmotions = new EmotionalState();
    private EmotionalState baselineEmotions = new EmotionalState();

    void Start()
    {
        baselineEmotions = currentEmotions.Copy();
        targetEmotions = currentEmotions.Copy();
    }

    public void UpdateEmotionalState(float deltaTime)
    {
        // Smooth transition to target emotions
        currentEmotions.motivation = Mathf.Lerp(currentEmotions.motivation, targetEmotions.motivation, deltaTime * emotionalSmoothingSpeed);
        currentEmotions.satisfaction = Mathf.Lerp(currentEmotions.satisfaction, targetEmotions.satisfaction, deltaTime * emotionalSmoothingSpeed);
        currentEmotions.frustration = Mathf.Lerp(currentEmotions.frustration, targetEmotions.frustration, deltaTime * emotionalSmoothingSpeed);
        currentEmotions.curiosity = Mathf.Lerp(currentEmotions.curiosity, targetEmotions.curiosity, deltaTime * emotionalSmoothingSpeed);

        // Natural decay towards baseline
        ApplyNaturalDecay(deltaTime);
    }

    private void ApplyNaturalDecay(float deltaTime)
    {
        float decayFactor = deltaTime * naturalDecaySpeed;
        targetEmotions.motivation = Mathf.Lerp(targetEmotions.motivation, baselineEmotions.motivation, decayFactor * 0.1f);
        targetEmotions.satisfaction = Mathf.Lerp(targetEmotions.satisfaction, baselineEmotions.satisfaction, decayFactor * 0.2f);
        targetEmotions.frustration = Mathf.Lerp(targetEmotions.frustration, 0f, decayFactor * 0.3f); // Frustration always decays to 0
        targetEmotions.curiosity = Mathf.Lerp(targetEmotions.curiosity, baselineEmotions.curiosity, decayFactor * 0.05f);
    }

    public void TriggerEmotionalEvent(string emotionType, float intensity)
    {
        targetEmotions.UpdateEmotion(emotionType, intensity);
    }

    public void ProcessMovementEmotions(float movementIntensity, bool isStuck)
    {
        if (movementIntensity > 0.1f)
        {
            TriggerEmotionalEvent("motivation", 0.5f * Time.fixedDeltaTime);
        }

        if (isStuck)
        {
            TriggerEmotionalEvent("frustration", 10f * Time.fixedDeltaTime);
            TriggerEmotionalEvent("curiosity", 2f * Time.fixedDeltaTime);
        }
    }

    public EmotionalState GetCurrentEmotions()
    {
        return currentEmotions;
    }
}

