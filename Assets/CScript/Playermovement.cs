using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Playermovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runningSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    private float verticalVelocity;

    public Vector2 moveInput;
    public Animator animator;
    public CharacterController characterController;

    private Playercontrol playerControl;
    private Vector3 moveDirection;
    public bool isRunning = false;

    private void OnEnable()
    {
        // Initialize Player Control and Bind Actions
        playerControl = new Playercontrol();

        // Movement Input
        playerControl.PlayerActionMap.PlayerActionGlobal.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerControl.PlayerActionMap.PlayerActionGlobal.canceled += ctx => moveInput = Vector2.zero;

        // Jump Action
        playerControl.PlayerActionMap.jump.performed += _ => Jump();

        // Sprint Action
        playerControl.PlayerActionMap.sprint.started += _ => isRunning = true;
        playerControl.PlayerActionMap.sprint.canceled += _ => isRunning = false;

        playerControl.Enable();
    }

    private void OnDisable()
    {
        playerControl.Disable();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Reset the Jump trigger at the start of the game
        animator.ResetTrigger("Jump");
    }

    void Update()
    {
        Move();
        Animate();
    }

    private void Move()
    {
        // Horizontal movement
        float targetSpeed = isRunning ? runningSpeed : walkSpeed;
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

        // Rotate character to face movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        moveDirection = transform.TransformDirection(moveDirection) * targetSpeed;

        // Apply gravity
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small negative value to keep grounded
        }
        verticalVelocity += gravity * Time.deltaTime;

        moveDirection.y = verticalVelocity;

        // Move character
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void Jump()
    {
        if (characterController.isGrounded)
        {
            // Apply upward velocity for the jump
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Trigger the jump animation
            animator.SetTrigger("jump");
        }
    }

    private void Animate()
    {
        // Calculate Speed based on movement input and toggle between walking/running
        float targetSpeed = isRunning ? runningSpeed : walkSpeed;
        float movementSpeed = moveInput.magnitude * targetSpeed;

        // Update Animator parameter
        animator.SetFloat("Speed", movementSpeed);
    }
}
