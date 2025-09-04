using UnityEngine;
using Unity.MLAgents.Sensors;

/// <summary>
/// Recopila toda la informaci�n (observaciones) que el agente necesita para tomar decisiones.
/// </summary>
public class PerceptionSystem : MonoBehaviour
{
    public Transform goalTarget;
    public float maxObservationDistance = 20f;

    public void CollectBasicObservations(VectorSensor sensor, Transform agentTransform, Rigidbody agentRb, EmotionalState emotions, bool isStuck)
    {
        // Agent's local velocity (normalized) - 3 obs
        Vector3 localVelocity = agentTransform.InverseTransformDirection(agentRb.linearVelocity);
        sensor.AddObservation(Mathf.Clamp(localVelocity.x / 5f, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(localVelocity.y / 5f, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(localVelocity.z / 5f, -1f, 1f));

        // Agent's forward direction - 3 obs
        sensor.AddObservation(agentTransform.forward);

        // Goal information (relative and normalized) - 4 obs
        if (goalTarget != null)
        {
            Vector3 toGoal = goalTarget.position - agentTransform.position;
            sensor.AddObservation(agentTransform.InverseTransformDirection(toGoal.normalized));
            sensor.AddObservation(Mathf.Clamp01(toGoal.magnitude / maxObservationDistance));
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(1f); // Max distance if no goal
        }

        // Emotional state (normalized 0-1) - 4 obs
        sensor.AddObservation(emotions.motivation / 100f);
        sensor.AddObservation(emotions.satisfaction / 100f);
        sensor.AddObservation(emotions.frustration / 100f);
        sensor.AddObservation(emotions.curiosity / 100f);

        // Stuck state - 1 obs
        sensor.AddObservation(isStuck ? 1f : 0f);

        // Total: 15 observations
    }

    public float GetDistanceToGoal(Transform agentTransform)
    {
        if (goalTarget == null) return float.MaxValue;
        return Vector3.Distance(agentTransform.position, goalTarget.position);
    }

    public Vector3 GetDirectionToGoal(Transform agentTransform)
    {
        if (goalTarget == null) return Vector3.zero;
        return (goalTarget.position - agentTransform.position).normalized;
    }
}
