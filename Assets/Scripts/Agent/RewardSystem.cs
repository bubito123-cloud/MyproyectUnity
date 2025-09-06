using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Manages the agent's reward signals for the ML-Agents training process.
/// This version is refactored for clarity and configurability from the Unity Inspector.
/// </summary>
public class RewardSystem : MonoBehaviour
{
    [Header("Primary Rewards & Penalties")]
    [Tooltip("The large, positive reward for successfully reaching the goal.")]
    public float goalReachedReward = 1.0f;
    
    [Tooltip("A small, constant penalty applied every step to encourage speed. Should be negative.")]
    public float timePenalty = -0.001f;

    [Header("Distance-Based Rewards")]
    [Tooltip("Multiplier for the reward given for getting closer to the goal. Positive value incentivizes closing the distance.")]
    public float distanceRewardMultiplier = 0.1f;

    [Header("Emotional State Modifiers")]
    [Tooltip("Multiplier for the penalty based on frustration level. Should be negative.")]
    public float frustrationPenaltyMultiplier = -0.01f;
    
    [Tooltip("Multiplier for the reward based on satisfaction level. Should be positive.")]
    public float satisfactionRewardMultiplier = 0.005f;

    [Tooltip("Small reward for any movement to discourage standing still. Based on curiosity.")]
    public float curiosityReward = 0.0001f;

    // Internal state for tracking changes
    private float previousDistanceToGoal = float.MaxValue;

    /// <summary>
    /// Initializes the reward system for a new episode.
    /// </summary>
    public void Initialize(Transform agentTransform, PerceptionSystem perception)
    {
        if (perception.goalTarget != null)
        {
            previousDistanceToGoal = Vector3.Distance(agentTransform.position, perception.goalTarget.position);
        }
        else
        {
            previousDistanceToGoal = float.MaxValue;
        }
    }

    /// <summary>
    /// Calculates and applies all relevant rewards for the current agent step.
    /// </summary>
    public void CalculateRewards(Agent agent, Transform agentTransform, PerceptionSystem perception, EmotionalState emotions, bool reachedGoal)
    {
        // --- Primary Goal Reward --- 
        if (reachedGoal)
        {
            agent.AddReward(goalReachedReward);
            // No need to continue calculating other rewards if the episode is ending.
            return;
        }

        // --- Step Penalties and Incentives ---
        // 1. Time Penalty: Penalize the agent for taking too long.
        agent.AddReward(timePenalty);

        // --- Distance Reward: Reward or penalize based on progress towards the goal ---
        if (perception.goalTarget != null)
        {
            float currentDistance = Vector3.Distance(agentTransform.position, perception.goalTarget.position);
            float distanceDelta = previousDistanceToGoal - currentDistance;
            // Only apply reward if distanceDelta is meaningful
            if (Mathf.Abs(distanceDelta) > 0.01f) 
            { 
                agent.AddReward(distanceDelta * distanceRewardMultiplier);
            }
            previousDistanceToGoal = currentDistance;
        }

        // --- Emotional Rewards/Penalties: Shape behavior by linking emotions to outcomes ---
        // 1. Frustration Penalty: Penalize the agent for being frustrated.
        if (emotions.frustration > 1f) // Don't penalize for tiny amounts of frustration
        {
            agent.AddReward(emotions.frustration / 100f * frustrationPenaltyMultiplier);
        }

        // 2. Satisfaction Reward: Reward the agent for feeling satisfied.
        if (emotions.satisfaction > 1f)
        {
            agent.AddReward(emotions.satisfaction / 100f * satisfactionRewardMultiplier);
        }
        
        // 3. Curiosity Reward: A small nudge to encourage exploration if curious.
        if (emotions.curiosity > 10f)
        {
            agent.AddReward(curiosityReward);
        }
    }
}
