using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController characterController;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isRunning;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 moveInput = ReadMoveInput();
        bool jumpInput = ReadJumpInput();
        isRunning = ReadRunInput();

        CheckGrounded();
        HandleJump(jumpInput);
        ApplyGravity();

        Vector3 worldDirection = ConvertDirectionToWorldSpace(moveInput);
        Move(worldDirection);
    }

    private Vector2 ReadMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        return new Vector2(horizontal, vertical);
    }

    private bool ReadJumpInput()
    {
        return Input.GetButtonDown("Jump");
    }

    private bool ReadRunInput()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    private void CheckGrounded()
    {
        isGrounded = characterController.isGrounded;
    }

    private void HandleJump(bool jumpInput)
    {
        if (jumpInput && isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private Vector3 ConvertDirectionToWorldSpace(Vector2 input)
    {
        Vector3 direction = transform.right * input.x + transform.forward * input.y;

        return direction.normalized;
    }

    private float GetCurrentMoveSpeed()
    {
        return isRunning ? runSpeed : moveSpeed;
    }

    private void Move(Vector3 worldDirection)
    {
        float currentSpeed = GetCurrentMoveSpeed();

        Vector3 horizontalVelocity = worldDirection * currentSpeed;
        Vector3 verticalVelocityVector = Vector3.up * verticalVelocity;
        Vector3 finalVelocity = horizontalVelocity + verticalVelocityVector;

        characterController.Move(finalVelocity * Time.deltaTime);
    }
}