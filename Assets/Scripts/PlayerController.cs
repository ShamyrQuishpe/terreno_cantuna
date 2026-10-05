using UnityEngine;

// Movimiento sencillo como el tercer tutorial: W/S caminan y A/D giran.
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float turnSpeed = 110f;
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private float verticalSpeed;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float forwardInput = Input.GetAxisRaw("Vertical");
        float turnInput = Input.GetAxisRaw("Horizontal");

        transform.Rotate(0f, turnInput * turnSpeed * Time.deltaTime, 0f);

        if (characterController.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -2f;
        else
            verticalSpeed += gravity * Time.deltaTime;

        Vector3 horizontalMotion = transform.forward * (forwardInput * moveSpeed);
        Vector3 motion = horizontalMotion + Vector3.up * verticalSpeed;
        characterController.Move(motion * Time.deltaTime);
    }
}
