using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A single tuple of experience, capturing a state-action-outcome sequence.
/// This is the fundamental unit of learning for the Forward Model.
/// </summary>
public struct Experience
{
    public Vector3 PerceptualState; // Simplified state representation
    public Vector3 Action;          // The action taken
    public Vector3 NextPerceptualState; // The resulting state
    public float Reward;            // The reward received
    public EmotionalState EmotionDelta; // How emotions changed
}

/// <summary>
/// A prediction result from the Forward Model, including uncertainty.
/// </summary>
public struct Prediction
{
    public Vector3 PredictedState; // Average predicted next state
    public float PredictedReward;
    public EmotionalState PredictedEmotionDelta;
    public float Uncertainty; // Variance in predictions, a measure of confidence
}

/// <summary>
/// The agent's internal model of the world. It learns the consequences of actions
/// by recording experiences and using them to predict future outcomes.
/// </summary>
public class ForwardModel : MonoBehaviour
{
    [Header("Learning Parameters")]
    [Tooltip("Maximum number of experiences to store.")]
    public int experienceCapacity = 2000;
    [Tooltip("How many similar experiences to use for making a prediction.")]
    public int kNearestNeighbors = 10;

    private List<Experience> experienceDatabase = new List<Experience>();

    /// <summary>
    /// Adds a real experience to the database, allowing the model to learn from it.
    /// </summary>
    public void AddExperience(Experience exp)
    {
        if (experienceDatabase.Count >= experienceCapacity)
        {
            experienceDatabase.RemoveAt(0); // Prune oldest experience
        }
        experienceDatabase.Add(exp);
    }

    /// <summary>
    /// Predicts the outcome of taking a specific action in a given state.
    /// </summary>
    public Prediction PredictOutcome(Vector3 currentState, Vector3 action)
    {
        if (experienceDatabase.Count < kNearestNeighbors) return new Prediction { Uncertainty = 1f }; // Not enough data

        // Find the k most similar past experiences
        var neighbors = experienceDatabase
            .OrderBy(exp => Vector3.Distance(exp.PerceptualState, currentState) + Vector3.Distance(exp.Action, action))
            .Take(kNearestNeighbors)
            .ToList();

        if (neighbors.Count == 0) return new Prediction { Uncertainty = 1f };

        // --- Calculate Average Predicted Outcome ---
        Vector3 avgNextState = Vector3.zero;
        float avgReward = 0;
        // Emotional deltas need to be averaged field by field
        float avgFrustration = 0, avgSatisfaction = 0; // etc.

        foreach (var neighbor in neighbors)
        {
            avgNextState += neighbor.NextPerceptualState;
            avgReward += neighbor.Reward;
            avgFrustration += neighbor.EmotionDelta.frustration;
            avgSatisfaction += neighbor.EmotionDelta.satisfaction;
        }

        var avgEmotionDelta = new EmotionalState
        {
            frustration = avgFrustration / neighbors.Count,
            satisfaction = avgSatisfaction / neighbors.Count
        };
        
        Prediction prediction = new Prediction
        {
            PredictedState = avgNextState / neighbors.Count,
            PredictedReward = avgReward / neighbors.Count,
            PredictedEmotionDelta = avgEmotionDelta
        };

        // --- Calculate Uncertainty (Variance) ---
        float stateVariance = 0;
        foreach (var neighbor in neighbors)
        {
            stateVariance += Vector3.SqrMagnitude(neighbor.NextPerceptualState - prediction.PredictedState);
        }
        prediction.Uncertainty = Mathf.Clamp01(stateVariance / neighbors.Count);

        return prediction;
    }
}
