using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ForwardModel))]
public class MentalSimulator : MonoBehaviour
{
    private ForwardModel forwardModel;

    [Header("Simulation Parameters")]
    public int simulationDepth = 3;
    public float discountFactor = 0.95f;

    [Header("Memory Integration")]
    [Tooltip("Multiplier for how much a memory's strength contributes to predicted frustration.")]
    public float memoryDreadFactor = 50f;

    void Awake()
    {
        forwardModel = GetComponent<ForwardModel>();
    }

    public RouteForecast SimulateAction(Vector3 initialAction, EmotionalState currentEmotions, Vector3 currentPerceptualState, MemoryStore memoryStore, Transform agentTransform)
    {
        // Initialize variables for the simulation
        List<Vector3> path = new List<Vector3>();
        float totalSatisfaction = currentEmotions.satisfaction;
        float totalFrustration = currentEmotions.frustration;
        float totalTime = 0f;
        bool leadsToGoal = false;

        Vector3 simulatedState = currentPerceptualState;
        Vector3 simulatedPosition = agentTransform.position;
        Vector3 simulatedAction = initialAction;

        for (int i = 0; i < simulationDepth; i++)
        {
            Prediction stepPrediction = forwardModel.PredictOutcome(simulatedState, simulatedAction);

            // Query Episodic Memory with the predicted state
            EpisodicMemory retrievedMemory = memoryStore.FindMostRelevantMemory(simulatedPosition, stepPrediction.PredictedState);
            float memoryDread = 0f;
            if (retrievedMemory != null)
            {
                float similarity = retrievedMemory.GetSimilarity(simulatedPosition, stepPrediction.PredictedState);
                memoryDread = retrievedMemory.Strength * similarity * memoryDreadFactor;
            }

            // Accumulate Results into the new format
            totalSatisfaction += stepPrediction.PredictedReward * Mathf.Pow(discountFactor, i);
            totalFrustration += memoryDread; // Frustration comes from dread

            // Update simulated state for the next step
            simulatedState = stepPrediction.PredictedState;
            simulatedPosition += simulatedAction.normalized; // Rough estimation
            path.Add(simulatedPosition);
            totalTime += 1f; // Assuming each step takes 1 unit of time

            // Check if this step leads towards a goal (simplified)
            if (stepPrediction.PredictedReward > 0) leadsToGoal = true;
        }

        // Create the forecast object with the new constructor
        return new RouteForecast(path, totalSatisfaction, totalFrustration, totalTime, leadsToGoal);
    }
}
