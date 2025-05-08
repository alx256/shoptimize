using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public string agentName = "Unnamed Agent";

    private MovementStrategy[] movementStrategies;
    private SelectionStrategy[] selectionStrategies;
    private NavMeshAgent navMeshAgent;
    private bool isFetching = false;
    private bool isDiscarding = false;
    private bool isReturning = false;
    private Vector3 returnPoint;
    private Quaternion returnRotation;
    private Item itemOfInterest;
    private Queue<Item> fetchQueue = new();
    private Queue<Item> discardQueue = new();

    public void Move(Vector3 movement)
    {
        navMeshAgent.Move(movement);
    }

    private void Start()
    {
        Scoreboard.Instance.RegisterId(gameObject.GetInstanceID(), agentName);

        movementStrategies = GetComponents<MovementStrategy>();
        selectionStrategies = GetComponents<SelectionStrategy>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (HasReachedPoint())
        {
            if (isFetching || isDiscarding)
            {
                isReturning = true;
                ClearDestination();
                navMeshAgent.SetDestination(returnPoint);

                if (isFetching)
                {
                    isFetching = false;
                    Scoreboard.Instance.AddItem(gameObject, itemOfInterest);
                }
                else if (isDiscarding)
                {
                    isDiscarding = false;
                    Scoreboard.Instance.RemoveItem(gameObject, itemOfInterest);
                }
            }

            if (isReturning)
            {
                isReturning = false;
                ClearDestination();
                transform.rotation = returnRotation;
                SetStrategiesEnabled(true);
            }
        }

        if (isFetching || isReturning || isDiscarding)
        {
            navMeshAgent.Move(transform.forward * Time.deltaTime);
        }
        else if (fetchQueue.Count != 0 || discardQueue.Count != 0)
        {
            if (fetchQueue.Count != 0)
            {
                isFetching = true;
            }
            else if (discardQueue.Count != 0)
            {
                isDiscarding = true;
            }

            Item item = (fetchQueue.Count != 0) ? fetchQueue.Dequeue() : discardQueue.Dequeue();
            navMeshAgent.SetDestination(item.transform.position);

            returnPoint = transform.position;
            returnRotation = transform.rotation;
            itemOfInterest = item;
            SetStrategiesEnabled(false);
        }
    }

    public void Fetch(Item item)
    {
        fetchQueue.Enqueue(item);
    }

    public void Discard(Item item)
    {
        discardQueue.Enqueue(item);
    }

    public bool HasReachedPoint()
    {
        return !navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
            (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
    }

    public void ClearDestination()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    private void SetStrategiesEnabled(bool enabled)
    {
        for (int i = 0; i < movementStrategies.Length; i++)
        {
            movementStrategies[i].enabled = enabled;
        }

        for (int i = 0; i < selectionStrategies.Length; i++)
        {
            selectionStrategies[i].enabled = enabled;
        }
    }
}
