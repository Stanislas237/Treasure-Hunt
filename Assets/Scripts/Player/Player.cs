using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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
    [SerializeField]
    private Weapon weapon = Weapon.None;

    /// <summary>
    /// Weapon currently equipped by the player.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI textPoints;
    private int BonusPoints;

    /// <summary>
    /// The input for player movement.
    /// </summary>
    private Vector3 movementInput;

    /// <summary>
    /// Animation controller for player actions.
    /// </summary>
    private Animator animator;

    /// <summary>
    /// Dictionary to manage player states and their animations.
    /// </summary>
    private Dictionary<string, bool> states = new()
    {
        { "Run", false },
        { "Walk", false },
        { "Hit", false },
        { "Attack", false },
    };

    private Dictionary<string, float> animLengths = new();
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Initialize input actions
        inputActions = new();
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx =>
        {
            if (states["Attack"] || states["Hit"])
                return;

            var input = ctx.ReadValue<Vector2>();
            movementInput = new(input.x, 0, input.y);

            string state = weapon == Weapon.Bow ? "Walk" : "Run";
            states[state] = true;
            animator.Play(state);
            animator.speed = 1;
        };
        inputActions.Player.Move.canceled += ctx =>
        {
            movementInput = Vector3.zero;
            states["Walk"] = states["Run"] = false;
            animator.Play("Idle");
            animator.speed = 1;
        };
        inputActions.Player.Jump.performed += ctx =>
        {
            if (!controller.isGrounded || states["Attack"] || states["Hit"])
                return;

            animator.Play("Jump");
            animator.speed = 2f / jumpForce;
            movementInput.y = Mathf.Sqrt(jumpForce * 2f * gravity);
            StartCoroutine(ResetAnimator());
        };
        inputActions.Player.Attack.performed += ctx =>
        {
            if (!controller.isGrounded || states["Attack"] || states["Hit"])
                return;

            string attackAnimation = weapon switch
            {
                Weapon.Sword => "Slash",
                Weapon.Bow => "Shot",
                _ => "Punch"
            };
            animator.Play(attackAnimation);
            animator.speed = 1;
            states["Attack"] = true;
            StartCoroutine(ResetAnimator());
        };
    }

    void OnDisable() => inputActions.Player.Disable();

    IEnumerator ResetAnimator()
    {
        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.speed = 1;
        states["Attack"] = false;
        states["Hit"] = false;
        if (states["Run"])
            animator.Play("Run");
        else if (states["Walk"])
            animator.Play("Walk");
        else
            animator.Play("Idle");
    }

    void Update() => Move();

    void Move()
    {
        if (!(movementInput.x == 0 && movementInput.z == 0))
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new(movementInput.x, 0, movementInput.z)), rotationSpeed * Time.deltaTime);

        if (controller.isGrounded && movementInput.y < 0)
            movementInput.y = -1;
        movementInput.y -= gravity * Time.deltaTime;
        controller.Move(movementInput * speed * Time.deltaTime);
    }
    
    public void AddPoints(int points)
    {
        BonusPoints += points;
        textPoints.text = $"Points : {BonusPoints}";
    }
}

enum Weapon
{
    None,
    Sword,
    Bow,
}
