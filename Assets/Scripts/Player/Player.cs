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
    /// The arrow prefab to be instantiated when the player uses a bow.
    /// </summary>
    [SerializeField]
    private GameObject ArrowPrefab;

    /// <summary>
    /// The input for player movement.
    /// </summary>
    private Vector3 movementInput;
    
    private Vector3 previousPosition;    
    /// <summary>
    /// The velocity of the player's movement.
    /// </summary>
    private Vector3 movementVelocity => (transform.position - previousPosition) / Time.deltaTime;

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

    void Update()
    {
        previousPosition = transform.position;

        if (!(movementInput.x == 0 && movementInput.z == 0))
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new(movementInput.x, 0, movementInput.z)), rotationSpeed * Time.deltaTime);

        if (controller.isGrounded && movementInput.y < 0)
            movementInput.y = -1;
        movementInput.y -= gravity * Time.deltaTime;

        float i = states["Walk"] ? 0.5f : 1f; // Adjust speed based on walking or running state
        controller.Move(movementInput * speed * i * Time.deltaTime);
    }

    Transform FindTargetInView(float viewAngle = 50, float viewDistance = 10)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance); // Détection dans un rayon
        Transform bestTarget = null;
        float bestAngle = viewAngle; // Angle maximal autorisé

        foreach (Collider col in colliders)
        {
            if (!col.CompareTag("Player") || col.transform == transform)
                continue; // Ignorer les objets qui ne sont pas des joueurs ou soi-même

            Transform target = col.transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle < viewAngle * 0.5f && angle < bestAngle) // Si la cible est bien devant
            {
                bestTarget = target;
                bestAngle = angle; // Garder la meilleure cible dans le champ de vision
            }
        }
        return bestTarget; // Retourner la meilleure cible ou null
    }

    List<Transform> FindTargetsInView(float viewAngle = 100, float viewDistance = 2)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance); // Détection dans un rayon
        List<Transform> targets = new();
        foreach (Collider col in colliders)
        {
            if (!col.CompareTag("Player") || col.transform == transform)
                continue; // Ignorer les objets qui ne sont pas des joueurs ou soi-même

            float angle = Vector3.Angle(transform.forward, (col.transform.position - transform.position).normalized);
            if (angle < viewAngle * 0.5f)
                targets.Add(col.transform); // Ajouter la cible si elle est dans le champ de vision
        }
        return targets;
    }

    void FireArrow(Transform target, float predictionTime)
    {
        Debug.Log($"Firing arrow at target: {target?.name}");
        Vector3 targetPosition = target ?
            targetPosition = target.position + Vector3.up * 0.5f /*+ target.GetComponent<Player>().movementVelocity * predictionTime*/ :
            transform.position + transform.forward * 50; // Si pas de cible, tirer droit devant

        // Instancier la flèche et lui appliquer une force
        GameObject arrow = Instantiate(ArrowPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        arrow.transform.up = targetPosition - transform.position; // Orienter la flèche vers la cible
        arrow.GetComponent<Arrow>().Initialize(targetPosition, gameObject); // Initialiser la flèche avec la position de la cible et le lanceur
    }

    public void LaunchArrow()
    {
        if (weapon != Weapon.Bow)
            return;

        FireArrow(FindTargetInView(), 0.5f); // Tirer une flèche courbée
    }

    public void SwordAttack()
    {
        if (weapon != Weapon.Sword)
            return;

        foreach (Transform target in FindTargetsInView())
            if (target.TryGetComponent(out Player player)) // Vérifier si la cible est un joueur
                player.TakeDamage(10); // Infliger des dégâts au joueur
    }

    public void Punch()
    {
        if (weapon != Weapon.None)
            return;

        if (FindTargetInView(50, 2).TryGetComponent(out Player player)) // Vérifier si la cible est un joueur
            player.TakeDamage(5); // Infliger des dégâts au joueur
    }

    public void TakeDamage(int damage)
    {
        // Logique pour gérer les dégâts subis par le joueur
        Debug.Log($"Player took {damage} damage!");
        states["Hit"] = true;
        animator.Play("Hit");
        StartCoroutine(ResetAnimator());
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
