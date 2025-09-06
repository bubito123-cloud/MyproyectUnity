using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PerceptionSystem : MonoBehaviour
{
    [Header("Perception Layers")]
    public LayerMask agentLayer;
    public LayerMask conceptLayer;
    public LayerMask wallLayer; // Layer for environmental obstacles

    public Transform goalTarget { get; set; }

    private List<ArtificialHumanAgent> perceivedAgents = new List<ArtificialHumanAgent>();
    private List<ConceptTag> perceivedConcepts = new List<ConceptTag>();

    public void UpdatePerception(Transform t, float socialDetectionRadius)
    {
        // --- Social Perception ---
        perceivedAgents.Clear();
        Collider[] agentColliders = Physics.OverlapSphere(t.position, socialDetectionRadius, agentLayer);
        foreach (var collider in agentColliders)
        {
            if (collider.transform == t) continue; // Don't perceive self
            ArtificialHumanAgent agent = collider.GetComponent<ArtificialHumanAgent>();
            if (agent != null) { perceivedAgents.Add(agent); }
        }

        // --- Conceptual Perception ---
        perceivedConcepts.Clear();
        Collider[] conceptColliders = Physics.OverlapSphere(t.position, socialDetectionRadius, conceptLayer);
        foreach (var collider in conceptColliders)
        {
            ConceptTag tag = collider.GetComponent<ConceptTag>();
            if (tag != null) { perceivedConcepts.Add(tag); }
        }
    }

    public List<ArtificialHumanAgent> GetPerceivedAgents() => perceivedAgents;
    public List<ConceptTag> GetPerceivedConcepts() => perceivedConcepts;

    /// <summary>
    /// Finds the most open direction for exploration by casting rays in a circle.
    /// </summary>
    /// <returns>A point in the most open direction, or null if no direction is found.</returns>
    public Vector3? FindMostOpenArea(Transform agentTransform)
    {
        int numRays = 36; // Cast 36 rays (every 10 degrees)
        float maxDistance = 0f;
        Vector3 bestDirection = Vector3.zero;
        float rayLength = 30f; // How far to check for obstacles

        for (int i = 0; i < numRays; i++)
        {
            float angle = i * (360f / numRays);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * agentTransform.forward;

            RaycastHit hit;
            float currentDistance;

            // Use wallLayer to only detect walls/obstacles
            if (Physics.Raycast(agentTransform.position, direction, out hit, rayLength, wallLayer))
            {
                currentDistance = hit.distance;
            }
            else
            {
                // If the ray doesn't hit anything, it's a very open direction
                currentDistance = rayLength;
            }

            if (currentDistance > maxDistance)
            {
                maxDistance = currentDistance;
                bestDirection = direction;
            }
        }

        if (maxDistance > 0)
        {
            // Return a point 10 units away in the best direction found
            return agentTransform.position + bestDirection * 10f;
        }

        return null;
    }
}
