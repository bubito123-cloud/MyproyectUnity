using UnityEngine;

public class GoalSpawner : MonoBehaviour
{
    [Header("Goal Configuration")]
    public GameObject goalPrefab;
    public Transform goalParent;
    public Vector3 spawnAreaMin = new Vector3(-8, 0, -8);
    public Vector3 spawnAreaMax = new Vector3(8, 0, 8);
    public float minDistanceFromAgent = 3f;
    public float goalLifetime = 30f;

    [Header("Spawn Behavior")]
    public bool autoRespawn = true;
    public float respawnDelay = 2f;

    [Header("Current Goal Info")]
    public GameObject currentGoal;
    public Vector3 currentGoalPosition;
    public float goalAge = 0f;

    // Internal state
    private Transform agentTransform;
    private bool goalConsumed = false;
    private float respawnTimer = 0f;

    private void Start()
    {
        // Find agent transform
        ArtificialHuman agent = FindObjectOfType<ArtificialHuman>();
        if (agent != null)
        {
            agentTransform = agent.transform;
        }

        // Create initial goal
        SpawnGoal();
    }

    private void Update()
    {
        // Update goal age
        if (currentGoal != null)
        {
            goalAge += Time.deltaTime;

            // Despawn goal if too old
            if (goalAge > goalLifetime)
            {
                DestroyCurrentGoal();
                if (autoRespawn)
                {
                    respawnTimer = respawnDelay;
                }
            }
        }

        // Handle respawn timer
        if (respawnTimer > 0)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0 && autoRespawn)
            {
                SpawnGoal();
            }
        }

        // Auto-respawn if no goal exists and auto-respawn is enabled
        if (currentGoal == null && autoRespawn && respawnTimer <= 0)
        {
            respawnTimer = respawnDelay;
        }
    }

    public void SpawnGoal()
    {
        // Destroy existing goal first
        if (currentGoal != null)
        {
            DestroyCurrentGoal();
        }

        // Find valid spawn position
        Vector3 spawnPosition = FindValidSpawnPosition();

        // Create goal
        if (goalPrefab != null)
        {
            currentGoal = Instantiate(goalPrefab, spawnPosition, Quaternion.identity);

            if (goalParent != null)
            {
                currentGoal.transform.SetParent(goalParent);
            }

            // Ensure goal has proper tag and collider
            if (!currentGoal.CompareTag("Goal"))
            {
                currentGoal.tag = "Goal";
            }

            // Add trigger collider if missing
            Collider goalCollider = currentGoal.GetComponent<Collider>();
            if (goalCollider == null)
            {
                SphereCollider sphereCollider = currentGoal.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = 1f;
            }
            else
            {
                goalCollider.isTrigger = true;
            }

            // Add visual effects
            AddGoalEffects(currentGoal);
        }

        currentGoalPosition = spawnPosition;
        goalAge = 0f;
        goalConsumed = false;
        respawnTimer = 0f;

        Debug.Log($"Goal spawned at {spawnPosition}");

        // Notify agent about new goal
        MLAgentWithEmotions agent = FindObjectOfType<MLAgentWithEmotions>();
        if (agent != null)
        {
            agent.goalTarget = currentGoal.transform;
        }
    }

    private Vector3 FindValidSpawnPosition()
    {
        Vector3 spawnPosition;
        int attempts = 0;
        int maxAttempts = 20;

        do
        {
            // Random position within spawn area
            spawnPosition = new Vector3(
                UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                UnityEngine.Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            attempts++;

        } while (attempts < maxAttempts && !IsValidSpawnPosition(spawnPosition));

        return spawnPosition;
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        // Check distance from agent
        if (agentTransform != null)
        {
            float distanceFromAgent = Vector3.Distance(position, agentTransform.position);
            if (distanceFromAgent < minDistanceFromAgent)
            {
                return false;
            }
        }

        // Check for obstacles using overlap sphere
        Collider[] overlapping = Physics.OverlapSphere(position, 1f);
        foreach (Collider col in overlapping)
        {
            if (col.CompareTag("Obstacle") || col.CompareTag("Wall"))
            {
                return false;
            }
        }

        return true;
    }

    private void AddGoalEffects(GameObject goal)
    {
        // Add rotating animation
        GoalRotator rotator = goal.GetComponent<GoalRotator>();
        if (rotator == null)
        {
            rotator = goal.AddComponent<GoalRotator>();
        }

        // Add pulsing light
        Light goalLight = goal.GetComponentInChildren<Light>();
        if (goalLight == null)
        {
            GameObject lightGO = new GameObject("GoalLight");
            lightGO.transform.SetParent(goal.transform);
            lightGO.transform.localPosition = Vector3.up * 2f;

            goalLight = lightGO.AddComponent<Light>();
            goalLight.type = LightType.Point;
            goalLight.color = Color.green;
            goalLight.intensity = 2f;
            goalLight.range = 10f;
        }

        // Add particle system
        ParticleSystem particles = goal.GetComponentInChildren<ParticleSystem>();
        if (particles == null)
        {
            GameObject particleGO = new GameObject("GoalParticles");
            particleGO.transform.SetParent(goal.transform);
            particleGO.transform.localPosition = Vector3.zero;

            particles = particleGO.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = Color.green;
            main.startLifetime = 2f;
            main.startSpeed = 2f;
            main.maxParticles = 50;

            var emission = particles.emission;
            emission.rateOverTime = 10f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;
        }
    }

    public void RespawnGoal()
    {
        if (autoRespawn)
        {
            respawnTimer = respawnDelay;
        }
        else
        {
            SpawnGoal();
        }
    }

    public void DestroyCurrentGoal()
    {
        if (currentGoal != null)
        {
            Destroy(currentGoal);
            currentGoal = null;

            // Clear agent's goal target
            MLAgentWithEmotions agent = FindObjectOfType<MLAgentWithEmotions>();
            if (agent != null)
            {
                agent.goalTarget = null;
            }
        }
    }

    public void OnGoalReached()
    {
        goalConsumed = true;
        DestroyCurrentGoal();

        if (autoRespawn)
        {
            respawnTimer = respawnDelay;
        }
    }

    // Public getters
    public bool HasActiveGoal() => currentGoal != null;
    public float GetDistanceToGoal(Vector3 fromPosition) =>
        currentGoal != null ? Vector3.Distance(fromPosition, currentGoal.transform.position) : float.MaxValue;
    public Vector3 GetGoalDirection(Vector3 fromPosition) =>
        currentGoal != null ? (currentGoal.transform.position - fromPosition).normalized : Vector3.zero;

    // Configuration methods
    public void SetSpawnArea(Vector3 min, Vector3 max)
    {
        spawnAreaMin = min;
        spawnAreaMax = max;
    }

    public void SetAutoRespawn(bool enabled)
    {
        autoRespawn = enabled;
    }

    public void SetRespawnDelay(float delay)
    {
        respawnDelay = Mathf.Max(0f, delay);
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        // Draw spawn area
        Gizmos.color = Color.cyan;
        Vector3 center = (spawnAreaMin + spawnAreaMax) / 2f;
        Vector3 size = spawnAreaMax - spawnAreaMin;
        Gizmos.DrawWireCube(center, size);

        // Draw current goal
        if (currentGoal != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentGoal.transform.position, 1f);
        }

        // Draw minimum distance from agent
        if (agentTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(agentTransform.position, minDistanceFromAgent);
        }
    }

    [ContextMenu("Spawn Goal Now")]
    public void SpawnGoalNow()
    {
        SpawnGoal();
    }

    [ContextMenu("Destroy Goal")]
    public void DestroyGoalNow()
    {
        DestroyCurrentGoal();
    }
}

// Helper component for goal animation
public class GoalRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0, 45, 0);
    public bool enablePulsing = true;
    public float pulseSpeed = 2f;
    public float pulseScale = 0.2f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Rotate goal
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // Pulse scale
        if (enablePulsing)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = originalScale + Vector3.one * pulse;
        }
    }
}