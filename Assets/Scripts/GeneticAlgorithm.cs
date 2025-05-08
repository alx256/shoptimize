using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GeneticAlgorithm : SelectionStrategy
{
    private class Solution : System.IComparable
    {
        private readonly List<int> data = new();
        private float totalSavedValue = 0.0f;
        private float totalSize = 0.0f;

        public void UseItem(int index)
        {
            BackfillFrom(index);
            data[index]++;
            totalSavedValue += itemsPool[index].SavedValue;
            totalSize += itemsPool[index].Size;
        }

        public void Put(int index, int value)
        {
            Item item = itemsPool[index];

            BackfillFrom(index);

            totalSavedValue -= item.SavedValue * data[index];
            totalSize -= item.Size * data[index];

            data[index] = value;
            totalSavedValue += itemsPool[index].SavedValue * value;
            totalSize += itemsPool[index].Size * value;
        }

        public int Get(int index)
        {
            if (index >= data.Count)
            {
                return 0;
            }

            return data[index];
        }

        public int[] GetDifference(Solution other)
        {
            int[] diff = new int[itemsPool.Count];

            for (int i = 0; i < itemsPool.Count; i++)
            {
                diff[i] = other.Get(i) - Get(i);
            }

            return diff;
        }

        public float Fitness()
        {
            return (totalSize > Parameters.Instance.ShoppingCartCapacity) ? -1.0f : totalSavedValue;
        }

        private void BackfillFrom(int index)
        {
            for (int i = data.Count; i <= index; i++)
            {
                data.Add(0);
            }
        }

        public int CompareTo(object obj)
        {
            Solution solution = (Solution)obj;
            float f1 = Fitness();
            float f2 = solution.Fitness();

            if (f1 == f2)
            {
                return 0;
            }

            return (f1 > f2) ? 1 : -1;
        }

        public override string ToString()
        {
            string s = "[";
            int i = 0;

            foreach (int occurences in data)
            {
                s += occurences;

                if (i < data.Count - 1)
                {
                    s += ", ";
                }
            }

            s += "]";
            return s;
        }
    }

    private HashSet<Item> seenItems;
    private static List<Item> itemsPool;
    private Eyes eyes;
    private Agent agent;
    private const int MINIMUM_SEEN_ITEMS = 10;
    private const int POPULATION_SIZE = 1000;
    private const int TOURNAMENT_SIZE = 200;
    private const float ALPHA = 0.5f;
    private const int G = (int)(ALPHA * POPULATION_SIZE);
    private const float CROSSOVER_PROBABILITY = 0.33f;
    private const int THINK_FRAME = 25;
    private bool started = false;
    private Solution bestSolution;
    private Solution[] replacementPopulation;
    private Solution[] population;

    private void Start()
    {
        eyes = GetComponent<Eyes>();
        agent = GetComponent<Agent>();
        seenItems = new();
        itemsPool = new();
        population = new Solution[POPULATION_SIZE];
        replacementPopulation = new Solution[G];
    }

    private void Update()
    {
        // Add all new items seen
        IEnumerable<Item> scanResult = eyes.ShortSightedScan().Select(x => x.GetComponent<Item>());
        IEnumerable<Item> newItems = scanResult.Where(x => !seenItems.Contains(x));

        itemsPool.AddRange(newItems);
        seenItems.AddRange(newItems);
        Step();
    }

    private void Step()
    {
        if (Time.frameCount % THINK_FRAME != 0)
        {
            return;
        }

        if (!started)
        {
            if (seenItems.Count >= MINIMUM_SEEN_ITEMS)
            {
                started = true;
                RandomInitialisation();
                System.Array.Sort(population);
            }
            else
            {
                return;
            }
        }

        Solution[] parents = TournamentSelection();

        for (int i = 0; i < G; i += 2)
        {
            Solution a = parents[i];
            Solution b = parents[i + 1];

            Solution[] children = UniformCrossover(a, b);
            replacementPopulation[i] = RandomAdd(children[0]);
            replacementPopulation[i + 1] = RandomAdd(children[1]);
        }

        // Replace first G solutions (AKA least fit solutions)
        // with the replacement population
        for (int i = 0; i < G; i++)
        {
            population[i] = replacementPopulation[i];
        }

        System.Array.Sort(population);
        Solution candidateBest = population.Last();

        if (candidateBest.Fitness() == -1)
        {
            Debug.LogWarning("Best solution in GA was invalid");
        }
        else if (bestSolution == null)
        {
            for (int i = 0; i < itemsPool.Count; i++)
            {
                int occurences = candidateBest.Get(i);

                if (occurences != 0)
                {
                    for (int j = 0; j < occurences; j++)
                    {
                        agent.Fetch(itemsPool[i]);
                    }
                }
            }
        }
        else
        {
            int[] diff = bestSolution.GetDifference(candidateBest);
            int index = 0;

            foreach (int change in diff)
            {
                if (change < 0)
                {
                    for (int i = 0; i < Mathf.Abs(change); i++)
                    {
                        agent.Discard(itemsPool[index]);
                    }
                }

                index++;
            }

            index = 0;

            foreach (int change in diff)
            {
                if (change > 0)
                {
                    for (int i = 0; i < change; i++)
                    {
                        agent.Fetch(itemsPool[index]);
                    }
                }

                index++;
            }
        }

        bestSolution = candidateBest;
    }

    private void RandomInitialisation()
    {
        for (int i = 0; i < POPULATION_SIZE; i++)
        {
            Solution solution = new();

            for (int j = 0; j < MINIMUM_SEEN_ITEMS; j++)
            {
                solution.UseItem(Random.Range(0, itemsPool.Count));
            }

            population[i] = solution;
        }
    }

    private Solution[] TournamentSelection()
    {
        Solution[] parents = new Solution[G];

        for (int i = 0; i < G; i++)
        {
            float bestFitness = -1.0f;
            Solution bestSolution = new();

            // Choose TOURNAMENT_SIZE random individuals from the population
            for (int j = 0; j < TOURNAMENT_SIZE; j++)
            {
                Solution solution = population[Random.Range(0, population.Length)];
                float fitness = solution.Fitness();

                if (fitness > bestFitness)
                {
                    bestFitness = fitness;
                    bestSolution = solution;
                }
            }

            parents[i] = bestSolution;
        }

        return parents;
    }

    private Solution[] UniformCrossover(Solution a, Solution b)
    {
        Solution[] children = new Solution[] { new(), new() };

        for (int i = 0; i < itemsPool.Count; i++)
        {
            // 50/50 chance of swapping
            if (Random.Range(0, 2) == 0)
            {
                children[0].Put(i, b.Get(i));
                children[1].Put(i, a.Get(i));
            }
            else
            {
                children[0].Put(i, a.Get(i));
                children[1].Put(i, b.Get(i));
            }
        }

        return children;
    }

    private Solution RandomAdd(Solution solution)
    {
        for (int i = 0; i < itemsPool.Count; i++)
        {
            if (Random.Range(0, 9) == 0)
            {
                int multiplier = (Random.Range(0, 2) == 0) ? 1 : -1;
                int newVal = solution.Get(i) + multiplier;

                if (newVal >= 0)
                {
                    // Should not be negative
                    // (this will still work but can lead to unexpected problems
                    // down the line)
                    solution.Put(i, solution.Get(i) + 1 * multiplier);
                }
            }
        }

        return solution;
    }
}
