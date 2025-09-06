using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Acts as an "imaginary expert" or "ghost" that always knows the optimal path.
/// It uses Unity's NavMesh system to provide a perfect reference direction for the agent to compare against.
/// Requires a baked NavMesh in the scene to function.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Pathfinder : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private Transform goalTarget;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updatePosition = false; // The Pathfinder doesn't move the agent, it only calculates paths
        navAgent.updateRotation = false;
        navAgent.speed = 0; // It has no speed of its own
    }

    /// <summary>
    /// Sets the goal for the pathfinder to calculate against.
    /// </summary>
    public void SetGoal(Transform target)
    {
        goalTarget = target;
    }

    /// <summary>
    /// Calculates and returns the optimal direction vector towards the current goal.
    /// </summary>
    public Vector3 GetOptimalPathDirection()
    {
        if (goalTarget == null || !navAgent.isOnNavMesh)
        {
            return Vector3.zero;
        }

        // Update the NavMeshAgent's internal position to match the actual agent
        navAgent.nextPosition = transform.position;
        
        // Calculate the path
        if (navAgent.SetDestination(goalTarget.position))
        {
            // The desired velocity is the first step on the optimal path
            return navAgent.desiredVelocity.normalized;
        }

        return Vector3.zero;
    }
}
