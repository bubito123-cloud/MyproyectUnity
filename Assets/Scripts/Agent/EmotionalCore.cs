using UnityEngine;
using System.Collections.Generic;

public class EmotionalCore : MonoBehaviour
{
    private AgentGenome genome;

    public float satisfaction = 50f;
    public float frustration = 0f;
    public float anxiety = 0f;
    public float curiosity = 0f;

    private Dictionary<string, float> emotionalImpacts = new Dictionary<string, float>();

    public void Initialize(AgentGenome agentGenome)
    {
        this.genome = agentGenome;
    }

    void Update()
    {
        DecayEmotions();
    }

    public void TriggerEmotionalEvent(string eventType, float intensity)
    {
        // The agent's genetic sensitivity amplifies the emotional impact.
        float finalIntensity = intensity * genome.emotionalSensitivity;

        switch (eventType)
        {
            case "satisfaction":
                satisfaction += finalIntensity;
                break;
            case "frustration":
                frustration += finalIntensity;
                break;
            case "anxiety":
                anxiety += finalIntensity;
                break;
            case "curiosity":
                curiosity += finalIntensity;
                break;
        }
        ClampEmotions();
    }

    private void DecayEmotions()
    {
        // The genetic decay rate determines how quickly emotions fade.
        float decay = genome.emotionalDecayRate * Time.deltaTime * 100f;

        satisfaction = Mathf.Lerp(satisfaction, 50f, decay);
        frustration = Mathf.Lerp(frustration, 0f, decay);
        anxiety = Mathf.Lerp(anxiety, 0f, decay);
        curiosity = Mathf.Lerp(curiosity, 0f, decay);
    }

    private void ClampEmotions()
    {
        satisfaction = Mathf.Clamp(satisfaction, 0, 100);
        frustration = Mathf.Clamp(frustration, 0, 100);
        anxiety = Mathf.Clamp(anxiety, 0, 100);
        curiosity = Mathf.Clamp(curiosity, 0, 100);
    }
}
