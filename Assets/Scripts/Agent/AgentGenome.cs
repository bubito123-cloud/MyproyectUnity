using UnityEngine;

/// <summary>
/// Represents the "genetic code" of an agent. 
/// These values are heritable and determine the agent's base personality and emotional responses.
/// They are typically values between 0.0 and 1.0.
/// </summary>
[System.Serializable] // Makes it visible in the Inspector and serializable
public class AgentGenome
{
    [Header("Personality Genes")]
    [Range(0.1f, 1.0f)]
    public float riskAversion = 0.5f; // High = cautious, Low = brave

    [Range(0.1f, 1.0f)]
    public float socialOrientation = 0.5f; // High = seeks others, Low = loner

    [Range(0.1f, 1.0f)]
    public float curiosity = 0.5f; // High = drawn to unknown, Low = ignores unknown

    [Header("Emotional Genes")]
    [Range(0.1f, 2.0f)]
    public float emotionalSensitivity = 1.0f; // Multiplier for incoming emotional events

    [Range(0.001f, 0.1f)]
    public float emotionalDecayRate = 0.01f; // How quickly emotions return to baseline

    /// <summary>
    /// Creates a "child" genome by combining genes from two parents.
    /// </summary>
    public static AgentGenome Crossover(AgentGenome parent1, AgentGenome parent2)
    {
        AgentGenome child = new AgentGenome();

        // Randomly pick genes from either parent
        child.riskAversion = Random.value < 0.5f ? parent1.riskAversion : parent2.riskAversion;
        child.socialOrientation = Random.value < 0.5f ? parent1.socialOrientation : parent2.socialOrientation;
        child.curiosity = Random.value < 0.5f ? parent1.curiosity : parent2.curiosity;
        child.emotionalSensitivity = Random.value < 0.5f ? parent1.emotionalSensitivity : parent2.emotionalSensitivity;
        child.emotionalDecayRate = Random.value < 0.5f ? parent1.emotionalDecayRate : parent2.emotionalDecayRate;

        return child;
    }

    /// <summary>
    /// Applies random mutations to the genome's genes.
    /// </summary>
    public void Mutate(float mutationRate)
    {
        if (Random.value < mutationRate) riskAversion = Mathf.Clamp(riskAversion + Random.Range(-0.1f, 0.1f), 0.1f, 1.0f);
        if (Random.value < mutationRate) socialOrientation = Mathf.Clamp(socialOrientation + Random.Range(-0.1f, 0.1f), 0.1f, 1.0f);
        if (Random.value < mutationRate) curiosity = Mathf.Clamp(curiosity + Random.Range(-0.1f, 0.1f), 0.1f, 1.0f);
        if (Random.value < mutationRate) emotionalSensitivity = Mathf.Clamp(emotionalSensitivity + Random.Range(-0.2f, 0.2f), 0.1f, 2.0f);
        if (Random.value < mutationRate) emotionalDecayRate = Mathf.Clamp(emotionalDecayRate + Random.Range(-0.01f, 0.01f), 0.001f, 0.1f);
    }

    /// <summary>
    /// Creates a deep copy of this genome.
    /// </summary>
    public AgentGenome Clone()
    {
        return new AgentGenome
        {
            riskAversion = this.riskAversion,
            socialOrientation = this.socialOrientation,
            curiosity = this.curiosity,
            emotionalSensitivity = this.emotionalSensitivity,
            emotionalDecayRate = this.emotionalDecayRate
        };
    }
}
