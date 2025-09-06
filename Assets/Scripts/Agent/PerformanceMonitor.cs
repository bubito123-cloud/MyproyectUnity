using UnityEngine;

/// <summary>
/// Observes and records an agent's performance during an episode.
/// It provides a final score to evaluate the agent's "fitness".
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("Performance Metrics")]
    public float timeElapsed = 0f;
    public int traumasSuffered = 0;
    public float totalSatisfaction = 0f;
    public float totalFrustration = 0f;
    public bool goalAchieved = false;

    private bool isRunning = true;

    void Update()
    {
        if (isRunning)
        {
            timeElapsed += Time.deltaTime;
        }
    }

    public void LogTrauma()
    {
        traumasSuffered++;
    }

    public void LogEmotionalState(float satisfaction, float frustration)
    {
        totalSatisfaction += satisfaction * Time.deltaTime;
        totalFrustration += frustration * Time.deltaTime;
    }

    public void LogGoalCompletion()
    {
        goalAchieved = true;
        StopMonitoring();
    }

    public void StopMonitoring()
    {
        isRunning = false;
    }

    /// <summary>
    /// Calculates the final fitness score based on all recorded metrics.
    /// Higher is better.
    /// </summary>
    public float CalculateFitnessScore()
    {
        float score = 0;

        // Huge reward for achieving the goal
        if (goalAchieved) score += 1000f;

        // Penalize for time taken (the faster, the better)
        score -= timeElapsed;

        // Heavily penalize for each trauma suffered
        score -= traumasSuffered * 200f;

        // Reward for positive emotional state
        float emotionalBalance = totalSatisfaction - totalFrustration;
        score += emotionalBalance * 10f;

        // Ensure score is not negative
        return Mathf.Max(0, score);
    }
}
