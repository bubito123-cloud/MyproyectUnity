using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }

    [Header("Evolution Parameters")]
    public int populationSize = 10;
    public float mutationRate = 0.05f;
    public int elitism = 2;

    [Header("Simulation Setup")]
    public GameObject agentPrefab;
    public Transform spawnAreaCenter;
    public float spawnAreaRadius = 20f;

    private List<AgentGenome> currentGeneration;
    // Using a tuple to store fitness and genome. Item1 is fitness, Item2 is genome.
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
        Debug.Log($"<color=green>--- STARTING GENERATION {generationCount} ---</color>");

        currentGeneration = newGenomes;
        fitnessReports.Clear();

        if (populationSize <= 0)
        {
            Debug.LogError("[EvolutionManager] Population Size is 0! Cannot spawn agents. Please set a value greater than 0 in the Inspector.");
            return;
        }

        for (int i = 0; i < populationSize; i++)
        {
            float angle = i * (360f / populationSize);
            Vector3 spawnPosition = spawnAreaCenter.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * spawnAreaRadius;

            GameObject agentGO = Instantiate(agentPrefab, spawnPosition, Quaternion.identity);
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

        if (fitnessReports.Count >= populationSize)
        {
            Evolve();
        }
    }

    private void Evolve()
    {
        // CORRECTED: Use .Item1 to access the fitness value from the tuple.
        var sortedGenomes = fitnessReports.Values.OrderByDescending(report => report.Item1).ToList();

        List<AgentGenome> nextGeneration = new List<AgentGenome>();

        for (int i = 0; i < elitism; i++)
        {
            nextGeneration.Add(sortedGenomes[i].Item2); // .Item2 is the genome
        }

        while (nextGeneration.Count < populationSize)
        {
            AgentGenome parent1 = SelectParent(sortedGenomes).Item2; // .Item2 is the genome
            AgentGenome parent2 = SelectParent(sortedGenomes).Item2;

            AgentGenome child = AgentGenome.Crossover(parent1, parent2);
            child.Mutate(mutationRate);

            nextGeneration.Add(child);
        }

        // CORRECTED: Use modern, more performant FindObjectsByType.
        foreach (var agent in FindObjectsByType<ArtificialHumanAgent>(FindObjectsSortMode.None))
        {
            Destroy(agent.gameObject);
        }

        StartGeneration(nextGeneration);
    }

    private (float, AgentGenome) SelectParent(List<(float, AgentGenome)> sortedPopulation)
    {
        int tournamentSize = 3;
        List<(float, AgentGenome)> tournamentContestants = new List<(float, AgentGenome)>();
        for (int i = 0; i < tournamentSize; i++)
        {
            tournamentContestants.Add(sortedPopulation[Random.Range(0, sortedPopulation.Count)]);
        }
        // CORRECTED: Use .Item1 to access fitness for sorting.
        return tournamentContestants.OrderByDescending(c => c.Item1).First();
    }
}
