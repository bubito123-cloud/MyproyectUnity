using UnityEngine;
using System.Collections.Generic;
using System.IO; // Required for file operations

/// <summary>
/// A simple script to tag objects in the world with a conceptual name.
/// </summary>
public class ConceptTag : MonoBehaviour
{
    public string ConceptName;
}

// --- NEW: Data structures to match the JSON file ---
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
/// A static class that acts as a simulated external knowledge database.
/// It now loads its knowledge from a JSON file instead of being hard-coded.
/// </summary>
public static class KnowledgeBridge
{
    private static Dictionary<string, ConceptEntry> knowledgeBase = new Dictionary<string, ConceptEntry>();
    private static bool isInitialized = false;

    // Static constructor to load data automatically on first access
    static KnowledgeBridge()
    {
        LoadKnowledgeBase();
    }

    private static void LoadKnowledgeBase()
    {
        if (isInitialized) return;

        string path = Path.Combine(Application.streamingAssetsPath, "knowledge_base.json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            KnowledgeData data = JsonUtility.FromJson<KnowledgeData>(jsonContent);

            foreach (var concept in data.concepts)
            {
                knowledgeBase[concept.name.ToUpper()] = concept;
            }
            Debug.Log($"<color=green>[KnowledgeBridge] Successfully loaded {knowledgeBase.Count} concepts from JSON.</color>");
        }
        else
        {
            Debug.LogError($"[KnowledgeBridge] knowledge_base.json not found at path: {path}. The knowledge base will be empty.");
        }
        isInitialized = true;
    }

    public static ConceptEntry GetConceptInfo(string conceptName)
    {
        // Ensure the database is loaded. This is a fallback for safety.
        if (!isInitialized) LoadKnowledgeBase();

        conceptName = conceptName.ToUpper();
        if (knowledgeBase.ContainsKey(conceptName))
        {
            return knowledgeBase[conceptName];
        }

        // Return a default "unknown" concept if not found
        return new ConceptEntry
        {
            name = conceptName,
            description = "Concepto: Desconocido. Atributos: Incierto. Implicación: Neutral, Causa curiosidad.",
            valence = 0,
            isDangerous = false
        };
    }
}
