using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ArtificialHumanAgent))]
[RequireComponent(typeof(PerceptionSystem))]
[RequireComponent(typeof(MemoryStore))]
[RequireComponent(typeof(EmotionalCore))]
[RequireComponent(typeof(Conceptualizer))]
[RequireComponent(typeof(DeliberativePlanner))]
[RequireComponent(typeof(SocialEngine))]
[RequireComponent(typeof(BufferSensorComponent))]
public class CognitiveController : MonoBehaviour
{
    // --- Core References ---
    private ArtificialHumanAgent agentBody;
    private PerceptionSystem perceptionSystem;
    private MemoryStore memoryStore;
    private EmotionalCore emotionalCore;
    private Conceptualizer conceptualizer;
    private DeliberativePlanner planner;
    private SocialEngine socialEngine;
    private BufferSensorComponent bufferSensor;
    private NarratorEngine narrator;

    public AgentGenome genome = new AgentGenome();
    private bool hasKey = false;
    private SubGoal currentSubGoal;
    private List<Collider> touchingObjects = new List<Collider>();
    private Dictionary<string, int> tagToConceptId = new Dictionary<string, int>();

    // --- Metacognition State ---
    private float timeOnCurrentGoal = 0f;
    private bool isConfused = false;
    [Header("Metacognition Parameters")]
    public float confusionTimeThreshold = 20f;
    public float frustrationThresholdForConfusion = 90f;

    void Awake()
    {
        agentBody = GetComponent<ArtificialHumanAgent>();
        perceptionSystem = GetComponent<PerceptionSystem>();
        memoryStore = GetComponent<MemoryStore>();
        emotionalCore = GetComponent<EmotionalCore>();
        conceptualizer = GetComponent<Conceptualizer>();
        planner = GetComponent<DeliberativePlanner>();
        socialEngine = GetComponent<SocialEngine>();
        bufferSensor = GetComponent<BufferSensorComponent>();
        narrator = Object.FindFirstObjectByType<NarratorEngine>();
    }

    IEnumerator Start()
    {
        while (KnowledgeBridge.Instance == null || !KnowledgeBridge.Instance.IsInitialized())
        {
            yield return null;
        }
        var concepts = KnowledgeBridge.Instance.GetAllConcepts();
        for (int i = 0; i < concepts.Count; i++)
        {
            tagToConceptId[concepts[i].name.ToUpper()] = i;
        }
    }

    void FixedUpdate()
    {
        var perceivedAgents = perceptionSystem.GetPerceivedAgents();
        var emotionalChange = emotionalCore.GetEmotionalChange();
        socialEngine.UpdateSocialModel(perceivedAgents, emotionalChange);
    }

    public void OnEpisodeBegin()
    {
        this.hasKey = false;
        memoryStore.ClearMemories();
        perceptionSystem.Clear();
        touchingObjects.Clear();
        currentSubGoal = null;
        timeOnCurrentGoal = 0f;
        isConfused = false;
        if(narrator != null) narrator.ClearHistory();
    }

    public void CollectAgentObservations(VectorSensor sensor)
    {
        // --- Vector Observations (Low-dimensional data) ---
        // Total Size: 7
        sensor.AddObservation(hasKey); // 1. bool

        var emotions = emotionalCore.GetCurrentState();
        sensor.AddObservation(emotions.satisfaction / 100f); // 2. float
        sensor.AddObservation(emotions.frustration / 100f);  // 3. float
        sensor.AddObservation(emotions.curiosity / 100f);    // 4. float
        sensor.AddObservation(emotions.focus / 100f);        // 5. float

        sensor.AddObservation(currentSubGoal != null ? (int)currentSubGoal.type : -1);     // 6. int
        sensor.AddObservation(currentSubGoal != null ? (int)currentSubGoal.priority : -1); // 7. int

        // --- Buffer Observations (High-dimensional data for each perceived object) ---
        foreach (var p_obj in perceptionSystem.GetPerceivedObjects())
        {
            var concept = conceptualizer.LearnNewConcept(p_obj.tag);
            if (concept == null) continue;

            float[] observation = new float[5]; // x, y, z position + isDangerous + conceptId
            var relativePos = transform.InverseTransformPoint(p_obj.transform.position);
            observation[0] = relativePos.x;
            observation[1] = relativePos.y;
            observation[2] = relativePos.z;
            observation[3] = concept.IsDangerous ? 1f : 0f;
            observation[4] = tagToConceptId.ContainsKey(concept.Name.ToUpper()) ? tagToConceptId[concept.Name.ToUpper()] : -1;

            bufferSensor.AppendObservation(observation);
        }
    }

    public void ProcessActions(ActionBuffers actions)
    {
        UpdateConfusion();
        var newSubGoal = planner.CreateNewSubGoal(genome, hasKey, transform, emotionalCore.GetCurrentState(), isConfused);
        if (newSubGoal?.type != currentSubGoal?.type)
        {
            timeOnCurrentGoal = 0f;
            currentSubGoal = newSubGoal;
            if (narrator != null) narrator.Narrate("My new goal is to: " + currentSubGoal.type);
        }

        int discreteAction = actions.DiscreteActions[0];
        if (currentSubGoal == null) {
            Explore();
            return;
        }

        switch (discreteAction)
        {
            case 0: agentBody.SetNavMeshDestination(currentSubGoal.position); break;
            case 1: Explore(); break;
            case 2: Interact(); break;
            case 3: AskForHelp(); break;
        }
    }

    private void UpdateConfusion()
    {
        timeOnCurrentGoal += Time.fixedDeltaTime;
        if (timeOnCurrentGoal > confusionTimeThreshold && emotionalCore.GetCurrentState().frustration > frustrationThresholdForConfusion)
        {
            isConfused = true;
            if(narrator != null) narrator.Narrate("I'm confused and don't know what to do.");
        }
    }

    private void AskForHelp()
    {
        if(currentSubGoal?.type == SubGoalType.AskForHelp)
        {
            if (narrator != null) narrator.Narrate("I'm stuck. My goal is " + currentSubGoal.type + ". I should ask for help.");
            KnowledgeBridge.Instance.QueryOracle("I am confused about how to achieve goal: " + currentSubGoal.type);
            isConfused = false;
            timeOnCurrentGoal = 0f;
        }
    }

    private void Interact()
    {
        if (currentSubGoal?.type == SubGoalType.ActivateSwitch)
        {
            var switchCollider = touchingObjects.FirstOrDefault(c => c.CompareTag("Switch"));
            if (switchCollider != null)
            {
                Switch switchComponent = switchCollider.GetComponent<Switch>();
                if (switchComponent != null)
                {
                    switchComponent.Activate();
                    agentBody.AddAgentReward(0.5f);
                    emotionalCore.TriggerEmotionalEvent("satisfaction", 15f);
                    if (narrator != null) narrator.Narrate("I activated the switch!");
                    currentSubGoal = null;
                }
            }
        }
    }

    private void Explore()
    {
        if (narrator != null) narrator.Narrate("I'm not sure what to do, so I'll explore.");
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        Vector3 explorationPoint = transform.position + new Vector3(randomDirection.x, 0, randomDirection.z);
        agentBody.SetNavMeshDestination(explorationPoint);
    }

    public void ReportTouch(Collider other)
    {
        if (!touchingObjects.Contains(other)) touchingObjects.Add(other);
        var concept = conceptualizer.LearnNewConcept(other.tag);
        if (concept == null) return;
        var embedding = CreatePerceptualEmbedding(concept);
        memoryStore.AddOrReinforceMemory(other.transform.position, embedding, Mathf.Abs(concept.Valence), concept.IsDangerous, concept.Name);
        if (concept.Name == "KEY") {
            if (!hasKey) {
                hasKey = true;
                agentBody.AddAgentReward(1.0f);
                emotionalCore.TriggerEmotionalEvent("satisfaction", 20f);
                if (narrator != null) narrator.Narrate("I found the key!");
                currentSubGoal = null;
            }
        } else if (concept.Name == "GOAL") {
            agentBody.AddAgentReward(hasKey ? 2.0f : -0.5f);
            emotionalCore.TriggerEmotionalEvent(hasKey ? "satisfaction" : "frustration", hasKey ? 40f : 10f);
            agentBody.EndAgentEpisode();
        } else if (concept.IsDangerous) {
            agentBody.AddAgentReward(-1.0f);
            emotionalCore.TriggerEmotionalEvent("frustration", 50f);
            if (narrator != null) narrator.Narrate("Ouch, that's dangerous!");
            memoryStore.RecordTrauma(other.transform.position, concept.Name);
            agentBody.EndAgentEpisode();
        }
    }

    public void ReportTouchExit(Collider other)
    {
        touchingObjects.Remove(other);
    }

    private Vector3 CreatePerceptualEmbedding(UnderstoodConcept concept)
    {
        return new Vector3(concept.Valence, concept.IsDangerous ? 1.0f : 0.0f, 0);
    }
}
