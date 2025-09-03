using UnityEngine;
using System.Collections.Generic;

// NOTA: Este es un MonoBehaviour para que pueda estar en el objeto y ser fácilmente accesible,
// pero su lógica principal no depende del ciclo de vida de Unity.
public class PersonalityEngine : MonoBehaviour
{
    [Header("Personality Development (Phase 1 - Dormant)")]
    public bool isActive = false;
    public float personalityInfluence = 0f;

    [Header("Basic Decision Making")]
    public float goalSeekingTendency = 0.7f;
    public float explorationTendency = 0.3f;

    // Estos se usarán en fases futuras
    // private Dictionary<string, float> preferences = new Dictionary<string, float>();
    // private List<string> personalityTraits = new List<string>();

    public Vector3 MakeDecision(Transform agentTransform, PerceptionSystem perception, EmotionalState emotions)
    {
        if (!isActive)
        {
            return MakeBasicDecision(agentTransform, perception, emotions);
        }
        // Las fases futuras añadirán lógica aquí
        return MakeBasicDecision(agentTransform, perception, emotions);
    }

    private Vector3 MakeBasicDecision(Transform agentTransform, PerceptionSystem perception, EmotionalState emotions)
    {
        Vector3 decision = Vector3.zero;

        Vector3 goalDirection = perception.GetDirectionToGoal(agentTransform);
        decision += goalDirection * goalSeekingTendency * (emotions.motivation / 100f);

        Vector3 explorationDirection = GetExplorationDirection(agentTransform);
        decision += explorationDirection * explorationTendency * (emotions.curiosity / 100f);

        return decision.normalized;
    }

    private Vector3 GetExplorationDirection(Transform agentTransform)
    {
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 forwardBias = agentTransform.forward * 0.3f;
        return (randomDirection + forwardBias).normalized;
    }
}
