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
/// It now uses structured data from the KnowledgeBridge.
/// </summary>
public class Conceptualizer : MonoBehaviour
{
    public Dictionary<string, UnderstoodConcept> knownConcepts = new Dictionary<string, UnderstoodConcept>();

    /// <summary>
    /// Learns about a new concept by querying the KnowledgeBridge and using the structured info.
    /// </summary>
    public UnderstoodConcept LearnNewConcept(string conceptName)
    {
        string upperConceptName = conceptName.ToUpper();
        if (knownConcepts.ContainsKey(upperConceptName))
        {
            return knownConcepts[upperConceptName];
        }

        Debug.Log($"<color=lightblue>[Conceptualizer] Encountered unknown concept: '{conceptName}'. Querying Knowledge Bridge...</color>");
        
        // --- REFACTORED: Use structured data directly ---
        ConceptEntry conceptInfo = KnowledgeBridge.GetConceptInfo(conceptName);
        Debug.Log($"<color=lightblue>[Conceptualizer] Info received for '{conceptInfo.name}'. Valence: {conceptInfo.valence}, Dangerous: {conceptInfo.isDangerous}</color>");

        // --- The "Digestion" Process is now direct mapping ---
        UnderstoodConcept newConcept = new UnderstoodConcept
        {
            Name = upperConceptName,
            Valence = conceptInfo.valence,
            IsDangerous = conceptInfo.isDangerous
        };

        Debug.Log($"<color=lightblue>[Conceptualizer] Digested! '{newConcept.Name}' is considered { (newConcept.Valence > 0 ? "Good" : (newConcept.Valence < 0 ? "Bad" : "Neutral")) } and {(newConcept.IsDangerous ? "Dangerous" : "Safe")}.</color>");

        knownConcepts[newConcept.Name] = newConcept;
        return newConcept;
    }
}
