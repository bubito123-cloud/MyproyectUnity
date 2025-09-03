using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class MemoryEvent
{
    public string eventType;
    public string timestamp;
    public Vector3 location;
    public Dictionary<string, float> emotionalImpact;
    public string context;
}

[System.Serializable]
public class LearnedRule
{
    public string id;
    public string condition;
    public string action;
    public float confidence;
    public Vector3 location;
}

[System.Serializable]
public class AgentMemoryData
{
    public string agentId;
    public string sessionStart;
    public List<MemoryEvent> memories;
    public List<LearnedRule> rules;
    public Dictionary<string, object> metadata;

    public AgentMemoryData()
    {
        memories = new List<MemoryEvent>();
        rules = new List<LearnedRule>();
        metadata = new Dictionary<string, object>();
    }
}

public class MemoryStore : MonoBehaviour
{
    [Header("Configuration")]
    public string agentId = "agent_001";
    public int maxMemories = 1000;
    public bool enableFirebaseSync = false;
    public string firebaseEndpoint = "";

    [Header("Runtime Info")]
    public int currentMemoryCount = 0;
    public int currentRuleCount = 0;
    public string lastSaveTime = "";

    // Internal data
    private AgentMemoryData memoryData;
    private string localFilePath;
    private float lastAutoSave = 0f;
    private const float AUTO_SAVE_INTERVAL = 30f; // seconds

    private void Awake()
    {
        // Initialize memory data
        memoryData = new AgentMemoryData();
        memoryData.agentId = agentId;
        memoryData.sessionStart = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Set up file path
        localFilePath = Path.Combine(Application.persistentDataPath, $"memories_{agentId}.json");

        Debug.Log($"MemoryStore initialized. File path: {localFilePath}");
    }

    private void Start()
    {
        LoadMemories();

        // Add initial metadata
        memoryData.metadata["unity_version"] = Application.unityVersion;
        memoryData.metadata["platform"] = Application.platform.ToString();
        memoryData.metadata["session_id"] = System.Guid.NewGuid().ToString();
    }

    private void Update()
    {
        // Auto-save every 30 seconds
        if (Time.time - lastAutoSave > AUTO_SAVE_INTERVAL)
        {
            SaveMemories();
            lastAutoSave = Time.time;
        }
    }

    public void AddMemory(string category, MemoryEvent memory)
    {
        if (memoryData.memories.Count >= maxMemories)
        {
            // Remove oldest memory to make space
            memoryData.memories.RemoveAt(0);
        }

        memoryData.memories.Add(memory);
        currentMemoryCount = memoryData.memories.Count;

        Debug.Log($"Memory added: {memory.eventType} at {memory.timestamp}");

        // Auto-save important memories immediately
        if (memory.eventType == "goal_reached" || memory.eventType == "collision_repeated")
        {
            SaveMemories();
        }
    }

    public void AddRule(LearnedRule rule)
    {
        // Check if similar rule exists
        bool ruleExists = false;
        for (int i = 0; i < memoryData.rules.Count; i++)
        {
            if (memoryData.rules[i].condition == rule.condition &&
                Vector3.Distance(memoryData.rules[i].location, rule.location) < 2f)
            {
                // Update existing rule confidence
                memoryData.rules[i].confidence = Mathf.Min(1f, memoryData.rules[i].confidence + 0.1f);
                ruleExists = true;
                break;
            }
        }

        if (!ruleExists)
        {
            memoryData.rules.Add(rule);
            currentRuleCount = memoryData.rules.Count;
            Debug.Log($"New rule learned: {rule.condition} → {rule.action}");
        }

        SaveMemories();
    }

    public List<MemoryEvent> GetMemories(string eventType = null)
    {
        if (string.IsNullOrEmpty(eventType))
        {
            return new List<MemoryEvent>(memoryData.memories);
        }

        List<MemoryEvent> filtered = new List<MemoryEvent>();
        foreach (var memory in memoryData.memories)
        {
            if (memory.eventType == eventType)
            {
                filtered.Add(memory);
            }
        }

        return filtered;
    }

    public List<LearnedRule> GetRules()
    {
        return new List<LearnedRule>(memoryData.rules);
    }

    public void SaveMemories()
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(memoryData, Formatting.Indented);
            File.WriteAllText(localFilePath, jsonData);

            lastSaveTime = System.DateTime.Now.ToString("HH:mm:ss");
            Debug.Log($"Memories saved to {localFilePath}");

            // Optional Firebase sync
            if (enableFirebaseSync && !string.IsNullOrEmpty(firebaseEndpoint))
            {
                StartCoroutine(SyncToFirebase());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save memories: {e.Message}");
        }
    }

    public void LoadMemories()
    {
        try
        {
            if (File.Exists(localFilePath))
            {
                string jsonData = File.ReadAllText(localFilePath);
                memoryData = JsonConvert.DeserializeObject<AgentMemoryData>(jsonData);

                if (memoryData == null)
                {
                    memoryData = new AgentMemoryData();
                    memoryData.agentId = agentId;
                }

                currentMemoryCount = memoryData.memories.Count;
                currentRuleCount = memoryData.rules.Count;

                Debug.Log($"Loaded {currentMemoryCount} memories and {currentRuleCount} rules");
            }
            else
            {
                Debug.Log("No existing memory file found. Starting fresh.");
                memoryData = new AgentMemoryData();
                memoryData.agentId = agentId;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load memories: {e.Message}");
            memoryData = new AgentMemoryData();
            memoryData.agentId = agentId;
        }
    }

    public void ClearMemories()
    {
        memoryData.memories.Clear();
        memoryData.rules.Clear();
        currentMemoryCount = 0;
        currentRuleCount = 0;
        SaveMemories();
        Debug.Log("All memories cleared");
    }

    public void AddMetadata(string key, object value)
    {
        memoryData.metadata[key] = value;
    }

    public T GetMetadata<T>(string key, T defaultValue = default(T))
    {
        if (memoryData.metadata.ContainsKey(key))
        {
            try
            {
                return (T)memoryData.metadata[key];
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    // Optional Firebase sync
    private System.Collections.IEnumerator SyncToFirebase()
    {
        if (string.IsNullOrEmpty(firebaseEndpoint))
        {
            yield break;
        }

        var jsonData = JsonConvert.SerializeObject(memoryData);
        var postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        var www = new UnityEngine.Networking.UnityWebRequest(firebaseEndpoint, "POST");
        www.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(postData);
        www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.Log("Memory synced to Firebase successfully");
        }
        else
        {
            Debug.LogWarning($"Firebase sync failed: {www.error}");
        }
    }

    // Public API for external access
    public AgentMemoryData GetAllData() => memoryData;

    public string GetMemoryFilePath() => localFilePath;

    public void ForceSync()
    {
        SaveMemories();
    }

    // Debug methods
    [ContextMenu("Print Memory Stats")]
    public void PrintMemoryStats()
    {
        Debug.Log($"Agent ID: {memoryData.agentId}");
        Debug.Log($"Session Start: {memoryData.sessionStart}");
        Debug.Log($"Total Memories: {memoryData.memories.Count}");
        Debug.Log($"Total Rules: {memoryData.rules.Count}");

        // Memory breakdown by type
        Dictionary<string, int> memoryTypes = new Dictionary<string, int>();
        foreach (var memory in memoryData.memories)
        {
            if (!memoryTypes.ContainsKey(memory.eventType))
                memoryTypes[memory.eventType] = 0;
            memoryTypes[memory.eventType]++;
        }

        Debug.Log("Memory breakdown:");
        foreach (var kvp in memoryTypes)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }

    [ContextMenu("Save Now")]
    public void SaveNow()
    {
        SaveMemories();
    }

    [ContextMenu("Load Now")]
    public void LoadNow()
    {
        LoadMemories();
    }
}
