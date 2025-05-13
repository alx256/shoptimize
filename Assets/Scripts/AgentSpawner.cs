using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject agentPrefab;
    [SerializeField]
    private GameObject rlPrefab;
    [SerializeField]
    private NavMeshSurface ground;

    private readonly string[] MOVEMENT_STRATEGIES = {
        "RandomExploration",
        "ShelfExplore",
        "RL"
    };

    private readonly string[] SELECTION_STRATEGIES = {
        "SavingsSize",
        "GA",
        "DP"
    };

    private void Start()
    {
        foreach (string movementStrategy in MOVEMENT_STRATEGIES)
        {
            foreach (string selectionStrategy in SELECTION_STRATEGIES)
            {
                GameObject clone = Instantiate((movementStrategy == "RL") ? rlPrefab : agentPrefab);
                MoveToRandomPosition(clone);
                AddStrategy(movementStrategy, clone);
                AddStrategy(selectionStrategy, clone);
                clone.name = movementStrategy + " " + selectionStrategy;
                clone.GetComponent<Shopper>().shopperName = movementStrategy + " " + selectionStrategy;
            }
        }
    }

    private void MoveToRandomPosition(GameObject clone)
    {
        Vector3 v;
        MeshRenderer groundMeshRenderer = ground.GetComponent<MeshRenderer>();
        MeshRenderer cloneMeshRenderer = clone.GetComponent<MeshRenderer>();
        float xStart = -groundMeshRenderer.bounds.size.x / 2;
        float xEnd = groundMeshRenderer.bounds.size.x / 2;
        float zStart = -groundMeshRenderer.bounds.size.z / 2;
        float zEnd = groundMeshRenderer.bounds.size.z / 2;

        // Find a vector v where we are not overlapping with any shelves
        do
        {
            float x = Random.Range(xStart, xEnd);
            float y = cloneMeshRenderer.bounds.size.y + 10;
            float z = Random.Range(zStart, zEnd);
            v = new Vector3(x, y, z);
        } while (Physics.OverlapBox(v, clone.transform.localScale).Length != 0);

        clone.transform.position = v;
    }

    private void AddStrategy(string name, GameObject clone)
    {
        switch (name)
        {
            case "RandomExploration":
                clone.AddComponent<RandomExploration>();
                return;
            case "ShelfExplore":
                clone.AddComponent<ShelfExplore>();
                return;
            case "RL":
                return;
            case "SavingsSize":
                clone.AddComponent<SavingsSizeRatioSelection>();
                return;
            case "GA":
                clone.AddComponent<GeneticAlgorithm>();
                return;
            case "DP":
                clone.AddComponent<DynamicProgramming>();
                return;
        }

        Debug.LogWarning("Tried to add unknown strategy '" + name + "'");
    }
}
