using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.MLAgents;
using UnityEngine;

public class DynamicProgramming : SelectionStrategy
{
    private Shopper shopper;
    private Eyes eyes;
    private HashSet<Item> seenItemsSet = new();
    private List<Item> seenItemsList = new();
    private Thread thread;
    private Queue<Item> fetchQueue = new();
    private Queue<Item> discardQueue = new();
    private bool newSolutionFound = false;

    private void Start()
    {
        eyes = GetComponent<Eyes>();
        shopper = GetComponent<Shopper>();

        thread = new(new ThreadStart(DPRunner));
        thread.Start();
    }

    private void Update()
    {
        HashSet<GameObject> shortSightedResult = eyes.ShortSightedScan();

        foreach (GameObject result in shortSightedResult)
        {
            Item item = result.GetComponent<Item>();
            if (!seenItemsSet.Contains(item))
            {
                seenItemsSet.Add(item);
                seenItemsList.Add(item);
            }
        }

        if (newSolutionFound)
        {
            while (discardQueue.Count > 0)
            {
                shopper.Discard(discardQueue.Dequeue());
            }

            while (fetchQueue.Count > 0)
            {
                shopper.Fetch(fetchQueue.Dequeue());
            }

            newSolutionFound = false;
        }
    }

    private void OnDestroy()
    {
        thread.Abort();
    }

    private void DPRunner()
    {
        int[] currentSolution = { };

        while (true)
        {
            if (newSolutionFound)
            {
                continue;
            }

            int[] solution = UKP5();

            for (int i = 0; i < solution.Length; i++)
            {
                int diff = (i >= currentSolution.Length) ?
                    solution[i] :
                    solution[i] - currentSolution[i]; ;

                if (diff > 0)
                {
                    for (int j = 0; j < diff; j++)
                    {
                        fetchQueue.Enqueue(seenItemsList[i]);
                    }
                }
                else if (diff < 0)
                {
                    for (int j = 0; j < Mathf.Abs(diff); j++)
                    {
                        discardQueue.Enqueue(seenItemsList[i]);
                    }
                }
            }

            currentSolution = solution;
            newSolutionFound = true;
        }
    }

    private int[] UKP5()
    {
        int c = Mathf.CeilToInt(Parameters.Instance.ShoppingCartCapacity);
        int wmin = Mathf.CeilToInt(Parameters.Instance.MinSize);
        int wmax = Mathf.CeilToInt(Parameters.Instance.MaxSize);
        int n = seenItemsList.Count;
        float[] g = new float[c + wmax + 1];
        int[] d = Enumerable.Repeat(n - 1, c + wmax + 1).ToArray();

        for (int i = 0; i < n; i++)
        {
            int wi = Mathf.CeilToInt(seenItemsList[i].Size);
            float pi = seenItemsList[i].SavedValue;

            if (g[wi] < pi)
            {
                g[wi] = pi;
                d[wi] = i;
            }
        }

        float opt = 0.0f;
        // y value where opt is
        int yopt = 0;

        for (int y = wmin; y <= c; y++)
        {
            if (g[y] <= opt)
            {
                continue;
            }

            yopt = y;
            opt = g[y];

            for (int i = 1; i <= d[y]; i++)
            {
                int wi = Mathf.CeilToInt(seenItemsList[i].Size);
                float pi = seenItemsList[i].SavedValue;

                // Debug.Log(wi + " compared to " + wmax);

                if (g[y + wi] < g[y] + pi)
                {
                    g[y + wi] = g[y] + pi;
                    d[y + wi] = i;
                }
            }
        }

        int[] solution = new int[n];

        while (yopt > 0)
        {
            int i = d[yopt];
            int wi = Mathf.CeilToInt(seenItemsList[i].Size);
            solution[i]++;
            yopt -= wi;
        }

        return solution;
    }
}
