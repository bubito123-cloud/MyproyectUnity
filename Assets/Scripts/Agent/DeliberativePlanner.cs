using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Enums and SubGoal class are unchanged
public enum SubGoalType { Explore, GoToKey, GoToExit, ApproachConcept, AvoidConcept, ApproachAgent, FollowAgent, AvoidAgent, GoToFoodSource }
public enum SubGoalPriority { Trivial = 0, Low = 1, Medium = 2, High = 3, Critical = 4 }

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

    public override string ToString()
    {
        return $"Goal: {type}, Priority: {priority}, Position: {position}";
    }
}


[RequireComponent(typeof(PerceptionSystem), typeof(Conceptualizer), typeof(SocialEngine))]
public class DeliberativePlanner : MonoBehaviour
{
    private PerceptionSystem perceptionSystem;
    private SocialEngine socialEngine;

    void Awake()
    {
        // --- FIX: KnowledgeBridge now initializes itself. This call is no longer needed. ---
        // KnowledgeBridge.Initialize(); 
        perceptionSystem = GetComponent<PerceptionSystem>();
        socialEngine = GetComponent<SocialEngine>();
    }

    public SubGoal CreateNewSubGoal(AgentGenome genome, bool hasKey, Transform agentTransform, EmotionalState emotions)
    {
        // --- FIX: Access the KnowledgeBridge singleton instance --- 
        if (KnowledgeBridge.Instance == null || !KnowledgeBridge.Instance.IsInitialized())
        {
            // If the knowledge base isn't ready, the agent should wait or explore.
            // This prevents errors if the planner runs before the KB has loaded its data.
            Vector3 fallbackExploration = agentTransform.position + new Vector3(Random.insideUnitSphere.x * 10, 0, Random.insideUnitSphere.z * 10);
            return new SubGoal(fallbackExploration, SubGoalType.Explore, SubGoalPriority.Trivial);
        }

        var potentialGoals = new List<SubGoal>();
        List<PerceivedObject> allPerceivedObjects = perceptionSystem.GetPerceivedObjects();

        int frustrationBonus = (int)(emotions.frustration / 33f);
        int curiosityBonus = (int)(emotions.curiosity / 25f);

        // --- All calls now use `KnowledgeBridge.Instance` ---
        var dangerousConcepts = KnowledgeBridge.Instance.GetAllConcepts().Where(c => c.isDangerous).Select(c => c.name.ToUpper());
        var perceivedDangers = allPerceivedObjects
            .Where(o => o.transform.GetComponent<ConceptTag>() != null && dangerousConcepts.Contains(o.transform.GetComponent<ConceptTag>().ConceptName.ToUpper()))
            .ToList();

        if (perceivedDangers.Any())
        {
            var closestDanger = perceivedDangers.OrderBy(d => Vector3.Distance(agentTransform.position, d.transform.position)).First();
            Vector3 avoidancePosition = agentTransform.position + (agentTransform.position - closestDanger.transform.position).normalized * 15f;
            return new SubGoal(avoidancePosition, SubGoalType.AvoidConcept, SubGoalPriority.Critical, null, closestDanger.transform.GetComponent<InteractableObject>());
        }

        if (emotions.satisfaction < 50f)
        {
            var foodConcepts = KnowledgeBridge.Instance.GetAllConcepts().Where(c => c.name.ToUpper() == "MANZANA").Select(c => c.name.ToUpper());
            var perceivedFood = allPerceivedObjects
                .Where(o => o.transform.GetComponent<ConceptTag>() != null && foodConcepts.Contains(o.transform.GetComponent<ConceptTag>().ConceptName.ToUpper()))
                .Select(o => o.transform.GetComponent<FoodSource>())
                .FirstOrDefault(fs => fs != null && fs.isAvailable);

            if (perceivedFood != null)
            {
                SubGoalPriority foodPriority = SubGoalPriority.Medium + (int)((50 - emotions.satisfaction) / 15f);
                potentialGoals.Add(new SubGoal(perceivedFood.transform.position, SubGoalType.GoToFoodSource, foodPriority, null, perceivedFood));
            }
        }

        if (hasKey)
        {
            var exitObj = allPerceivedObjects.FirstOrDefault(o => o.transform.CompareTag("Goal"));
            if (exitObj != null) potentialGoals.Add(new SubGoal(exitObj.transform.position, SubGoalType.GoToExit, SubGoalPriority.High + frustrationBonus));
        }
        else
        {
            var keyObj = allPerceivedObjects.FirstOrDefault(o => o.transform.CompareTag("Key"));
            if (keyObj != null) potentialGoals.Add(new SubGoal(keyObj.transform.position, SubGoalType.GoToKey, SubGoalPriority.Medium + curiosityBonus));
        }

        if (!potentialGoals.Any())
        {
            Vector3 randomDirection = Random.insideUnitSphere * 15f;
            Vector3 explorationPoint = agentTransform.position + new Vector3(randomDirection.x, 0, randomDirection.z);
            potentialGoals.Add(new SubGoal(explorationPoint, SubGoalType.Explore, SubGoalPriority.Trivial + (int)(curiosityBonus * 0.5f)));
        }

        return potentialGoals.OrderByDescending(g => g.priority).First();
    }
}
