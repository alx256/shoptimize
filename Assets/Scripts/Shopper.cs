using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A class for generic shopper behaviour within the environment.
/// Contains helper methods for navigating and interacting with
/// the environment.
/// </summary>
public class Shopper : MonoBehaviour
{
    /// <summary>
    /// An action that can be conducted by a shopper.
    private enum OperationAction
    {
        /// <summary>
        /// Shopper is conducting no operation.
        /// </summary>
        NONE,
        /// <summary>
        /// Shopper is fetching an item, and moving towards
        /// it to get it.
        /// </summary>
        FETCH,
        /// <summary>
        /// Shopper is discarding an item, and moving towards
        /// it to put it back.
        /// </summary>
        DISCARD
    }

    /// <summary>
    /// An operation that can be conducted by a shopper.
    /// This involves doing something with an item of
    /// interest, such as fetching or discarding it.
    /// </summary>
    private struct Operation
    {
        /// <summary>
        /// The item of interest that this operation
        /// concerns. i.e. this operation involves the
        /// shopper doing something with this item.
        /// </summary>
        public Item itemOfInterest;
        /// <summary>
        /// The action that is being carried out on the
        /// <see cref="itemOfInterest"/>.
        /// </summary>
        public OperationAction action;
    }

    public string shopperName = "Unnamed Shopper";

    private List<MonoBehaviour> switchableComponents;
    private NavMeshAgent navMeshAgent;
    private Operation currentOperation;
    private bool isReturning = false;
    private bool wasMovingToDestination = false;
    private Quaternion targetRotation;
    private Vector3 returnPoint;
    private Vector3 originalDestination;
    private Quaternion returnRotation;
    private readonly Queue<Operation> operationQueue = new();

    private void Start()
    {
        Scoreboard.Instance.RegisterId(gameObject.GetInstanceID(), shopperName);

        switchableComponents.AddRange(GetComponents<MovementStrategy>());
        switchableComponents.AddRange(GetComponents<SelectionStrategy>());
        switchableComponents.AddRange(GetComponents<ReinforcementLearningShopper>());
        switchableComponents.AddRange(GetComponents<BehaviorParameters>());
        switchableComponents.AddRange(GetComponents<DecisionRequester>());
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
                // Returned to original point.
                // Return to (as close to) the original state
                // that the shopper was in before carrying out
                // an operation (as possible).
                isReturning = false;
                ClearDestination();
                transform.rotation = targetRotation;
                SetStrategiesEnabled(true);

                if (wasMovingToDestination)
                {
                    // Was moving towards some point, restore this
                    // behaviour.
                    navMeshAgent.SetDestination(originalDestination);
                }
            }
            else if (currentOperation.action != OperationAction.NONE)
            {
                // Reached the point belonging to an item of interest for some
                // operation. Do whatever we need to do to this item.
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
                // There are more operations queued.
                // Do the next one.
                Operation operation = operationQueue.Dequeue();
                currentOperation = operation;
                navMeshAgent.SetDestination(operation.itemOfInterest.transform.position);
                SetStrategiesEnabled(false);
            }
        }
    }

    /// <summary>
    /// Move this shopper in some direction.
    /// </summary>
    /// <param name="movement">The direction that this shopper should move
    /// in.</param>
    public void Move(Vector3 movement)
    {
        navMeshAgent.Move(movement);
    }

    /// <summary>
    /// Interrupt current strategies and go to fetch
    /// a specified item, adding it to the shopping
    /// cart, and returning back to the original
    /// location.
    /// </summary>
    /// <param name="item"></param>
    public void Fetch(Item item)
    {
        AddOperation(OperationAction.FETCH, item);
    }

    /// <summary>
    /// Interrupt current strategies and go to discard
    /// a specifieditem, removing it from the shopping
    /// cart, and returning back to the original
    /// location.
    /// </summary>
    /// <param name="item"></param>
    public void Discard(Item item)
    {
        AddOperation(OperationAction.DISCARD, item);
    }

    /// <summary>
    /// Returns <c>true</c> if this shopper has reached the point
    /// that it is moving towards.
    /// </summary>
    /// <returns><c>true</c> if this shopper has reached the point
    /// that it is moving towards.</returns>
    public bool HasReachedPoint()
    {
        return !navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
            (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
    }

    /// <summary>
    /// Stop the shopper from moving towards its destination.
    /// </summary>
    public void ClearDestination()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    /// <summary>
    /// Queue a given operation based on the
    /// <see cref="OperationAction"/> that should be performed to
    /// an <see cref="Item"/>. 
    /// </summary>
    /// <param name="action">The action that should be completed.</param>
    /// <param name="item">The item that this action should be performed
    /// to</param>
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

    /// <summary>
    /// Enable or disable strategies for this shopper.
    /// This can be used to halt or reinstate
    /// interference from the strategies that might have
    /// their own policies for movement and which actions
    /// should be completed.
    /// </summary>
    /// <param name="enabled">Set this to <c>true</c> to
    /// enable strategies and <c>false</c> to disable.</param>
    private void SetStrategiesEnabled(bool enabled)
    {
        for (int i = 0; i < switchableComponents.Count; i++)
        {
            switchableComponents[i].enabled = enabled;
        }
    }
}
