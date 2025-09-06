using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text; // Using StringBuilder for efficient string concatenation

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

    public override string ToString()
    {
        return $"Goal: {type}, Priority: {priority}, Position: {position}";
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

    public SubGoal CreateNewSubGoal(AgentGenome genome, bool hasKey, Transform agentTransform)
    {
        var potentialGoals = new List<SubGoal>();

        // --- ENHANCED LOGGING ---
        var logBuilder = new StringBuilder();
        logBuilder.AppendLine($"<color=orange>[Planner] Starting deliberation for agent {gameObject.GetInstanceID()}.</color>");

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
        if (criticalGoal != null)
        {
            logBuilder.AppendLine($"<color=red>Critical threat detected! Overriding all other goals to avoid '{criticalGoal}'.</color>");
            Debug.Log(logBuilder.ToString());
            return criticalGoal;
        }

        // 2. High-Priority Social Goals: Approach other agents if social
        if (perceivedAgents.Any() && genome.socialOrientation > 0.7f)
        {
            var targetAgent = perceivedAgents.First();
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

        // --- ENHANCED LOGGING ---
        logBuilder.AppendLine("Potential goals considered:");
        foreach (var goal in potentialGoals)
        {
            logBuilder.AppendLine($"- {goal}");
        }

        // 6. ULTIMATE FALLBACK
        if (!potentialGoals.Any())
        {
            logBuilder.AppendLine("<color=yellow>No specific goals found. Creating fallback goal.</color>");
            Vector3 fallbackPosition = agentTransform.position + agentTransform.forward * 10f;
            potentialGoals.Add(new SubGoal(fallbackPosition, SubGoalType.Explore, SubGoalPriority.Trivial));
        }

        var chosenGoal = potentialGoals.OrderByDescending(g => g.priority).FirstOrDefault();
        logBuilder.AppendLine($"<color=cyan>Chosen Goal: {chosenGoal}</color>");
        Debug.Log(logBuilder.ToString());

        return chosenGoal;
    }
}
