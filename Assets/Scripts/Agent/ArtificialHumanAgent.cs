using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

// NOTE: This body is now a simple interface between the ML-Agents plugin and the CognitiveController.
// It holds no logic of its own and purely executes commands or passes data.
[RequireComponent(typeof(CognitiveController), typeof(UnityEngine.AI.NavMeshAgent))]
public class ArtificialHumanAgent : Agent
{
    // --- All references are now managed by the CognitiveController ---

    // --- Internal Components ---
    private CognitiveController cognitiveController;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    // We use Initialize to get references to components.
    public override void Initialize()
    {
        cognitiveController = GetComponent<CognitiveController>();
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public override void OnEpisodeBegin()
    {
        cognitiveController.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // The CognitiveController is now responsible for collecting all observations.
        cognitiveController.CollectAgentObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // The CognitiveController is now responsible for interpreting actions.
        AddReward(-1f / MaxStep); // A small penalty for every step to encourage efficiency.
        cognitiveController.ProcessActions(actions);
    }

    // This now represents a physical touch/interaction with an object.
    void OnTriggerEnter(Collider other)
    {
        cognitiveController.ReportTouch(other);
    }

    void OnTriggerExit(Collider other)
    {
        cognitiveController.ReportTouchExit(other);
    }

    // --- Public methods for the CognitiveController to command the agent's body ---
    public void AddAgentReward(float value) { AddReward(value); }
    public void EndAgentEpisode() { EndEpisode(); }
    public void SetNavMeshDestination(Vector3 destination)
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(destination);
        }
    }
}
