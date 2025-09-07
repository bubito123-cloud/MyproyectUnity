using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents the agent's digested understanding of a concept.
/// </summary>
public class UnderstoodConcept
{
    public string Name;
    public float Valence; // Is it good (>0) or bad (<0)?
    public bool IsDangerous;
}

/// <summary>
/// The agent's module for learning and understanding concepts in the world.
/// </summary>
public class Conceptualizer : MonoBehaviour
{
    public Dictionary<string, UnderstoodConcept> knownConcepts = new Dictionary<string, UnderstoodConcept>();

    /// <summary>
    /// Learns about a new concept by querying the KnowledgeBridge instance.
    /// </summary>
    public UnderstoodConcept LearnNewConcept(string conceptName)
    {
        string upperConceptName = conceptName.ToUpper();
        if (knownConcepts.ContainsKey(upperConceptName))
        {
            return knownConcepts[upperConceptName];
        }

        Debug.Log($"<color=lightblue>[Conceptualizer] Encountered unknown concept: '{conceptName}'. Querying Knowledge Bridge...</color>");

        // --- FIX: An object reference is required. Access the singleton Instance. ---
        // Before: ConceptEntry conceptInfo = KnowledgeBridge.GetConceptInfo(conceptName);
        ConceptEntry conceptInfo = KnowledgeBridge.Instance.GetConceptInfo(conceptName);

        Debug.Log($"<color=lightblue>[Conceptualizer] Info received for '{conceptInfo.name}'. Valence: {conceptInfo.valence}, Dangerous: {conceptInfo.isDangerous}</color>");

        var newConcept = new UnderstoodConcept
        {
            Name = conceptInfo.name,
            Valence = conceptInfo.valence,
            IsDangerous = conceptInfo.isDangerous
        };

        knownConcepts[upperConceptName] = newConcept;
        return newConcept;
    }
}
