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
/// It sees an object, asks the KnowledgeBridge what it is, and forms an internal model.
/// </summary>
public class Conceptualizer : MonoBehaviour
{
    public Dictionary<string, UnderstoodConcept> knownConcepts = new Dictionary<string, UnderstoodConcept>();

    /// <summary>
    /// Learns about a new concept by querying the KnowledgeBridge and digesting the information.
    /// </summary>
    public UnderstoodConcept LearnNewConcept(string conceptName)
    {
        if (knownConcepts.ContainsKey(conceptName.ToUpper()))
        {
            return knownConcepts[conceptName.ToUpper()];
        }

        Debug.Log($"<color=lightblue>[Conceptualizer] Encountered unknown concept: '{conceptName}'. Querying Knowledge Bridge...</color>");
        string rawInfo = KnowledgeBridge.GetConceptInfo(conceptName);
        Debug.Log($"<color=lightblue>[Conceptualizer] Info received: \"{rawInfo}\"</color>");

        // --- The "Digestion" Process ---
        UnderstoodConcept newConcept = new UnderstoodConcept { Name = conceptName.ToUpper() };
        
        if (rawInfo.Contains("Positivo")) newConcept.Valence = 1.0f;
        else if (rawInfo.Contains("Negativo")) newConcept.Valence = -1.0f;
        else newConcept.Valence = 0.0f;

        if (rawInfo.Contains("Peligroso") || rawInfo.Contains("Daño")) newConcept.IsDangerous = true;

        Debug.Log($"<color=lightblue>[Conceptualizer] Digested! '{newConcept.Name}' is considered { (newConcept.Valence > 0 ? "Good" : (newConcept.Valence < 0 ? "Bad" : "Neutral")) } and {(newConcept.IsDangerous ? "Dangerous" : "Safe")}.</color>");

        knownConcepts[newConcept.Name] = newConcept;
        return newConcept;
    }
}
