using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an object that has been detected by the agent's sensors.
/// </summary>
public class PerceivedObject
{
    public Transform transform;
    public string tag;

    public PerceivedObject(Transform t)
    {
        transform = t;
        tag = t.tag;
    }
}

/// <summary>
/// REFACTORED: This is now an active vision system that uses raycasting to "see"
/// objects and other agents in the environment.
/// </summary>
public class PerceptionSystem : MonoBehaviour
{
    [Header("Vision Parameters")]
    public float viewRadius = 15f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public int raycastCount = 10;
    public LayerMask targetLayers;
    public LayerMask obstacleLayers;

    private List<PerceivedObject> perceivedObjects = new List<PerceivedObject>();
    private List<ArtificialHumanAgent> perceivedAgents = new List<ArtificialHumanAgent>();

    void FixedUpdate()
    {
        CastVisionRays();
    }

    private void CastVisionRays()
    {
        perceivedObjects.Clear();
        perceivedAgents.Clear();

        float stepAngle = viewAngle / raycastCount;
        float startAngle = -viewAngle / 2;

        for (int i = 0; i <= raycastCount; i++)
        {
            float angle = startAngle + stepAngle * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, viewRadius, targetLayers))
            {
                Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, hit.transform.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayers))
                {
                    // Check if it's an agent or an object
                    ArtificialHumanAgent agent = hit.transform.GetComponent<ArtificialHumanAgent>();
                    if (agent != null && agent != this.GetComponent<ArtificialHumanAgent>()) // Don't perceive self
                    {
                        if (!perceivedAgents.Contains(agent)) perceivedAgents.Add(agent);
                    }
                    else
                    {
                        if (!perceivedObjects.Any(o => o.transform == hit.transform)) perceivedObjects.Add(new PerceivedObject(hit.transform));
                    }
                }
            }
        }
    }

    public List<PerceivedObject> GetPerceivedObjects()
    {
        perceivedObjects.RemoveAll(o => o.transform == null);
        return new List<PerceivedObject>(perceivedObjects);
    }

    public List<ArtificialHumanAgent> GetPerceivedAgents()
    {
        perceivedAgents.RemoveAll(a => a == null);
        return new List<ArtificialHumanAgent>(perceivedAgents);
    }

    public bool IsVisible(GameObject obj)
    {
        if (obj == null) return false;
        return perceivedObjects.Any(o => o.transform == obj.transform) || perceivedAgents.Any(a => a.gameObject == obj);
    }

    public void Clear()
    {
        perceivedObjects.Clear();
        perceivedAgents.Clear();
    }
}
