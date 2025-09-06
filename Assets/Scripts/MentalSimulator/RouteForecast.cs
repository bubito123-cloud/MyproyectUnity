using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a simulated future path and its expected outcomes.
/// </summary>
public class RouteForecast
{
    public List<Vector3> path;
    public float expectedSatisfaction;
    public float expectedFrustration;
    public float estimatedTime;
    public bool leadsToGoal;

    public RouteForecast(List<Vector3> path, float satisfaction, float frustration, float time, bool leadsToGoal)
    {
        this.path = path;
        this.expectedSatisfaction = satisfaction;
        this.expectedFrustration = frustration;
        this.estimatedTime = time;
        this.leadsToGoal = leadsToGoal;
    }
}
