using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A simple script to tag objects in the world with a conceptual name.
/// </summary>
public class ConceptTag : MonoBehaviour
{
    public string ConceptName;
}

/// <summary>
/// A simple, static class that acts as a simulated external knowledge database (like Wikipedia or an AI chat).
/// </summary>
public static class KnowledgeBridge
{
    private static Dictionary<string, string> knowledgeBase = new Dictionary<string, string>()
    {
        {"MANZANA", "Concepto: Fruta. Atributos: Comestible, Dulce, Fuente de energía. Implicación: Positivo, Reduce el hambre."},
        {"FUEGO", "Concepto: Elemento. Atributos: Caliente, Peligroso, Quema. Implicación: Negativo, Causa daño, Evitar."},
        {"LIBRO", "Concepto: Objeto. Atributos: Informativo, Pasivo. Implicación: Neutral, Potencial para la curiosidad."}
    };

    public static string GetConceptInfo(string conceptName)
    {
        conceptName = conceptName.ToUpper();
        if (knowledgeBase.ContainsKey(conceptName))
        {
            return knowledgeBase[conceptName];
        }
        return "Concepto: Desconocido. Atributos: Incierto. Implicación: Neutral, Causa curiosidad.";
    }
}
