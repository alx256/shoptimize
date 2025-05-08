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
    private Vector3 returnPoint;
    private Quaternion returnRotation;
    private Queue<Operation> operationQueue = new();

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
            OperationAction currentAction = currentOperation.action;
            Item itemOfInterest = currentOperation.itemOfInterest;

            if (currentAction != OperationAction.NONE)
            {
                if (currentAction == OperationAction.FETCH && Scoreboard.Instance.AddItem(gameObject, itemOfInterest) ||
                    currentAction == OperationAction.DISCARD && Scoreboard.Instance.RemoveItem(gameObject, itemOfInterest))
                {
                    currentOperation.action = OperationAction.NONE;
                    isReturning = true;
                    ClearDestination();
                    navMeshAgent.SetDestination(returnPoint);
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

        if (currentOperation.action != OperationAction.NONE || isReturning)
        {
            navMeshAgent.Move(transform.forward * Time.deltaTime);
        }
        else if (operationQueue.Count != 0)
        {
            Operation operation = operationQueue.Dequeue();
            currentOperation = operation;
            navMeshAgent.SetDestination(operation.itemOfInterest.transform.position);

            returnPoint = transform.position;
            returnRotation = transform.rotation;
            SetStrategiesEnabled(false);
        }
    }

    public void Fetch(Item item)
    {
        Operation operation = new();
        operation.action = OperationAction.FETCH;
        operation.itemOfInterest = item;
        operationQueue.Enqueue(operation);
    }

    public void Discard(Item item)
    {
        Operation operation = new();
        operation.action = OperationAction.DISCARD;
        operation.itemOfInterest = item;
        operationQueue.Enqueue(operation);
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
