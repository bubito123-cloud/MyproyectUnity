using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum SubGoalType { Explore, GoToKey, GoToExit, ApproachConcept, AvoidConcept, ApproachAgent, FollowAgent, AvoidAgent, GoToFoodSource, ActivateSwitch, AskForHelp }
public enum SubGoalPriority { Trivial = 0, Low = 1, Medium = 2, High = 3, Critical = 4, Overriding = 5 }

public class SubGoal
{
    public Vector3 position;
    public SubGoalType type;
    public SubGoalPriority priority;
    public Transform targetAgent;
    public InteractableObject interactableTarget;

    public SubGoal(Vector3 pos, SubGoalType t, SubGoalPriority p, Transform agent = null, InteractableObject interactable = null)
    {
        position = pos;
        type = t;
        priority = p;
        targetAgent = agent;
        interactableTarget = interactable;
    }
}

[RequireComponent(typeof(PerceptionSystem), typeof(Conceptualizer), typeof(SocialEngine))]
public class DeliberativePlanner : MonoBehaviour
{
    private PerceptionSystem perceptionSystem;
    private SocialEngine socialEngine;

    void Awake()
    {
        perceptionSystem = GetComponent<PerceptionSystem>();
        socialEngine = GetComponent<SocialEngine>();
    }

    public SubGoal CreateNewSubGoal(AgentGenome genome, bool hasKey, Transform agentTransform, EmotionalState emotions, bool isConfused)
    {
        // --- Highest Priority: Metacognition ---
        if (isConfused)
        {
            return new SubGoal(agentTransform.position, SubGoalType.AskForHelp, SubGoalPriority.Overriding);
        }

        if (KnowledgeBridge.Instance == null || !KnowledgeBridge.Instance.IsInitialized())
        {
            return new SubGoal(agentTransform.position + new Vector3(Random.insideUnitSphere.x * 10, 0, Random.insideUnitSphere.z * 10), SubGoalType.Explore, SubGoalPriority.Trivial);
        }

        var potentialGoals = new List<SubGoal>();
        List<PerceivedObject> allPerceivedObjects = perceptionSystem.GetPerceivedObjects();
        int frustrationBonus = (int)(emotions.frustration / 33f);
        int curiosityBonus = (int)(emotions.curiosity / 25f);

        // --- Priority 1: Safety ---
        var dangerousConcepts = KnowledgeBridge.Instance.GetAllConcepts().Where(c => c.isDangerous).Select(c => c.name.ToUpper());
        var perceivedDangers = allPerceivedObjects.Where(o => dangerousConcepts.Contains(o.tag.ToUpper())).ToList();
        if (perceivedDangers.Any())
        {
            var closestDanger = perceivedDangers.OrderBy(d => Vector3.Distance(agentTransform.position, d.transform.position)).First();
            Vector3 avoidancePosition = agentTransform.position + (agentTransform.position - closestDanger.transform.position).normalized * 15f;
            return new SubGoal(avoidancePosition, SubGoalType.AvoidConcept, SubGoalPriority.Critical);
        }

        // --- Priority 2: Task-Based Goals ---
        var exitObj = allPerceivedObjects.FirstOrDefault(o => o.transform.CompareTag("Goal"));
        var doorObj = allPerceivedObjects.FirstOrDefault(o => o.transform.CompareTag("Door"));
        var switchObj = allPerceivedObjects.FirstOrDefault(o => o.transform.CompareTag("Switch"));

        if (exitObj == null && doorObj != null && switchObj != null)
        {
             potentialGoals.Add(new SubGoal(switchObj.transform.position, SubGoalType.ActivateSwitch, SubGoalPriority.High + 1));
        }

        if (hasKey)
        {
            if (exitObj != null) potentialGoals.Add(new SubGoal(exitObj.transform.position, SubGoalType.GoToExit, SubGoalPriority.High + frustrationBonus));
        }
        else
        {
            var keyObj = allPerceivedObjects.FirstOrDefault(o => o.transform.CompareTag("Key"));
            if (keyObj != null) potentialGoals.Add(new SubGoal(keyObj.transform.position, SubGoalType.GoToKey, SubGoalPriority.Medium + curiosityBonus));
        }

        // --- Priority 3: Social Goals ---
        if (socialEngine.knownAgents.Any())
        {
            foreach (var relationship in socialEngine.knownAgents.Values)
            {
                if (relationship.Agent == null) continue;
                float socialUrgency = (genome?.socialOrientation ?? 0.5f) * (relationship.Affinity / 100f);
                if (socialUrgency > 0.3f)
                {
                    potentialGoals.Add(new SubGoal(relationship.Agent.transform.position, SubGoalType.ApproachAgent, SubGoalPriority.Low + (int)(socialUrgency * 2), relationship.Agent.transform));
                }
                else if (socialUrgency < -0.3f)
                {
                    Vector3 avoidancePos = agentTransform.position + (agentTransform.position - relationship.Agent.transform.position).normalized * 10f;
                    potentialGoals.Add(new SubGoal(avoidancePos, SubGoalType.AvoidAgent, SubGoalPriority.Medium - (int)(socialUrgency * 2), relationship.Agent.transform));
                }
            }
        }

        // --- Lowest Priority: Fallback Goal ---
        if (!potentialGoals.Any())
        {
            Vector3 explorationPoint = agentTransform.position + new Vector3(Random.insideUnitSphere.x, 0, Random.insideUnitSphere.z) * 15f;
            potentialGoals.Add(new SubGoal(explorationPoint, SubGoalType.Explore, SubGoalPriority.Trivial + (int)(curiosityBonus * 0.5f)));
        }

        return potentialGoals.OrderByDescending(g => g.priority).First();
    }
}
