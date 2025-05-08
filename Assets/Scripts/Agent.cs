using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    private enum OperationAction
    {
        NONE,
        FETCH,
        DISCARD
    }

    private struct Operation
    {
        public Item itemOfInterest;
        public OperationAction action;
    }

    public string agentName = "Unnamed Agent";

    private MovementStrategy[] movementStrategies;
    private SelectionStrategy[] selectionStrategies;
    private NavMeshAgent navMeshAgent;
    private Operation currentOperation;
    private bool isReturning = false;
    private bool wasMovingToDestination = false;
    private Quaternion targetRotation;
    private Vector3 returnPoint;
    private Vector3 originalDestination;
    private Quaternion returnRotation;
    private readonly Queue<Operation> operationQueue = new();

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

        currentOperation = new();
        currentOperation.action = OperationAction.NONE;
    }

    private void Update()
    {
        if (HasReachedPoint())
        {
            if (isReturning)
            {
                isReturning = false;
                ClearDestination();
                transform.rotation = targetRotation;
                SetStrategiesEnabled(true);

                if (wasMovingToDestination)
                {
                    navMeshAgent.SetDestination(originalDestination);
                }
            }
            else if (currentOperation.action != OperationAction.NONE)
            {
                if (currentOperation.action == OperationAction.DISCARD &&
                    Scoreboard.Instance.RemoveItem(gameObject, currentOperation.itemOfInterest) ||
                    currentOperation.action == OperationAction.FETCH &&
                    Scoreboard.Instance.AddItem(gameObject, currentOperation.itemOfInterest))
                {
                    currentOperation.action = OperationAction.NONE;
                    ClearDestination();

                    if (operationQueue.Count == 0)
                    {
                        isReturning = true;
                        targetRotation = returnRotation;
                        navMeshAgent.SetDestination(returnPoint);
                    }
                }
            }
            else if (operationQueue.Count != 0)
            {
                Operation operation = operationQueue.Dequeue();
                currentOperation = operation;
                navMeshAgent.SetDestination(operation.itemOfInterest.transform.position);
                SetStrategiesEnabled(false);
            }
        }
    }

    public void Fetch(Item item)
    {
        AddOperation(OperationAction.FETCH, item);
    }

    public void Discard(Item item)
    {
        AddOperation(OperationAction.DISCARD, item);
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

    private void AddOperation(OperationAction action, Item item)
    {
        Operation operation = new()
        {
            action = action,
            itemOfInterest = item
        };
        operationQueue.Enqueue(operation);
        returnPoint = transform.position;
        returnRotation = transform.rotation;
        originalDestination = navMeshAgent.destination;
        wasMovingToDestination = navMeshAgent.hasPath;
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
