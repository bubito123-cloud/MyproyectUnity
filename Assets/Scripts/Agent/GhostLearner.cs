using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implements a DAgger-style continuous learning algorithm.
/// It compares the agent's actions to an expert (the Pathfinder) and periodically forces
/// the agent to imitate the expert, while also providing intrinsic rewards for mimicry.
/// </summary>
[RequireComponent(typeof(Pathfinder))]
public class GhostLearner : MonoBehaviour
{
    [Header("DAgger Parameters")]
    [Tooltip("How often (in seconds) to check if imitation is needed.")]
    public float imitationCheckInterval = 10f;
    [Tooltip("The average discrepancy required to trigger an imitation session.")]
    public float discrepancyThresholdForImitation = 45f; // degrees
    [Tooltip("How long an imitation session lasts (in seconds).")]
    public float imitationDuration = 2.0f;
    [Tooltip("Amount of random noise added during imitation to encourage exploration.")]
    public float imitationNoiseFactor = 0.15f;

    [Header("Intrinsic Rewards")]
    [Tooltip("Maximum reward given for perfectly matching the expert's action.")]
    public float maxMimicryReward = 0.05f;

    private Pathfinder expertPathfinder;
    private List<float> discrepancyLog = new List<float>();
    private float lastImitationCheckTime;
    private float imitationUntilTime = -1f;

    public bool IsImitating => Time.time < imitationUntilTime;

    void Awake()
    {
        expertPathfinder = GetComponent<Pathfinder>();
    }

    void Start()
    {
        lastImitationCheckTime = Time.time;
    }

    /// <summary>
    /// Calculates the discrepancy and decides if imitation is needed.
    /// Returns the expert's action if currently imitating.
    /// </summary>
    public Vector3? GetExpertAction(Vector3 agentAction, Transform agentTransform)
    {
        Vector3 expertAction = expertPathfinder.GetOptimalPathDirection();
        float discrepancy = Vector3.Angle(agentTransform.TransformDirection(agentAction), expertAction);
        discrepancyLog.Add(discrepancy);

        // Check if we need to start imitating
        if (Time.time - lastImitationCheckTime > imitationCheckInterval)
        {
            float averageDiscrepancy = discrepancyLog.Any() ? discrepancyLog.Average() : 0;
            if (averageDiscrepancy > discrepancyThresholdForImitation)
            {
                Debug.Log($"<color=orange>[DAgger] High discrepancy ({averageDiscrepancy:F1}°). Forcing imitation.</color>");
                imitationUntilTime = Time.time + imitationDuration;
            }
            discrepancyLog.Clear();
            lastImitationCheckTime = Time.time;
        }

        if (IsImitating)
        {
            Vector3 noisyExpertAction = expertAction + Random.insideUnitSphere * imitationNoiseFactor;
            return agentTransform.InverseTransformDirection(noisyExpertAction.normalized);
        }

        return null; // Not imitating, agent is free to choose
    }

    /// <summary>
    /// Calculates an intrinsic reward based on how well the agent is mimicking the expert.
    /// </summary>
    public float GetMimicryReward(Vector3 agentAction, Transform agentTransform)
    {
        Vector3 expertAction = expertPathfinder.GetOptimalPathDirection();
        float angle = Vector3.Angle(agentTransform.TransformDirection(agentAction), expertAction);
        // Reward is high when the angle is small (max reward at 0 degrees, 0 reward at 90+ degrees)
        float reward = maxMimicryReward * Mathf.Max(0, 1 - (angle / 90f));
        return reward;
    }
}
