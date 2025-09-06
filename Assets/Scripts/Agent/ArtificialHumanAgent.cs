using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EmotionalCore))]
public class ArtificialHumanAgent : Agent
{
    private Rigidbody rb;
    public AgentGenome genome;

    [Header("Movement Parameters")]
    public float forceMultiplier = 20f;
    public float rotationSpeed = 100f;

    [Header("Goal")]
    public Transform target;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (genome == null) { genome = new AgentGenome(); }
        GetComponent<EmotionalCore>().Initialize(genome);
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);

        // Find the Goal in the scene automatically
        if (target == null)
        {
            Goal goal = FindObjectOfType<Goal>();
            if (goal != null)
            {
                target = goal.transform;
            }
            else
            {
                Debug.LogError("Goal object with 'Goal' script not found in scene. Please add one.");
                return; // Stop if no goal is found
            }
        }

        target.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float horizontal = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? -1f : 0f);
        float vertical = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? -1f : 0f);

        continuousActions[0] = horizontal;
        continuousActions[1] = vertical;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotate = actions.ContinuousActions[0];
        transform.Rotate(0, rotate * rotationSpeed * Time.deltaTime, 0);

        float moveZ = actions.ContinuousActions[1];
        Vector3 force = transform.forward * moveZ * forceMultiplier;
        rb.AddForce(force);

        AddReward(-0.05f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (target != null)
        {
            sensor.AddObservation(target.localPosition);
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(rb.linearVelocity.x);
            sensor.AddObservation(rb.linearVelocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Goal>() != null)
        {
            AddReward(1.0f);
            EndEpisode();
        }
    }
}
