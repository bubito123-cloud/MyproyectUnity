using UnityEngine;
using System.Collections.Generic;

// The filename MUST be StuckDetector.cs
public class StuckDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float checkInterval = 2.0f;
    public float positionThreshold = 0.5f;
    public int historyCount = 5;

    [Header("Stuck State")]
    [SerializeField] private bool isStuck = false;
    [SerializeField] private float timeStuck = 0f;

    // Internal state
    private List<Vector3> positionHistory = new List<Vector3>();
    private float lastCheckTime = 0f;
    private Transform agentTransform;

    private void Start()
    {
        agentTransform = transform;
    }

    private void Update()
    {
        if (Time.time > lastCheckTime + checkInterval)
        {
            lastCheckTime = Time.time;
            positionHistory.Add(agentTransform.position);

            if (positionHistory.Count > historyCount)
            {
                positionHistory.RemoveAt(0);
            }

            CheckIfStuck();
        }

        if (isStuck)
        {
            timeStuck += Time.deltaTime;
        }
    }

    private void CheckIfStuck()
    {
        if (positionHistory.Count < historyCount)
        {
            return; 
        }

        float maxDistance = 0f;
        for (int i = 0; i < positionHistory.Count; i++)
        {
            for (int j = i + 1; j < positionHistory.Count; j++)
            {
                maxDistance = Mathf.Max(maxDistance, Vector3.Distance(positionHistory[i], positionHistory[j]));
            }
        }

        if (maxDistance < positionThreshold)
        {
            if (!isStuck)
            {
                isStuck = true;
                timeStuck = 0f;
            }
        }
        else
        {
            isStuck = false;
            timeStuck = 0f;
        }
    }

    public bool IsStuck() => isStuck;
    public float GetTimeStuck() => timeStuck;

    private void OnDrawGizmos()
    {
        // Correction: Only draw if agentTransform exists (i.e., in Play mode)
        if (agentTransform == null) return;

        Gizmos.color = isStuck ? Color.red : Color.green;
        Gizmos.DrawWireSphere(agentTransform.position, positionThreshold);

        if (positionHistory.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in positionHistory)
            {
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }
}
