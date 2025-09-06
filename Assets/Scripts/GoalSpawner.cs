using UnityEngine;
using System.Collections.Generic;

public class GoalSpawner : MonoBehaviour
{
    [Header("Goal Settings")]
    public GameObject goalPrefab;
    public float minSpawnRadius = 3f;
    public float maxSpawnRadius = 10f;

    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;
    public int numberOfObstacles = 1;
    public float obstacleSpawnRadius = 5f;
    public LayerMask obstacleLayerMask; // Layer for walls/obstacles

    [Header("Monitoring")]
    [SerializeField] private List<GameObject> activeGoals = new List<GameObject>();
    [SerializeField] private List<GameObject> activeObstacles = new List<GameObject>();

    private float lastRespawnTime = -100f;
    private const float RESPAWN_COOLDOWN = 1.0f;

    public void RespawnGoalAndObstacles()
    {
        if (Time.time < lastRespawnTime + RESPAWN_COOLDOWN)
        {
            return;
        }
        lastRespawnTime = Time.time;

        // --- CHANGE: Respawn obstacles and goals at the same time ---
        DestroyAllGameObjects(activeObstacles);
        DestroyAllGameObjects(activeGoals);
        
        SpawnObstacles();
        SpawnGoal();
    }

    private void SpawnGoal()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool positionFound = false;
        int attempts = 0;

        while (!positionFound && attempts < 100) 
        {
            attempts++;
            Vector2 randomPoint = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, maxSpawnRadius);
            spawnPosition = new Vector3(randomPoint.x, goalPrefab.transform.position.y, randomPoint.y);

            if (!Physics.CheckSphere(spawnPosition, 1.5f, obstacleLayerMask))
            {
                positionFound = true;
            }
        }

        if (positionFound)
        {
            GameObject newGoal = Instantiate(goalPrefab, spawnPosition, Quaternion.identity);
            newGoal.tag = "Goal";
            activeGoals.Add(newGoal);
        }
        else
        { 
            Debug.LogWarning("Could not find a valid spawn position for the goal. Try reducing obstacle size/count or increasing spawn radius.");
        }
    }

    private void SpawnObstacles()
    { 
        if (obstaclePrefab == null) return;

        for (int i = 0; i < numberOfObstacles; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * obstacleSpawnRadius;
            Vector3 spawnPosition = new Vector3(randomPoint.x, obstaclePrefab.transform.position.y, randomPoint.y);
            Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0, 180), 0);
            GameObject newObstacle = Instantiate(obstaclePrefab, spawnPosition, spawnRotation);
            newObstacle.tag = "Wall";
            // Set the layer for the obstacle
            newObstacle.layer = LayerMask.NameToLayer("Obstacles");
            activeObstacles.Add(newObstacle);
        }
    }

    private void DestroyAllGameObjects(List<GameObject> objectList)
    {
        for (int i = objectList.Count - 1; i >= 0; i--)
        {
            if (objectList[i] != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(objectList[i]);
                }
                else
                {
                    DestroyImmediate(objectList[i]);
                }
            }
        }
        objectList.Clear();
    }
}
