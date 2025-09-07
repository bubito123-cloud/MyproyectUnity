using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EpisodicMemory
{
    public Vector3 Position;
    public Vector3 PerceptualEmbedding; // Represents the general context
    public float Strength;
    public float LastAccessTimestamp;

    // --- MODIFIED: Added specific fields for trauma ---
    public bool IsTraumatic;
    public string TraumaSourceConcept; // e.g., "FUEGO"

    public float GetSimilarity(Vector3 queryPosition, Vector3 queryEmbedding)
    {
        float posSimilarity = 1f / (1f + Vector3.Distance(Position, queryPosition));
        float embeddingSimilarity = 1f / (1f + Vector3.Distance(PerceptualEmbedding, queryEmbedding));
        return (posSimilarity * 0.6f) + (embeddingSimilarity * 0.4f);
    }
}

public class MemoryStore : MonoBehaviour
{
    [Header("Memory System Parameters")]
    public int memoryCapacity = 50;
    public float decayTimeConstant = 180f; // in seconds
    [Range(0, 1)] public float reinforcementRate = 0.5f;
    public float reinforcementThreshold = 0.8f;

    private List<EpisodicMemory> memories = new List<EpisodicMemory>();

    void Update()
    {
        ApplyMemoryDecay(Time.deltaTime);
    }

    public void AddOrReinforceMemory(Vector3 position, Vector3 perceptualEmbedding, float initialStrength, bool isTraumatic, string traumaSource = null)
    {
        var (mostSimilarMemory, similarity) = FindMostSimilarMemory(position, perceptualEmbedding);

        if (mostSimilarMemory != null && similarity >= reinforcementThreshold)
        {
            // Reinforce existing memory
            mostSimilarMemory.Strength += reinforcementRate * (1 - mostSimilarMemory.Strength); // Approach 1.0
            mostSimilarMemory.LastAccessTimestamp = Time.time;
            if (isTraumatic && string.IsNullOrEmpty(mostSimilarMemory.TraumaSourceConcept))
            {
                mostSimilarMemory.IsTraumatic = true;
                mostSimilarMemory.TraumaSourceConcept = traumaSource;
            }
        }
        else
        {
            // Add new memory
            if (memories.Count >= memoryCapacity)
            {
                EvictLeastRelevantMemory();
            }
            memories.Add(new EpisodicMemory
            {
                Position = position,
                PerceptualEmbedding = perceptualEmbedding,
                Strength = Mathf.Clamp01(initialStrength),
                IsTraumatic = isTraumatic,
                TraumaSourceConcept = traumaSource,
                LastAccessTimestamp = Time.time
            });
        }
    }

    // --- MODIFIED: Method now takes the source concept of the trauma ---
    public void RecordTrauma(Vector3 position, string sourceConceptName)
    {
        Debug.Log($"<color=red>[Memory] Recording traumatic event at {position} caused by '{sourceConceptName}'.</color>");
        // Use a placeholder for the general perceptual embedding, as the key info is the source concept.
        AddOrReinforceMemory(position, Vector3.one * -1, 1.0f, true, sourceConceptName.ToUpper());
    }

    public List<EpisodicMemory> GetRecentTraumas()
    {
        return memories.Where(m => m.IsTraumatic).OrderByDescending(m => m.LastAccessTimestamp).ToList();
    }

    public EpisodicMemory FindMostRelevantMemory(Vector3 queryPosition, Vector3 queryEmbedding)
    {
        if (memories.Count == 0) return null;
        return memories.OrderByDescending(m => m.GetSimilarity(queryPosition, queryEmbedding) * m.Strength).First();
    }

    private (EpisodicMemory, float) FindMostSimilarMemory(Vector3 pos, Vector3 emb)
    {
        if (memories.Count == 0) return (null, 0);
        EpisodicMemory bestMatch = null;
        float highestSimilarity = -1f;
        foreach (var mem in memories)
        {
            float sim = mem.GetSimilarity(pos, emb);
            if (sim > highestSimilarity)
            {
                highestSimilarity = sim;
                bestMatch = mem;
            }
        }
        return (bestMatch, highestSimilarity);
    }

    private void ApplyMemoryDecay(float deltaTime)
    {
        float decayFactor = Mathf.Exp(-deltaTime / decayTimeConstant);
        for (int i = memories.Count - 1; i >= 0; i--)
        {
            memories[i].Strength *= decayFactor;
            // Non-traumatic memories fade away completely
            if (memories[i].Strength < 0.01f && !memories[i].IsTraumatic)
            {
                memories.RemoveAt(i);
            }
        }
    }

    private void EvictLeastRelevantMemory()
    {
        // Find the weakest, oldest, non-traumatic memory to evict.
        var evictCandidate = memories
            .Where(m => !m.IsTraumatic)
            .OrderBy(m => m.Strength)
            .ThenBy(m => m.LastAccessTimestamp)
            .FirstOrDefault();

        if (evictCandidate != null)
        {
            memories.Remove(evictCandidate);
        }
        else
        {
            // If all memories are traumatic, we must remove the weakest/oldest one.
            var oldestTrauma = memories.OrderBy(m => m.Strength).ThenBy(m => m.LastAccessTimestamp).FirstOrDefault();
            if (oldestTrauma != null)
            {
                memories.Remove(oldestTrauma);
            }
        }
    }

    public void ClearMemories() { memories.Clear(); }
}
