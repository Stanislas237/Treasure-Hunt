using System.Linq;
using System.Collections.Generic;
using System.Collections;
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
    [SerializeField]
    private Weapon weapon = Weapon.None;

    public int BonusPoints { get; private set; } = 0;

    /// <summary>
    /// UI component to display player points.
    /// </summary>
    PlayerUI playerUI;

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
        { "Mud", false },
        { "Slow", false },
    };

    /// <summary>
    /// Dictionary to manage player trap quantities.
    /// </summary>
    private Dictionary<string, int> trapQuantities = new()
    {
        { "Mud", 1 },
        { "Spike", 1 },
    };

    /// <summary>
    /// The mud and spike game objects for the player.
    /// </summary>
    private GameObject MudPrefab;

    private GameObject SpikePrefab;

    /// <summary>
    /// The sword and bow game objects for the player.
    /// </summary>
    private GameObject Sword;

    private GameObject Bow;

    void Start()
    {
        Sword = transform.GetComponentsInChildren<Transform>().FirstOrDefault(child => child.name == "Sword")?.gameObject;
        Bow = transform.GetComponentsInChildren<Transform>().FirstOrDefault(child => child.name == "Bow")?.gameObject;
        Sword.SetActive(false); // Start with the sword hidden
        Bow.SetActive(false); // Start with the bow hidden

        MudPrefab = Resources.Load<GameObject>("Props/Traps/Mud/Mud");
        SpikePrefab = Resources.Load<GameObject>("Props/Traps/SpikeTrap/SpikeTrap");

        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerUI = GetComponent<PlayerUI>();

        // Initialize input actions
        inputActions = new();
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx =>
        {
            if (states["Attack"] || states["Hit"] || states["Mud"])
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
            if (states["Mud"])
                return;

            movementInput = Vector3.zero;
            states["Walk"] = states["Run"] = false;
            animator.Play("Idle");
            animator.speed = 1;
        };
        inputActions.Player.Jump.performed += ctx =>
        {
            if (!controller.isGrounded || states["Attack"] || states["Hit"] || states["Mud"])
                return;

            animator.Play("Jump");
            animator.speed = 2f / jumpForce;
            movementInput.y = Mathf.Sqrt(jumpForce * 2f * gravity);
            StartCoroutine(ResetAnimator());
        };
        inputActions.Player.Attack.performed += ctx =>
        {
            if (!controller.isGrounded || states["Attack"] || states["Hit"] || states["Mud"])
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
        inputActions.Player.Interact.performed += ctx =>
        {
            if (trapQuantities["Mud"] > 0) ThrowTrap("Mud");
            else ThrowTrap("Spike");
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

        float i = states["Walk"] || states["Mud"] ? 0.4f : 1f; // Adjust speed based on walking or running state
        if (states["Slow"]) i /= 2;
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
        arrow.transform.right = targetPosition - transform.position; // Orienter la flèche vers la cible
        arrow.GetComponent<Arrow>().Initialize(targetPosition, gameObject); // Initialiser la flèche avec la position de la cible et le lanceur
    }

    public void EquipWeapon(string newWeapon)
    {
        switch (newWeapon)
        {
            case "Bow":
                weapon = Weapon.Bow;
                Bow.SetActive(true); // Activer le modèle de l'arc
                Sword.SetActive(false); // Désactiver le modèle de l'épée
                break;
            case "Sword":
                weapon = Weapon.Sword;
                Bow.SetActive(false); // Désactiver le modèle de l'arc
                Sword.SetActive(true); // Activer le modèle de l'épée
                break;
            default:
                weapon = Weapon.None;
                Bow.SetActive(false); // Désactiver le modèle de l'arc
                Sword.SetActive(false); // Désactiver le modèle de l'épée
                break;
        }

        playerUI.UpdateWeaponIcon(newWeapon); // Mettre à jour l'icône de l'arme dans l'interface utilisateur
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
            {
                int damages = Mathf.Min(5, player.BonusPoints);
                player.TakeDamage(damages); // Infliger des dégâts au joueur
                AddPoints(damages); // Ajouter les points du joueur à soi-même
            }
    }

    public void Punch()
    {
        if (weapon != Weapon.None)
            return;

        try
        {
            if (FindTargetInView(50, 2).TryGetComponent(out Player player)) // Vérifier si la cible est un joueur
                player.TakeDamage(0); // Infliger des dégâts au joueur
        }
        catch { }
    }

    public void TakeDamage(int damage)
    {
        AddPoints(-damage); // Soustraire les points en fonction des dégâts subis
        states["Hit"] = true;
        animator.Play("Hit");
        StartCoroutine(ResetAnimator());
    }

    public void AddPoints(int points)
    {
        BonusPoints += points;
        playerUI.UpdatePoints(BonusPoints); // Mettre à jour l'interface utilisateur avec les nouveaux points
    }

    public void ThrowTrap(string trapType)
    {
        if (trapQuantities.ContainsKey(trapType) && trapQuantities[trapType] > 0)
        {
            GameObject trapPrefab = trapType == "Mud" ? MudPrefab : SpikePrefab;
            if (trapPrefab != null)
            {
                trapQuantities[trapType]--;
                Instantiate(trapPrefab, transform.position - transform.forward + Vector3.up * 0.1f, Quaternion.identity);
                playerUI.UpdateTrapCounts(trapQuantities["Mud"], trapQuantities["Spike"]); // Mettre à jour l'interface utilisateur avec les quantités de pièges
            }
        }
    }

    public void AddTrap(string trapType)
    {
        if (trapQuantities.ContainsKey(trapType))
            trapQuantities[trapType]++;
        playerUI.UpdateTrapCounts(trapQuantities["Mud"], trapQuantities["Spike"]); // Mettre à jour l'interface utilisateur avec les quantités de pièges
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Respawn"))
        {
            controller.enabled = false; // Désactiver le CharacterController pour éviter les problèmes de collision            
            transform.position = new Vector3(3.7f, 5f, 0); // Réinitialiser la position du joueur
            AddPoints(-2); // Pénalité de points pour la mort
            controller.enabled = true; // Réactiver le CharacterController
        }
        else if (other.gameObject.CompareTag("Mud"))
        {
            foreach (var state in states.Keys.ToList())
                states[state] = false; // Réinitialiser tous les états
            states["Mud"] = true; // Activer l'état de boue
            animator.speed = 1; // Réinitialiser la vitesse de l'animation
            animator.Play("Idle");
            yield return new WaitForSeconds(3f); // Attendre 3 secondes pour simuler l'effet de la boue
            states["Mud"] = false; // Désactiver l'état de boue
            movementInput = Vector3.zero; // Arrêter le mouvement du joueur
        }
        else if (other.gameObject.CompareTag("Spike"))
        {
            states["Slow"] = true; // Activer l'état de ralentissement
            yield return new WaitForSeconds(3f); // Attendre 3 secondes pour simuler l'effet de ralentissement
            states["Slow"] = false; // Désactiver l'état de ralentissement
        }

        if (other.TryGetComponent(out Animator anim))
        {
            anim.Play("TrapFadeOut"); // Jouer l'animation de disparition du piège
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Attendre la fin de l'animation
            Destroy(other.gameObject); // Détruire l'objet bonus après application
        }
    }
}

enum Weapon
{
    None,
    Sword,
    Bow,
}
