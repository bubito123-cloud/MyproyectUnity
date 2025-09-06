using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ADDED: New goal type for social interaction
public enum SubGoalType { Explore, GoToKey, GoToExit, ApproachConcept, AvoidConcept, ApproachAgent }
public enum SubGoalPriority { Trivial, Low, Medium, High, Critical }

public class SubGoal
{
    public Vector3 position;
    public SubGoalType type;
    public SubGoalPriority priority;

    public SubGoal(Vector3 pos, SubGoalType t, SubGoalPriority p)
    {
        position = pos;
        type = t;
        priority = p;
    }
}

[RequireComponent(typeof(PerceptionSystem), typeof(Conceptualizer))]
public class DeliberativePlanner : MonoBehaviour
{
    private PerceptionSystem perceptionSystem;
    private Conceptualizer conceptualizer;

    void Awake()
    {
        perceptionSystem = GetComponent<PerceptionSystem>();
        conceptualizer = GetComponent<Conceptualizer>();
    }

    // CORRECTED: Now accepts the agent's genome to make decisions based on personality.
    public SubGoal CreateNewSubGoal(AgentGenome genome, bool hasKey, Transform agentTransform)
    {
        var potentialGoals = new List<SubGoal>();

        var perceivedConcepts = perceptionSystem.GetPerceivedConcepts();
        var perceivedAgents = perceptionSystem.GetPerceivedAgents();

        // 1. Critical Goals: Avoid immediate danger
        foreach (var conceptTag in perceivedConcepts)
        {
            if (conceptualizer.knownConcepts.TryGetValue(conceptTag.ConceptName.ToUpper(), out var concept))
            {
                if (concept.IsDangerous)
                {
                    Vector3 avoidancePosition = agentTransform.position + (agentTransform.position - conceptTag.transform.position).normalized * 10f;
                    potentialGoals.Add(new SubGoal(avoidancePosition, SubGoalType.AvoidConcept, SubGoalPriority.Critical));
                }
            }
        }
        var criticalGoal = potentialGoals.FirstOrDefault(g => g.priority == SubGoalPriority.Critical);
        if (criticalGoal != null) return criticalGoal;

        // 2. High-Priority Social Goals: Approach other agents if social
        if (perceivedAgents.Any() && genome.socialOrientation > 0.7f)
        {
            var targetAgent = perceivedAgents.First(); // Simple: just approach the first one seen
            potentialGoals.Add(new SubGoal(targetAgent.transform.position, SubGoalType.ApproachAgent, SubGoalPriority.High));
        }

        // 3. High-Priority Conceptual Goals: Investigate positive concepts if curious
        if (perceivedConcepts.Any() && genome.curiosity > 0.6f)
        {
            foreach (var conceptTag in perceivedConcepts)
            {
                if (conceptualizer.knownConcepts.TryGetValue(conceptTag.ConceptName.ToUpper(), out var concept))
                {
                    if (concept.Valence > 0)
                    {
                        potentialGoals.Add(new SubGoal(conceptTag.transform.position, SubGoalType.ApproachConcept, SubGoalPriority.High));
                    }
                }
            }
        }

        // 4. Standard Goals (Placeholder)
        if (hasKey) { potentialGoals.Add(new SubGoal(Vector3.one * 100, SubGoalType.GoToExit, SubGoalPriority.Medium)); }

        // 5. Default Goal: Exploration
        Vector3? openArea = perceptionSystem.FindMostOpenArea(agentTransform);
        if (openArea.HasValue)
        {
            potentialGoals.Add(new SubGoal(openArea.Value, SubGoalType.Explore, SubGoalPriority.Low));
        }

        // 6. ULTIMATE FALLBACK: If there are STILL no goals, just move forward.
        if (!potentialGoals.Any())
        {
            Debug.Log("<color=yellow>[Planner] No specific goals found. Creating fallback goal.</color>");
            Vector3 fallbackPosition = agentTransform.position + agentTransform.forward * 10f;
            potentialGoals.Add(new SubGoal(fallbackPosition, SubGoalType.Explore, SubGoalPriority.Trivial));
        }

        // Return the highest priority goal from the list
        return potentialGoals.OrderByDescending(g => g.priority).FirstOrDefault();
    }
}
