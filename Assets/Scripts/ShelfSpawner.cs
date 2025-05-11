using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ShelfSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject ground;
    [SerializeField]
    private GameObject agentPrefab;
    [SerializeField]
    private GameObject[] shelfPrefabs;
    [SerializeField]
    private int spawnNumber;
    [SerializeField]
    private int agentSpawnNumber;
    [SerializeField]
    private float margin;
    [SerializeField]
    private NavMeshSurface navMeshSurface;
    private readonly float[] POSSIBLE_ROTATIONS = {
        0.0f,
        90.0f,
        180.0f,
        270.0f
    };

    private MeshRenderer groundMeshRenderer;
    private readonly Queue<GameObject> addedShelves = new();

    private void Awake()
    {
        groundMeshRenderer = ground.GetComponent<MeshRenderer>();
    }

    public void SpawnOtherAgents()
    {
        for (int i = 0; i < agentSpawnNumber; i++)
        {
            GameObject clone = SpawnPrefab(agentPrefab);
            clone.AddComponent<RandomExploration>();
        }
    }

    public void Spawn()
    {
        // Remove all previously-spawned shelves
        while (addedShelves.Count != 0)
        {
            GameObject shelfObject = addedShelves.Dequeue();
            Destroy(shelfObject);
        }

        for (int i = 0; i < spawnNumber; i++)
        {
            GameObject shelf = shelfPrefabs[Random.Range(0, shelfPrefabs.Length)];
            addedShelves.Enqueue(SpawnPrefab(shelf));
        }

        StartCoroutine(RebuildNextFrame());
    }

    private GameObject SpawnPrefab(GameObject prefab)
    {
        MeshRenderer meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
        float groundStartX = ground.transform.position.x - groundMeshRenderer.bounds.size.x / 2;
        float groundEndX = ground.transform.position.x + groundMeshRenderer.bounds.size.x / 2;
        float groundStartZ = ground.transform.position.z - groundMeshRenderer.bounds.size.z / 2;
        float groundEndZ = ground.transform.position.z + groundMeshRenderer.bounds.size.z / 2;
        float x = Random.Range(groundStartX + margin, groundEndX - margin);
        float y = ground.transform.position.y + meshRenderer.bounds.size.y;
        float z = Random.Range(groundStartZ + margin, groundEndZ - margin);
        float rotationAngle = POSSIBLE_ROTATIONS[Random.Range(0, POSSIBLE_ROTATIONS.Length)];

        return Instantiate(prefab,
                new Vector3(x, y, z),
                Quaternion.AngleAxis(rotationAngle, Vector3.up));
    }

    private IEnumerator RebuildNextFrame()
    {
        // Wait one frame so Destroy() can finish
        yield return null;

        navMeshSurface.BuildNavMesh();
    }
}
