using UnityEngine;

public class BoustrophedonDecomposition : MovementStrategy
{
    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        characterController.Move(Vector3.forward);
    }
}
