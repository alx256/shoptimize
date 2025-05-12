using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class ReinforcementLearningShopper : Agent
{
    public ShelfSpawner shelfSpawner;

    private Shopper shopper;
    private Eyes eyes;
    private readonly HashSet<GameObject> seenItems = new();
    private readonly HashSet<GameObject> seenShelves = new();
    private int itemMask;
    private int shelfMask;
    private int totalItemCount = int.MaxValue;
    private const float TARGET_COVERAGE = 0.62f;

    private void Start()
    {
        shopper = GetComponent<Shopper>();
        eyes = GetComponent<Eyes>();
        itemMask = 1 << LayerMask.NameToLayer("Item");
        shelfMask = 1 << LayerMask.NameToLayer("Shelf");

        if (Academy.Instance.IsCommunicatorOn)
        {
            shelfSpawner.SpawnOtherAgents();
        }
    }

    public override void OnEpisodeBegin()
    {
        // Randomise the shelf layout
        if (Academy.Instance.IsCommunicatorOn)
        {
            shelfSpawner.Spawn();
        }
        seenItems.Clear();
        StartCoroutine(DelayTotalItemCounting());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 itemPos = Vector3.zero;
        bool canSeeItem = false;
        Vector3 objectPos = Vector3.zero;
        bool canSeeObject = false;
        Vector3 shelfPos = Vector3.zero;
        bool canSeeShelf = false;
        float[] savedValues = new float[3];
        float[] itemSizes = new float[3];
        bool isNewItem = false;
        bool isNewShelf = false;

        GameObject justInFrontItemObject = eyes.PeekDirectlyInFront(mask: itemMask);
        if (justInFrontItemObject != null)
        {
            canSeeItem = true;
            itemPos = justInFrontItemObject.transform.position;
            isNewItem = !seenItems.Contains(justInFrontItemObject);
        }

        GameObject justInFrontAllObjects = eyes.PeekDirectlyInFront(mask: int.MaxValue);
        if (justInFrontAllObjects != null)
        {
            canSeeObject = true;
            objectPos = justInFrontAllObjects.transform.position;
        }

        HashSet<GameObject> longSightedLookObjects = eyes.LongSightedLook();
        if (longSightedLookObjects.Count != 0 && longSightedLookObjects.First().layer == LayerMask.NameToLayer("Shelf"))
        {
            GameObject shelfObject = longSightedLookObjects.First();
            canSeeShelf = true;
            shelfPos = shelfObject.transform.position;
            isNewShelf = !seenShelves.Contains(shelfObject);
        }

        HashSet<GameObject> scanResults = eyes.ShortSightedScan();
        List<GameObject> objectsByDistance = eyes
            .ShortSightedScan()
            .ToList()
            .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
            .ToList();

        // Find the closest 3 items and store the data of these.
        for (int i = 0; i < Mathf.Min(objectsByDistance.Count, 3); i++)
        {
            Item item = objectsByDistance[i].GetComponent<Item>();
            savedValues[i] = item.SavedValue;
            itemSizes[i] = item.Size;
        }

        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(canSeeItem);
        sensor.AddObservation(itemPos);
        sensor.AddObservation(canSeeObject);
        sensor.AddObservation(objectPos);
        sensor.AddObservation(canSeeShelf);
        sensor.AddObservation(shelfPos);
        sensor.AddObservation(savedValues);
        sensor.AddObservation(itemSizes);
        sensor.AddObservation(isNewItem);
        sensor.AddObservation(isNewShelf);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // How much to move left / right
        // (positive = left / negative = right)
        float leftRightMovement;
        // How much to move forward / backwards
        // (positive = forwards / negative = backwards)
        float forwardBackMovement;
        // How much to rotate along the yAxis
        float yAxisRotation = actions.DiscreteActions[2] - 360;

        if (actions.DiscreteActions[0] == 1)
        {
            leftRightMovement = 0.0f;
        }
        else if (actions.DiscreteActions[0] == 2)
        {
            leftRightMovement = 1.0f;
        }
        else
        {
            leftRightMovement = -1.0f;
        }

        if (actions.DiscreteActions[1] == 1)
        {
            forwardBackMovement = 0.0f;
        }
        else if (actions.DiscreteActions[1] == 2)
        {
            forwardBackMovement = 1.0f;
        }
        else
        {
            forwardBackMovement = -1.0f;
        }

        // Left / right movement
        shopper.Move(Quaternion.AngleAxis(-90.0f, Vector3.up) *
            transform.forward *
            leftRightMovement *
            Time.fixedDeltaTime);

        // Forward / back movement
        Vector3 before = transform.position;
        shopper.Move(forwardBackMovement * Time.fixedDeltaTime * transform.forward);
        Vector3 after = transform.position;

        if (Vector3.Distance(before, after) < 0.01f)
        {
            AddReward(-0.5f);
        }

        // Rotation
        transform.Rotate(0.0f, yAxisRotation * Time.fixedDeltaTime, 0.0f);

        List<GameObject> scanResultsList = eyes.ShortSightedScan().ToList();
        HashSet<GameObject> longSightedLookResults = eyes.LongSightedLook();

        if (longSightedLookResults.Count != 0)
        {
            GameObject longSightObject = longSightedLookResults.First();

            if (longSightObject.layer == LayerMask.NameToLayer("Shelf"))
            {
                AddReward(0.005f);

                if (!seenShelves.Contains(longSightObject))
                {
                    AddReward(0.022f);
                }
            }

        }

        GameObject jif = eyes.PeekDirectlyInFront(mask: shelfMask);

        if (jif != null)
        {
            seenShelves.Add(jif);
        }

        foreach (GameObject result in scanResultsList)
        {
            if (!seenItems.Contains(result))
            {
                AddReward(70.0f);
                seenItems.Add(result);
            }
            else
            {
                AddReward(-0.05f);
            }
        }

        if (seenItems.Count / totalItemCount >= TARGET_COVERAGE)
        {
            AddReward(100.0f);
            EndEpisode();
        }
    }

    private IEnumerator DelayTotalItemCounting()
    {
        // Wait one frame
        yield return null;

        totalItemCount = FindObjectsByType<Item>(FindObjectsSortMode.None).Length;
    }
}
