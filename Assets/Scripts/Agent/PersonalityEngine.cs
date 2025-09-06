using UnityEngine;
using System.Linq;

public class PersonalityEngine : MonoBehaviour
{
    private AgentGenome genome;

    public void Initialize(AgentGenome agentGenome)
    {
        this.genome = agentGenome;
    }

    public Vector3 MakeDecision(Transform agentTransform, PerceptionSystem perception, EmotionalCore emotions, Vector3 perceptualState, MemoryStore memory, SubGoal currentSubGoal)
    {
        if (currentSubGoal == null) return Vector3.zero;

        Vector3 direction = (currentSubGoal.position - agentTransform.position).normalized;

        // --- Genetic Influence on Decision Making ---

        // 1. Risk Aversion: How much does potential danger affect the decision?
        // CORRECTED: Removed argument from GetRecentTraumas call
        var traumas = memory.GetRecentTraumas();
        if (traumas.Any())
        {
            // CORRECTED: Changed t.position to t.Position
            float dangerProximity = traumas.Min(t => Vector3.Distance(agentTransform.position, t.Position));
            if (dangerProximity < 10f)
            {
                // High riskAversion makes the agent more hesitant when near danger.
                float hesitationFactor = 1.0f - (genome.riskAversion * (1.0f - (dangerProximity / 10f)));
                direction *= hesitationFactor;
            }
        }

        // 2. Social Orientation: How much does the presence of others affect the decision?
        var otherAgents = perception.GetPerceivedAgents();
        if (otherAgents.Any() && currentSubGoal.type != SubGoalType.GoToExit)
        {
            Vector3 socialVector = Vector3.zero;
            if (genome.socialOrientation > 0.7f) // High social orientation: move towards others
            {
                socialVector = (otherAgents.First().transform.position - agentTransform.position).normalized;
            }
            else if (genome.socialOrientation < 0.3f) // Low social orientation: move away from others
            {
                socialVector = (agentTransform.position - otherAgents.First().transform.position).normalized;
            }
            // The genetic trait blends the goal direction with the social direction.
            direction = Vector3.Lerp(direction, socialVector, genome.socialOrientation - 0.5f).normalized;
        }

        // 3. Curiosity: How much does the agent get distracted by new things?
        // (This could be expanded to influence the DeliberativePlanner to generate more Explore goals)

        // --- Emotional Influence ---
        float speed = 5.0f;
        if (emotions.frustration > 70)
        {
            speed *= 1.5f; // Act more erratically
        }
        if (emotions.anxiety > 50)
        {
            speed *= 0.5f; // Act more cautiously
        }

        return direction * speed;
    }
}
