using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Networking; // Required for UnityWebRequest
using System.Collections;      // Required for Coroutines

// ConceptTag and JSON data structures remain the same
public class ConceptTag : MonoBehaviour
{
    public string ConceptName;
}

[System.Serializable]
public class ConceptEntry
{
    public string name;
    public string description;
    public float valence;
    public bool isDangerous;
}

[System.Serializable]
public class KnowledgeData
{
    public List<ConceptEntry> concepts;
}

/// <summary>
/// MAJOR REFACTOR: KnowledgeBridge is now a MonoBehaviour Singleton.
/// This uses a coroutine with UnityWebRequest to reliably load the JSON from StreamingAssets,
/// avoiding startup race conditions and System.IO issues.
/// </summary>
public class KnowledgeBridge : MonoBehaviour
{
    public static KnowledgeBridge Instance { get; private set; }

    private Dictionary<string, ConceptEntry> knowledgeBase = new Dictionary<string, ConceptEntry>();
    private bool isInitialized = false;

    void Awake()
    {
        // --- Singleton Pattern ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- Start the reliable loading process ---
        StartCoroutine(LoadKnowledgeBase());
    }

    private IEnumerator LoadKnowledgeBase()
    {
        if (isInitialized) yield break;

        string path = Path.Combine(Application.streamingAssetsPath, "knowledge_base.json");

        // UnityWebRequest is the modern, cross-platform way to handle StreamingAssets
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonContent = www.downloadHandler.text;
                KnowledgeData data = JsonUtility.FromJson<KnowledgeData>(jsonContent);

                knowledgeBase.Clear();
                foreach (var concept in data.concepts)
                {
                    knowledgeBase[concept.name.ToUpper()] = concept;
                }
                Debug.Log($"<color=cyan>[KnowledgeBridge] Successfully loaded {knowledgeBase.Count} concepts via UnityWebRequest.</color>");
            }
            else
            {
                // This error is now more informative
                Debug.LogError($"[KnowledgeBridge] Failed to load knowledge_base.json. Error: {www.error} at path {path}");
            }
        }
        isInitialized = true;
    }

    public ConceptEntry GetConceptInfo(string conceptName)
    {
        // We no longer need EnsureInitialized because access will happen after Awake/Start
        conceptName = conceptName.ToUpper();
        if (knowledgeBase.TryGetValue(conceptName, out ConceptEntry entry))
        {
            return entry;
        }
        return new ConceptEntry { name = conceptName, description = "Unknown concept.", valence = 0, isDangerous = false };
    }

    public List<ConceptEntry> GetAllConcepts()
    {
        return knowledgeBase.Values.ToList();
    }

    // Public check for other scripts if needed
    public bool IsInitialized()
    {
        return isInitialized;
    }
}
