using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ShelfExplore : MovementStrategy
{
    private enum Phase
    {
        LOOKING,
        MOVING,
        EXPLORING
    }

    private const float ROTATION_STEP = 45.0f;
    private const float ROTATE_AROUND_SCALAR = 2.0f;

    private Phase phase = Phase.LOOKING;
    private Eyes eyes;
    private Shopper shopper;
    private int shelfMask;
    private int itemMask;
    private int wallMask;
    private int agentMask;
    private Queue<GameObject> shelfQueue;
    private GameObject currentShelf;
    private HashSet<GameObject> shelfPool;
    private NavMeshAgent navMeshAgent;
    private bool isRotating = false;
    private float totalRotation = 0.0f;
    private Vector3 rotateAroundPoint;
    private GameObject lastViewedItem;
    private GameObject firstViewedItem;
    private int multiplier = 1;
    private int hits = 0;
    private int cycles = 0;
    private Vector3 previousPosition;

    private void Start()
    {
        eyes = GetComponent<Eyes>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        shopper = GetComponent<Shopper>();

        shelfMask = 1 << LayerMask.NameToLayer("Shelf");
        itemMask = 1 << LayerMask.NameToLayer("Item");
        wallMask = 1 << LayerMask.NameToLayer("Wall");
        agentMask = 1 << LayerMask.NameToLayer("Agent");

        shelfQueue = new();
        shelfPool = new();
        transform.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
    }

    private void Update()
    {
        switch (phase)
        {
            case Phase.LOOKING:
                Looking();
                break;
            case Phase.MOVING:
                Moving();
                break;
            case Phase.EXPLORING:
                Exploring();
                break;
        }
    }

    private void TransitionTo(Phase phase)
    {
        this.phase = phase;

        switch (phase)
        {
            case Phase.LOOKING:
                break;
            case Phase.MOVING:
                hits = 0;
                multiplier = 1;
                cycles = 0;
                firstViewedItem = null;

                if (shelfQueue.Count > 0)
                {
                    currentShelf = shelfQueue.Dequeue();
                    navMeshAgent.SetDestination(currentShelf.transform.position);
                }
                else
                {
                    TransitionTo(Phase.LOOKING);
                }
                break;
            case Phase.EXPLORING:
                break;
        }
    }

    private void Looking()
    {
        // Rotate around in circles until we find
        // a shelf
        transform.Rotate(0.0f, ROTATION_STEP * Time.deltaTime, 0.0f);
        LookForShelf();

        if (shelfQueue.Count > 0)
        {
            TransitionTo(Phase.MOVING);
        }
    }

    private void Moving()
    {
        if (shopper.HasReachedPoint())
        {
            transform.LookAt(new Vector3(currentShelf.transform.position.x, transform.position.y, currentShelf.transform.position.z));
            TransitionTo(Phase.EXPLORING);
            shopper.ClearDestination();
        }
    }

    private void Exploring()
    {
        if (isRotating)
        {
            transform.RotateAround(rotateAroundPoint, Vector3.up, -ROTATION_STEP * Time.deltaTime * multiplier);
            totalRotation += ROTATION_STEP * Time.deltaTime;

            if (totalRotation >= 90.0f)
            {
                // Fix over-rotation (if needed)
                transform.Rotate(0.0f, 90.0f - totalRotation * multiplier, 0.0f);
                isRotating = false;
                totalRotation = 0.0f;
            }

            return;
        }

        navMeshAgent.Move(Quaternion.AngleAxis(90.0f, Vector3.up) * transform.forward * Time.deltaTime * multiplier);

        HashSet<GameObject> lookingAtShelves = eyes.ShortSightedLook(mask: shelfMask);
        HashSet<GameObject> lookingAtItems = eyes.ShortSightedLook(mask: itemMask);

        transform.Rotate(0.0f, 90.0f * multiplier, 0.0f);
        GameObject agentDirectlyInFront = eyes.PeekDirectlyInFront(agentMask);
        GameObject wallDirectlyInFront = eyes.PeekDirectlyInFront(wallMask);
        GameObject itemDirectlyInFront = eyes.PeekDirectlyInFront(itemMask);
        transform.Rotate(0.0f, -90.0f * multiplier, 0.0f);

        if (agentDirectlyInFront)
        {
            multiplier *= -1;
        }
        if (itemDirectlyInFront)
        {
            transform.Rotate(0.0f, 90.0f * multiplier, 0.0f);
        }
        else if (wallDirectlyInFront != null)
        {
            multiplier *= -1;
            hits++;
        }
        else if (lookingAtShelves.Count == 0 && lastViewedItem != null)
        {
            isRotating = true;
            rotateAroundPoint = lastViewedItem.transform.position;
            // rotateAroundPoint = transform.position + Quaternion.AngleAxis(-90.0f, Vector3.up) * transform.forward * ROTATE_AROUND_SCALAR;
        }

        if (lookingAtItems.Count > 0)
        {
            if (lastViewedItem == null || lookingAtItems.First() != lastViewedItem && lookingAtItems.First() == firstViewedItem)
            {
                cycles++;
            }

            lastViewedItem = lookingAtItems.First();

            if (firstViewedItem == null)
            {
                firstViewedItem = lookingAtItems.First();
            }
        }

        if (hits == 2 || cycles == 3)
        {
            TransitionTo(Phase.MOVING);
        }

        transform.Rotate(0.0f, 180.0f, 0.0f);
        LookForShelf();
        transform.Rotate(0.0f, -180.0f, 0.0f);
    }

    private void LookForShelf()
    {
        HashSet<GameObject> longSightedObjs = eyes.LongSightedLook(mask: shelfMask);

        if (longSightedObjs.Count > 0)
        {
            GameObject shelf = longSightedObjs.First();

            if (!shelfPool.Contains(shelf))
            {
                shelfPool.Add(shelf);
                shelfQueue.Enqueue(shelf);
            }
        }
    }
}
