using UnityEngine;

/// <summary>
/// Manages the agent's emotions. It handles the decay of base emotions and calculates
/// emergent meta-emotions based on the current state.
/// </summary>
public class EmotionalCore : MonoBehaviour
{
    private AgentGenome genome;
    private EmotionalState currentState = new EmotionalState();
    private EmotionalState previousState;

    [Header("Emotional Dynamics")]
    [Tooltip("How quickly emotions return to their baseline (per second).")]
    [SerializeField] private float decayRate = 0.5f;

    public void Initialize(AgentGenome agentGenome)
    {
        this.genome = agentGenome;
        if (genome != null)
        {
            // CORRECTED: Uses 'curiosity' from your AgentGenome.cs
            currentState.curiosity = genome.curiosity * 100;
        }
        previousState = new EmotionalState(currentState);
    }

    void Update()
    {
        previousState = new EmotionalState(currentState);

        // Decay base emotions
        currentState.satisfaction = Mathf.MoveTowards(currentState.satisfaction, 50f, decayRate * Time.deltaTime);
        currentState.frustration = Mathf.MoveTowards(currentState.frustration, 0f, decayRate * Time.deltaTime);
        currentState.motivation = Mathf.MoveTowards(currentState.motivation, 50f, decayRate * Time.deltaTime);
        currentState.composure = Mathf.MoveTowards(currentState.composure, 50f, decayRate * Time.deltaTime);
        if (genome != null)
        {
            // CORRECTED: Uses 'curiosity' from your AgentGenome.cs
            currentState.curiosity = Mathf.MoveTowards(currentState.curiosity, genome.curiosity * 100, decayRate * Time.deltaTime);
        }

        UpdateMetaEmotions();
    }

    private void UpdateMetaEmotions()
    {
        float satisfactionInverse = 1 - (currentState.satisfaction / 100f);
        currentState.anxiety = Mathf.Clamp(currentState.frustration * satisfactionInverse, 0, 100);

        float frustrationInverse = 1 - (currentState.frustration / 100f);
        currentState.focus = Mathf.Clamp(currentState.motivation * frustrationInverse, 0, 100);
    }

    public EmotionalState GetCurrentState()
    {
        return currentState;
    }

    public EmotionalState GetEmotionalChange()
    {
        if (previousState == null)
        {
            previousState = new EmotionalState(currentState);
        }
        return EmotionalState.GetDifference(currentState, previousState);
    }

    public void TriggerEmotionalEvent(string eventType, float intensity)
    {
        switch (eventType.ToLower())
        {
            case "satisfaction":
                currentState.satisfaction += intensity;
                break;
            case "frustration":
                currentState.frustration += intensity;
                break;
            case "curiosity":
                currentState.curiosity += intensity;
                break;
            case "motivation":
                currentState.motivation += intensity;
                break;
        }
        currentState.satisfaction = Mathf.Clamp(currentState.satisfaction, 0, 100);
        currentState.frustration = Mathf.Clamp(currentState.frustration, 0, 100);
        currentState.curiosity = Mathf.Clamp(currentState.curiosity, 0, 100);
        currentState.motivation = Mathf.Clamp(currentState.motivation, 0, 100);
    }
}
