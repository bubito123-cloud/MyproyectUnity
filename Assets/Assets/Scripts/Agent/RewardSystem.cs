using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;

/// <summary>
/// Centraliza toda la lógica de cálculo de recompensas para el agente.
/// </summary>
public class RewardSystem : MonoBehaviour
{
    [Header("Reward Settings")]
    public float approachRewardFactor = 0.1f;
    public float stepPenalty = -0.001f;
    public float goalReward = 1f;
    public float frustrationPenalty = 0.005f;
    public float explorationReward = 0.002f;

    private float lastDistanceToGoal = float.MaxValue;
    private Vector3 lastPosition;

    public void Initialize(Transform agentTransform, PerceptionSystem perception)
    {
        lastDistanceToGoal = perception.GetDistanceToGoal(agentTransform);
        lastPosition = agentTransform.position;
    }

    public void CalculateRewards(Agent agent, Transform agentTransform, PerceptionSystem perception, EmotionalState emotions, bool goalReached)
    {
        agent.AddReward(stepPenalty);

        float currentDistance = perception.GetDistanceToGoal(agentTransform);
        if (lastDistanceToGoal < float.MaxValue)
        {
            float distanceDelta = lastDistanceToGoal - currentDistance;
            agent.AddReward(distanceDelta * approachRewardFactor);
        }
        lastDistanceToGoal = currentDistance;

        agent.AddReward(-(emotions.frustration / 100f) * frustrationPenalty);

        if (Vector3.Distance(agentTransform.position, lastPosition) > 0.5f)
        {
            agent.AddReward((emotions.curiosity / 100f) * explorationReward);
        }
        lastPosition = agentTransform.position;

        if (goalReached)
        {
            agent.AddReward(goalReward);
            agent.EndEpisode();
        }
    }
}
