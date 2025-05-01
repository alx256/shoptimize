using UnityEngine;

public class BoustrophedonDecomposition : MonoBehaviour
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
