using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents the agent's opinion and history with another agent.
/// </summary>
public class Relationship
{
    public ArtificialHumanAgent Agent;
    public float Familiarity; // How well does the agent know them? (0-100)
    public float Affinity;    // Does the agent like or dislike them? (-100 to 100)

    public Relationship(ArtificialHumanAgent agent)
    {
        Agent = agent;
        Familiarity = 0f;
        Affinity = 0f; // Start neutral
    }
}

/// <summary>
/// Manages the agent's social relationships and opinions of other agents.
/// It forms judgments based on shared experiences.
/// </summary>
public class SocialEngine : MonoBehaviour
{
    public Dictionary<int, Relationship> knownAgents = new Dictionary<int, Relationship>();

    [Header("Social Dynamics")]
    [Tooltip("How quickly familiarity grows when near another agent.")]
    public float familiarityGainRate = 5f; 
    [Tooltip("How much positive/negative experiences with an agent affect affinity.")]
    public float affinityMultiplier = 10f;

    /// <summary>
    /// Updates relationships with all agents currently perceived.
    /// </summary>
    public void UpdateSocialModel(List<ArtificialHumanAgent> perceivedAgents, EmotionalState emotionalChange)
    {
        foreach (var agent in perceivedAgents)
        {
            int agentId = agent.GetInstanceID();
            if (!knownAgents.ContainsKey(agentId))
            {
                knownAgents[agentId] = new Relationship(agent);
                Debug.Log($"<color=pink>[Social] New agent met! ID: {agentId}</color>");
            }

            Relationship relationship = knownAgents[agentId];

            // Increase familiarity just by being around them
            relationship.Familiarity += familiarityGainRate * Time.deltaTime;
            relationship.Familiarity = Mathf.Clamp(relationship.Familiarity, 0, 100);

            // Update affinity based on shared experience. If we felt good, we like them more.
            float affinityChange = (emotionalChange.satisfaction - emotionalChange.frustration) * affinityMultiplier;
            if(Mathf.Abs(affinityChange) > 0.1f)
            {
                relationship.Affinity += affinityChange;
                relationship.Affinity = Mathf.Clamp(relationship.Affinity, -100, 100);
                Debug.Log($"<color=pink>[Social] Affinity for agent {agentId} changed by {affinityChange:F2}. New affinity: {relationship.Affinity:F1}</color>");
            }
        }
    }
}
