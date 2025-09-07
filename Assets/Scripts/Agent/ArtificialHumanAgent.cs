
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors; // We need this to use the VectorSensor

// NOTE: This body is now simpler. Observations are handled by the sensor.
[RequireComponent(typeof(CognitiveController))]
public class ArtificialHumanAgent : Agent
{
    // --- References to the scene ---
    public Transform key;
    public Transform goal;

    // --- Internal Components ---
    private CognitiveController cognitiveController;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    // Awake is called when the script instance is being loaded.
    // We use this to initialize components to ensure they are ready
    // before other methods like CollectObservations are called.
    public override void Initialize()
    {
        cognitiveController = GetComponent<CognitiveController>();
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        // Pass scene references to the cognitive controller
        cognitiveController.key = this.key;
    }

    public override void OnEpisodeBegin()
    {
        cognitiveController.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(cognitiveController.HasKey() ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-1f / MaxStep);

        int action = actions.DiscreteActions[0];

        // --- DEBUG: Let's see what the agent is thinking! ---
        // This will print the agent's state and its chosen action to the Unity Console.
        Debug.Log($"Has Key: {cognitiveController.HasKey()} -> Action Chosen: {action}");

        Transform target = null;

        // Action 0: GoToKey
        if (action == 0)
        {
            if (key != null)
            {
                target = key;
            }
        }
        // Action 1: GoToGoal
        else if (action == 1)
        {
            target = goal;
        }

        if (target != null)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        cognitiveController.ReportTriggerEnter(other);
    }

    // --- Public methods for external components ---
    public void AddAgentReward(float value) { AddReward(value); }
    public void EndAgentEpisode() { EndEpisode(); }
}
