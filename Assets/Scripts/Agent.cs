using System;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public string agentName = "Unnamed Agent";

    private MovementStrategy[] movementStrategies;
    private SelectionStrategy[] selectionStrategies;
    private NavMeshAgent navMeshAgent;
    private bool isMovingTowardsDestination = false;
    private Vector3 destination;
    private Eyes eyes;
    private int shelfMask;

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
        eyes = GetComponent<Eyes>();

        shelfMask = 1 << LayerMask.NameToLayer("Shelf");
    }

    private void Update()
    {
        if (isMovingTowardsDestination)
        {
            GameObject directlyInFront = eyes.PeekDirectlyInFront(shelfMask);

            navMeshAgent.SetDestination(destination);
            transform.LookAt(new Vector3(destination.x, transform.position.y, destination.z));

            if (directlyInFront != null && directlyInFront.transform.position == destination)
            {
                isMovingTowardsDestination = false;
                SetStrategiesEnabled(true);
            }
        }
    }

    public void GoTo(GameObject target)
    {
        destination = target.transform.position;
        isMovingTowardsDestination = true;
        SetStrategiesEnabled(false);
    }

    public bool HasReachedPoint()
    {
        return !navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
            (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
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
