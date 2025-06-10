using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Player's speed settings.
    /// </summary>
    [SerializeField]
    private float speed = .5f;
    /// <summary>
    /// Player's rotation speed settings.
    /// </summary>
    [SerializeField]
    private float rotationSpeed = 5f;
    /// <summary>
    /// Player jump settings.
    /// </summary>
    [SerializeField]
    private float jumpForce = 5f;
    /// <summary>
    /// Player's gravity settings.
    /// </summary>
    private const int gravity = 10;
    /// <summary>
    /// Player's character controller.
    /// </summary>
    private CharacterController controller;

    /// <summary>
    /// Input system for player actions.
    /// </summary>
    private PlayerInputActions inputActions;

    /// <summary>
    /// Weapon currently equipped by the player.
    /// </summary>
    private Weapon weapon = Weapon.None;

    /// <summary>
    /// The input for player movement.
    /// </summary>
    private Vector3 movementInput;

    /// <summary>
    /// Animation controller for player actions.
    /// </summary>
    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inputActions = new();
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx =>
        {
            var input = ctx.ReadValue<Vector2>();
            movementInput = new(input.x, 0, input.y);
            animator.Play(weapon == Weapon.Bow ? "Walk" : "Run");
        };
        inputActions.Player.Move.canceled += ctx =>
        {
            movementInput = Vector3.zero;
            animator.Play("Idle");
        };
        inputActions.Player.Jump.performed += ctx =>
        {
            if (controller.isGrounded)
            {
                animator.Play("Jump");
                movementInput.y = Mathf.Sqrt(jumpForce * 2f * gravity);
            }
        };
    }

    void OnDisable() => inputActions.Player.Disable();

    void Update() => Move();

    void Move()
    {
        if (movementInput != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new(movementInput.x, 0, movementInput.z)), rotationSpeed * Time.deltaTime);
        
        if (controller.isGrounded && movementInput.y < 0)
            movementInput.y = -1;
        movementInput.y -= gravity * Time.deltaTime;
        controller.Move(movementInput * speed * Time.deltaTime);
    }
}

enum Weapon
{
    None,
    Sword,
    Bow,
}
