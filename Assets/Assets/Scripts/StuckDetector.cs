using System.Collections.Generic;
using UnityEngine;

public class StuckDetector : MonoBehaviour
{
    [Header("Stuck Detection Settings")]
    public float stuckThreshold = 0.5f; // Minimum distance to move to not be stuck
    public float detectionWindow = 5f; // Time window to check for stuck behavior
    public int maxPositionHistory = 20;

    [Header("Pattern Detection")]
    public float loopDetectionRadius = 2f;
    public int minLoopSize = 3;

    [Header("Debug Info")]
    public bool isCurrentlyStuck = false;
    public float timeStuck = 0f;
    public string stuckReason = "";

    // Internal tracking
    private List<Vector3> positionHistory;
    private List<float> timeHistory;
    private Vector3 lastSignificantPosition;
    private float lastSignificantMoveTime;
    private bool wasStuckLastFrame = false;

    // Pattern detection
    private List<Vector3> recentPath;
    private float lastPatternCheck = 0f;
    private const float PATTERN_CHECK_INTERVAL = 1f;

    private void Awake()
    {
        positionHistory = new List<Vector3>();
        timeHistory = new List<float>();
        recentPath = new List<Vector3>();
        lastSignificantPosition = transform.position;
        lastSignificantMoveTime = Time.time;
    }

    public void UpdatePosition(Vector3 currentPosition)
    {
        // Add to history
        positionHistory.Add(currentPosition);
        timeHistory.Add(Time.time);
        recentPath.Add(currentPosition);

        // Maintain history size
        if (positionHistory.Count > maxPositionHistory)
        {
            positionHistory.RemoveAt(0);
            timeHistory.RemoveAt(0);
        }

        if (recentPath.Count > maxPositionHistory)
        {
            recentPath.RemoveAt(0);
        }

        // Check if significant movement occurred
        float distanceFromLast = Vector3.Distance(currentPosition, lastSignificantPosition);
        if (distanceFromLast > stuckThreshold)
        {
            lastSignificantPosition = currentPosition;
            lastSignificantMoveTime = Time.time;

            // Reset stuck timer if we moved significantly
            if (wasStuckLastFrame)
            {
                timeStuck = 0f;
                isCurrentlyStuck = false;
                stuckReason = "";
                wasStuckLastFrame = false;
            }
        }

        // Check for stuck condition
        CheckStuckCondition();

        // Periodic pattern detection
        if (Time.time - lastPatternCheck > PATTERN_CHECK_INTERVAL)
        {
            DetectMovementPatterns();
            lastPatternCheck = Time.time;
        }
    }

    private void CheckStuckCondition()
    {
        float timeSinceLastMove = Time.time - lastSignificantMoveTime;

        if (timeSinceLastMove > detectionWindow)
        {
            if (!isCurrentlyStuck)
            {
                isCurrentlyStuck = true;
                stuckReason = "No significant movement";
                timeStuck = 0f;
            }

            timeStuck = timeSinceLastMove;
            wasStuckLastFrame = true;
        }
        else
        {
            // Check for oscillation (moving back and forth)
            if (DetectOscillation())
            {
                if (!isCurrentlyStuck)
                {
                    isCurrentlyStuck = true;
                    stuckReason = "Oscillating movement";
                    timeStuck = 0f;
                }
                timeStuck += Time.fixedDeltaTime;
                wasStuckLastFrame = true;
            }
        }
    }

    private bool DetectOscillation()
    {
        if (positionHistory.Count < 6) return false;

        // Check last 6 positions for back-and-forth pattern
        int historyCount = positionHistory.Count;
        Vector3 pos1 = positionHistory[historyCount - 6];
        Vector3 pos2 = positionHistory[historyCount - 4];
        Vector3 pos3 = positionHistory[historyCount - 2];
        Vector3 pos4 = positionHistory[historyCount - 1];

        // If positions alternate around the same spots
        bool oscillation1 = Vector3.Distance(pos1, pos3) < stuckThreshold;
        bool oscillation2 = Vector3.Distance(pos2, pos4) < stuckThreshold;

        return oscillation1 && oscillation2;
    }

    private void DetectMovementPatterns()
    {
        if (recentPath.Count < minLoopSize * 2) return;

        // Simple loop detection: check if current position is close to a previous position
        // and if the path between forms a small loop
        Vector3 currentPos = recentPath[recentPath.Count - 1];

        for (int i = recentPath.Count - minLoopSize - 1; i >= 0; i--)
        {
            float distanceToOldPos = Vector3.Distance(currentPos, recentPath[i]);

            if (distanceToOldPos < loopDetectionRadius)
            {
                // Potential loop detected
                int loopSize = recentPath.Count - 1 - i;

                if (loopSize >= minLoopSize)
                {
                    if (!isCurrentlyStuck)
                    {
                        isCurrentlyStuck = true;
                        stuckReason = $"Circular pattern detected (loop size: {loopSize})";
                        timeStuck = 0f;
                    }

                    timeStuck += PATTERN_CHECK_INTERVAL;
                    wasStuckLastFrame = true;
                    break;
                }
            }
        }
    }

    public bool IsStuck()
    {
        return isCurrentlyStuck;
    }

    public float GetTimeStuck()
    {
        return timeStuck;
    }

    public string GetStuckReason()
    {
        return stuckReason;
    }

    public Vector3 GetUnstuckSuggestion()
    {
        if (!isCurrentlyStuck) return Vector3.zero;

        // Suggest direction based on stuck reason
        switch (stuckReason)
        {
            case "No significant movement":
                // Suggest random direction
                return new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    0,
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized;

            case "Oscillating movement":
                // Suggest perpendicular direction to break oscillation
                if (positionHistory.Count >= 2)
                {
                    Vector3 lastDirection = (positionHistory[positionHistory.Count - 1] -
                                           positionHistory[positionHistory.Count - 2]).normalized;
                    return Vector3.Cross(lastDirection, Vector3.up).normalized;
                }
                break;

            default:
                if (stuckReason.Contains("Circular pattern"))
                {
                    // Break out of circular pattern with random direction
                    return new Vector3(
                        UnityEngine.Random.Range(-1f, 1f),
                        0,
                        UnityEngine.Random.Range(-1f, 1f)
                    ).normalized;
                }
                break;
        }

        return Vector3.forward; // Default suggestion
    }

    public void ResetStuckDetection()
    {
        isCurrentlyStuck = false;
        timeStuck = 0f;
        stuckReason = "";
        positionHistory.Clear();
        timeHistory.Clear();
        recentPath.Clear();
        lastSignificantPosition = transform.position;
        lastSignificantMoveTime = Time.time;
        wasStuckLastFrame = false;
    }

    // Get statistics for memory system
    public Dictionary<string, object> GetStuckStatistics()
    {
        return new Dictionary<string, object>
        {
            {"is_stuck", isCurrentlyStuck},
            {"time_stuck", timeStuck},
            {"stuck_reason", stuckReason},
            {"position_history_count", positionHistory.Count},
            {"average_speed", CalculateAverageSpeed()},
            {"movement_variance", CalculateMovementVariance()}
        };
    }

    private float CalculateAverageSpeed()
    {
        if (positionHistory.Count < 2 || timeHistory.Count < 2) return 0f;

        float totalDistance = 0f;
        float totalTime = timeHistory[timeHistory.Count - 1] - timeHistory[0];

        for (int i = 1; i < positionHistory.Count; i++)
        {
            totalDistance += Vector3.Distance(positionHistory[i], positionHistory[i - 1]);
        }

        return totalTime > 0 ? totalDistance / totalTime : 0f;
    }

    private float CalculateMovementVariance()
    {
        if (positionHistory.Count < 3) return 0f;

        // Calculate variance in movement directions
        List<Vector3> directions = new List<Vector3>();
        for (int i = 1; i < positionHistory.Count; i++)
        {
            Vector3 dir = (positionHistory[i] - positionHistory[i - 1]).normalized;
            if (dir.magnitude > 0.1f) directions.Add(dir);
        }

        if (directions.Count < 2) return 0f;

        // Calculate average direction
        Vector3 avgDirection = Vector3.zero;
        foreach (Vector3 dir in directions)
        {
            avgDirection += dir;
        }
        avgDirection /= directions.Count;

        // Calculate variance from average
        float variance = 0f;
        foreach (Vector3 dir in directions)
        {
            variance += Vector3.Distance(dir, avgDirection);
        }

        return variance / directions.Count;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (transform == null) return; // 👈 Evita el null reference

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);

        if (positionHistory.Count < 2)
        {
            return;
        }

        // Draw movement history
        Gizmos.color = isCurrentlyStuck ? Color.red : Color.blue;
        for (int i = 1; i < positionHistory.Count; i++)
        {
            Gizmos.DrawLine(positionHistory[i - 1], positionHistory[i]);
        }

        // Draw stuck detection radius
        if (isCurrentlyStuck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastSignificantPosition, stuckThreshold);
        }

        // Draw loop detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loopDetectionRadius);
    }

    // Context menu debug methods
    [ContextMenu("Force Stuck Detection")]
    public void ForceStuckDetection()
    {
        isCurrentlyStuck = true;
        stuckReason = "Manually triggered";
        timeStuck = detectionWindow;
        Debug.Log("Stuck detection manually triggered");
    }

    [ContextMenu("Reset Detection")]
    public void ForceReset()
    {
        ResetStuckDetection();
        Debug.Log("Stuck detection reset");
    }

    [ContextMenu("Print Statistics")]
    public void PrintStatistics()
    {
        var stats = GetStuckStatistics();
        Debug.Log("Stuck Detector Statistics:");
        foreach (var kvp in stats)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
}
