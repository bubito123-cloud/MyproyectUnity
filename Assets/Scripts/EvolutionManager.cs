using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text; // For efficient logging

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }

    [Header("Evolution Parameters")]
    public int populationSize = 10;
    public float mutationRate = 0.05f;
    [Tooltip("The number of best-performing agents to carry over to the next generation.")]
    public int elitism = 2;

    [Header("Simulation Setup")]
    public GameObject agentPrefab;
    public Transform spawnAreaCenter;
    public float spawnAreaRadius = 20f;

    private List<AgentGenome> currentGeneration;
    private Dictionary<int, (float, AgentGenome)> fitnessReports;
    private int generationCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // --- ROBUSTNESS: Validate parameters on start ---
        if (agentPrefab == null)
        {
            Debug.LogError("[EvolutionManager] Agent Prefab is not assigned! Disabling script.", this);
            this.enabled = false;
            return;
        }
        if (elitism >= populationSize)
        {
            Debug.LogWarning($"[EvolutionManager] Elitism ({elitism}) is >= population size ({populationSize}). Adjusting to {populationSize - 1}.", this);
            elitism = Mathf.Max(0, populationSize - 1);
        }

        fitnessReports = new Dictionary<int, (float, AgentGenome)>();
        currentGeneration = new List<AgentGenome>();
        for (int i = 0; i < populationSize; i++)
        {
            currentGeneration.Add(new AgentGenome());
        }
        StartGeneration(currentGeneration);
    }

    public void StartGeneration(List<AgentGenome> newGenomes)
    {
        generationCount++;
        Debug.Log($"<color=green>--- STARTING GENERATION {generationCount} (Population: {newGenomes.Count}) ---</color>");

        currentGeneration = newGenomes;
        fitnessReports.Clear();

        if (populationSize <= 0)
        {
            Debug.LogError("[EvolutionManager] Population Size is 0! Cannot spawn agents.");
            return;
        }

        // Destroy old agents before creating new ones
        foreach (var agent in FindObjectsOfType<ArtificialHumanAgent>())
        {
            Destroy(agent.gameObject);
        }

        for (int i = 0; i < newGenomes.Count; i++)
        {
            float angle = i * (360f / newGenomes.Count);
            Vector3 spawnPosition = spawnAreaCenter.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * spawnAreaRadius;
            spawnPosition.y = agentPrefab.transform.position.y; // Ensure correct spawn height

            GameObject agentGO = Instantiate(agentPrefab, spawnPosition, Quaternion.identity);
            agentGO.name = $"Agent_{generationCount}_{i}";
            ArtificialHumanAgent agent = agentGO.GetComponent<ArtificialHumanAgent>();
            agent.genome = newGenomes[i];
            agent.Initialize();
        }
    }

    public void ReportFitnessAndGenome(int agentInstanceId, float fitness, AgentGenome genome)
    {
        if (!fitnessReports.ContainsKey(agentInstanceId))
        {
            fitnessReports.Add(agentInstanceId, (fitness, genome));
        }

        // Check if all agents from the current generation have reported
        if (fitnessReports.Count >= populationSize)
        {
            Evolve();
        }
    }

    private void Evolve()
    {
        // --- ROBUSTNESS: Handle case of empty fitness reports ---
        if (fitnessReports.Count == 0)
        {
            Debug.LogWarning("[EvolutionManager] Evolve called with no fitness reports. Restarting generation.");
            StartGeneration(currentGeneration); // Restart with the same generation
            return;
        }

        var sortedGenomes = fitnessReports.Values.OrderByDescending(report => report.Item1).ToList();

        // --- ENHANCED LOGGING ---
        var logBuilder = new StringBuilder();
        logBuilder.AppendLine($"<color=yellow>--- EVOLVING GENERATION {generationCount} ---</color>");
        logBuilder.AppendLine($"Top {elitism} performers (the 'elite'):");
        for(int i = 0; i < Mathf.Min(elitism, sortedGenomes.Count); i++)
        {
            logBuilder.AppendLine($"  - Fitness: {sortedGenomes[i].Item1:F3}, Genome: {sortedGenomes[i].Item2}");
        }
        Debug.Log(logBuilder.ToString());

        List<AgentGenome> nextGeneration = new List<AgentGenome>();

        // 1. Elitism
        for (int i = 0; i < elitism; i++)
        {
            nextGeneration.Add(sortedGenomes[i].Item2);
        }

        // 2. Crossover & Mutation
        while (nextGeneration.Count < populationSize)
        {
            AgentGenome parent1 = SelectParent(sortedGenomes).Item2;
            AgentGenome parent2 = SelectParent(sortedGenomes).Item2;

            AgentGenome child = AgentGenome.Crossover(parent1, parent2);
            child.Mutate(mutationRate);

            nextGeneration.Add(child);
        }

        StartGeneration(nextGeneration);
    }

    // Tournament Selection
    private (float, AgentGenome) SelectParent(List<(float, AgentGenome)> sortedPopulation)
    {
        int tournamentSize = 3;
        List<(float, AgentGenome)> tournamentContestants = new List<(float, AgentGenome)>();
        for (int i = 0; i < tournamentSize; i++)
        {
            tournamentContestants.Add(sortedPopulation[Random.Range(0, sortedPopulation.Count)]);
        }
        return tournamentContestants.OrderByDescending(c => c.Item1).First();
    }
}
