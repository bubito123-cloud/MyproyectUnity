using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EpisodicMemory
{
    public Vector3 Position;
    public Vector3 PerceptualEmbedding;
    public float Strength;
    public float LastAccessTimestamp;
    public bool IsTraumatic;

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
    public float decayTimeConstant = 180f;
    [Range(0, 1)] public float reinforcementRate = 0.5f;
    public float reinforcementThreshold = 0.8f;

    private List<EpisodicMemory> memories = new List<EpisodicMemory>();

    void Update()
    {
        ApplyMemoryDecay(Time.deltaTime);
    }

    public void AddOrReinforceMemory(Vector3 position, Vector3 perceptualEmbedding, float initialStrength, bool isTraumatic)
    {
        var (mostSimilarMemory, similarity) = FindMostSimilarMemory(position, perceptualEmbedding);

        if (mostSimilarMemory != null && similarity >= reinforcementThreshold)
        {
            mostSimilarMemory.Strength += reinforcementRate * (1f - mostSimilarMemory.Strength);
            mostSimilarMemory.LastAccessTimestamp = Time.time;
        }
        else
        {
            if (memories.Count >= memoryCapacity) EvictLeastRelevantMemory();
            memories.Add(new EpisodicMemory
            {
                Position = position,
                PerceptualEmbedding = perceptualEmbedding,
                Strength = Mathf.Clamp01(initialStrength),
                IsTraumatic = isTraumatic,
                LastAccessTimestamp = Time.time
            });
        }
    }

    // NEW METHOD: Specifically records a traumatic event.
    public void RecordTrauma(Vector3 position, string description)
    {
        // We don't have a system to convert `description` to an embedding, 
        // so we use Vector3.zero as a placeholder for the perceptual context.
        AddOrReinforceMemory(position, Vector3.zero, 1.0f, true);
    }

    // NEW METHOD: Gets a list of all traumatic memories.
    public List<EpisodicMemory> GetRecentTraumas()
    {
        // Returns all memories flagged as traumatic. A more complex implementation
        // could filter by time, but for now this fulfills the compiler's need.
        return memories.Where(m => m.IsTraumatic).ToList();
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
            if (memories[i].Strength < 0.01f && !memories[i].IsTraumatic) memories.RemoveAt(i);
        }
    }

    private void EvictLeastRelevantMemory()
    {
        var evictCandidate = memories
            .Where(m => !m.IsTraumatic)
            .OrderBy(m => m.Strength)
            .ThenBy(m => m.LastAccessTimestamp)
            .FirstOrDefault();

        if (evictCandidate != null) { memories.Remove(evictCandidate); }
        else
        {
            var oldestTrauma = memories.OrderBy(m => m.Strength).ThenBy(m => m.LastAccessTimestamp).First();
            memories.Remove(oldestTrauma);
        }
    }

    public void ClearMemories() { memories.Clear(); }
}
