using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using System.Collections.Generic;


/// <summary>
/// El "cerebro" del agente. Une todos los módulos.
/// Hereda de Agent de ML-Agents.
/// Su única responsabilidad es tomar decisiones, recolectar observaciones y gestionar recompensas.
/// Delega las tareas de movimiento, percepción y gestión emocional a otros componentes.
/// </summary>
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(PerceptionSystem))]
[RequireComponent(typeof(EmotionalCore))]
[RequireComponent(typeof(RewardSystem))]
[RequireComponent(typeof(PersonalityEngine))]
public class ArtificialHumanAgent : Agent
{
    [Header("Core Modules")]
    private MovementController movementController;
    private PerceptionSystem perceptionSystem;
    private EmotionalCore emotionalCore;
    private RewardSystem rewardSystem;
    private PersonalityEngine personalityEngine;

    [Header("External Dependencies")]
    public MemoryStore memoryStore;
    public NarratorEngine narrator;
    public EmotionalDisplay emotionalDisplay;
    public StuckDetector stuckDetector;

    [Header("Heuristic Settings")]
    public bool useHeuristicAI = true; // Si es verdadero, usará la IA del PersonalityEngine en lugar de las teclas

    // Internal State
    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float episodeTimer = 0f;

    #region INITIALIZATION & EPISODE MANAGEMENT

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        movementController = GetComponent<MovementController>();
        perceptionSystem = GetComponent<PerceptionSystem>();
        emotionalCore = GetComponent<EmotionalCore>();
        rewardSystem = GetComponent<RewardSystem>();
        personalityEngine = GetComponent<PersonalityEngine>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        if (memoryStore != null) memoryStore.LoadMemories();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        movementController.ResetPhysics();
        episodeTimer = 0f;

        GoalSpawner spawner = Object.FindFirstObjectByType<GoalSpawner>();
        if (spawner != null)
        {
            spawner.RespawnGoal();
            perceptionSystem.goalTarget = spawner.currentGoal.transform;
        }

        rewardSystem.Initialize(transform, perceptionSystem);
        emotionalCore.TriggerEmotionalEvent("motivation", 5f);
        emotionalCore.TriggerEmotionalEvent("frustration", -10f);
    }

    #endregion

    #region OBSERVATIONS, ACTIONS & HEURISTICS

    public override void CollectObservations(VectorSensor sensor)
    {
        bool isStuck = stuckDetector?.IsStuck() ?? false;
        perceptionSystem.CollectBasicObservations(sensor, transform, rb, emotionalCore.GetCurrentEmotions(), isStuck);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        episodeTimer += Time.fixedDeltaTime;

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotateY = actions.ContinuousActions[2];

        movementController.ApplyMovement(moveX, moveZ, rotateY);

        bool isStuck = stuckDetector?.IsStuck() ?? false;
        float movementIntensity = Mathf.Abs(moveX) + Mathf.Abs(moveZ) + Mathf.Abs(rotateY);
        emotionalCore.ProcessMovementEmotions(movementIntensity, isStuck);
        emotionalCore.UpdateEmotionalState(Time.fixedDeltaTime);

        rewardSystem.CalculateRewards(this, transform, perceptionSystem, emotionalCore.GetCurrentEmotions(), false);

        if (episodeTimer > 60f)
        {
            emotionalCore.TriggerEmotionalEvent("frustration", 15f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions.Clear();

        if (useHeuristicAI)
        {
            Vector3 decision = personalityEngine.MakeDecision(transform, perceptionSystem, emotionalCore.GetCurrentEmotions());
            continuousActions[0] = decision.x;
            continuousActions[1] = decision.z;

            if (decision.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(decision.x, decision.z) * Mathf.Rad2Deg;
                float angleDiff = Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle);
                continuousActions[2] = Mathf.Clamp(angleDiff / 90f, -1f, 1f);
            }
        }
        else // Control por teclado
        {
            if (Input.GetKey(KeyCode.W)) continuousActions[1] = 1f;
            if (Input.GetKey(KeyCode.S)) continuousActions[1] = -1f;
            if (Input.GetKey(KeyCode.A)) continuousActions[0] = -1f;
            if (Input.GetKey(KeyCode.D)) continuousActions[0] = 1f;
            if (Input.GetKey(KeyCode.Q)) continuousActions[2] = -1f;
            if (Input.GetKey(KeyCode.E)) continuousActions[2] = 1f;
        }
    }

    #endregion

    #region COLLISION & TRIGGERS

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            emotionalCore.TriggerEmotionalEvent("satisfaction", 30f);
            emotionalCore.TriggerEmotionalEvent("motivation", 15f);
            rewardSystem.CalculateRewards(this, transform, perceptionSystem, emotionalCore.GetCurrentEmotions(), true);
            // Lógica de memoria/narrador aquí
            // EndEpisode() es llamado dentro de CalculateRewards
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-0.1f);
            emotionalCore.TriggerEmotionalEvent("frustration", 15f);
            emotionalCore.TriggerEmotionalEvent("curiosity", 5f);
            // Lógica de memoria/narrador aquí
        }
    }

    #endregion
}

