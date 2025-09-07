
using UnityEngine;
using Unity.MLAgents.Sensors;

// NOTE: This controller is now robust to the key being optional.
[RequireComponent(typeof(ArtificialHumanAgent))]
public class CognitiveController : MonoBehaviour
{
    // --- Core References ---
    private ArtificialHumanAgent agentBody;

    // --- Goal Object References (passed from the body) ---
    [HideInInspector] public Transform key;
    [HideInInspector] public Transform goal;

    // --- Internal State ---
    private bool hasKey = false;
    // Public getter for other scripts to check the agent's state
    public bool HasKey() { return hasKey; }

    void Awake()
    {
        agentBody = GetComponent<ArtificialHumanAgent>();
    }

    public void OnEpisodeBegin()
    {
        // Reset internal state. If there is NO key in the scene,
        // we consider this condition met by default.
        this.hasKey = (key == null);
    }

    /// <summary>
    /// Provides the neural network with the information it needs to make a decision.
    /// </summary>
    public void CollectAgentObservations(VectorSensor sensor)
    {
        // Observation 1: Does the agent have the key? (1 for yes, 0 for no)
        sensor.AddObservation(hasKey);

        // Observation 2: How far is the agent from the key?
        // If the key doesn't exist, report distance as 0.
        float distanceToKey = 0f;
        if (key != null)
        {
            distanceToKey = Vector3.Distance(agentBody.transform.position, key.position);
        }
        sensor.AddObservation(distanceToKey);

        // Observation 3: How far is the agent from the goal?
        float distanceToGoal = 0f;
        if (goal != null)
        {
            distanceToGoal = Vector3.Distance(agentBody.transform.position, goal.position);
        }
        sensor.AddObservation(distanceToGoal);
    }

    /// <summary>
    /// Handles rewards and state changes when the agent enters a trigger zone.
    /// </summary>
    public void ReportTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            if (!hasKey)
            {
                hasKey = true;
                agentBody.AddAgentReward(1.0f); // Reward for getting the key
            }
        }

        if (other.CompareTag("Goal"))
        {
            if (hasKey)
            {
                agentBody.AddAgentReward(2.0f); // Win!
                agentBody.EndAgentEpisode();
            }
            else
            {
                agentBody.AddAgentReward(-0.5f); // Lose!
                agentBody.EndAgentEpisode();
            }
        }
    }
}
