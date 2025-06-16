using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class Entity : MonoBehaviour
{
    /// <summary>
    /// Player's speed settings.
    /// </summary>
    protected float speed = 4f;
    /// <summary>
    /// Player's rotation speed settings.
    /// </summary>
    protected float rotationSpeed = 5f;
    /// <summary>
    /// Player jump settings.
    /// </summary>
    protected float jumpForce = .5f;
    /// <summary>
    /// Player's gravity settings.
    /// </summary>
    protected const int gravity = 10;
    /// <summary>
    /// Player's character controller.
    /// </summary>
    protected CharacterController controller;
    /// <summary>
    /// Weapon currently equipped by the player.
    /// </summary>
    private Weapon weapon = Weapon.None;
    protected string CurrentWeapon => weapon.ToString(); // Retourne le nom de l'arme actuelle
    /// <summary>
    /// The arrow prefab to be instantiated when the player uses a bow.
    /// </summary>
    public static GameObject ArrowPrefab;
    /// <summary>
    /// Player's points.
    /// </summary>
    private int _points;
    public int BonusPoints
    {
        get => nPlayer == null ? _points : nPlayer.BonusPoints;
        protected set
        {
            if (nPlayer == null)
                _points = value;
            else
                nPlayer.BonusPoints = value;
        }
    }
    /// <summary>
    /// The input for player movement.
    /// </summary>
    protected Vector3 movementInput;
    protected Vector3 previousPosition;
    /// <summary>
    /// The velocity of the player's movement.
    /// </summary>
    protected Vector3 movementVelocity => (transform.position - previousPosition) / Time.deltaTime;
    /// <summary>
    /// Animation controller for player actions.
    /// </summary>
    protected Animator animator;
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
    protected Dictionary<string, int> trapQuantities = new()
    {
        { "Mud", 1 },
        { "Spike", 1 },
    };

    /// <summary>
    /// The mud and spike game objects for the player.
    /// </summary>
    public static GameObject MudPrefab;

    public static GameObject SpikePrefab;

    /// <summary>
    /// The sword and bow game objects for the player.
    /// </summary>
    private GameObject Sword;
    private GameObject Bow;

    [HideInInspector]
    public NPlayer nPlayer;


    protected virtual bool Start()
    {
        GameManager.Instance.Players.Add(this);

        // Weapons held
        Sword = transform.GetComponentsInChildren<Transform>().FirstOrDefault(child => child.name == "Sword")?.gameObject;
        Bow = transform.GetComponentsInChildren<Transform>().FirstOrDefault(child => child.name == "Bow")?.gameObject;
        Sword.SetActive(weapon == Weapon.Sword); // Start with the sword hidden or shown based on the weapon
        Bow.SetActive(weapon == Weapon.Bow); // Start with the bow hidden or shown based on the weapon

        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (TryGetComponent(out nPlayer))
            if (!nPlayer.isLocalPlayer)
            {
                enabled = false;
                return false;
            }

        // Load the trap prefabs
        if (MudPrefab == null)
            MudPrefab = Resources.Load<GameObject>("Props/Traps/Mud/Mud");
        if (SpikePrefab == null)
            SpikePrefab = Resources.Load<GameObject>("Props/Traps/SpikeTrap/SpikeTrap");
        if (ArrowPrefab == null)
            ArrowPrefab = Resources.Load<GameObject>("Props/Arrow/Arrow");
        return true;
    }

    private void PlayAnimation(string name)
    {
        animator?.Play(name);
        nPlayer?.CmdSetAnim(name);
    }

    protected IEnumerator ResetAnimator()
    {
        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.speed = 1;
        states["Attack"] = false;
        states["Hit"] = false;
        if (states["Run"])
            PlayAnimation("Run");
        else if (states["Walk"])
            PlayAnimation("Walk");
        else
            PlayAnimation("Idle");
    }

    private void ResetPos()
    {
        controller.enabled = false; // Désactiver le CharacterController pour éviter les problèmes de collision            
        transform.position = new Vector3(3.7f, 5f, 0); // Réinitialiser la position du joueur
        AddPoints(-5); // Pénalité de points pour la mort
        controller.enabled = true; // Réactiver le CharacterController
    }

    private void Update()
    {
        if (transform.position.y <= -20)
        {
            ResetPos();
            return;
        }

        previousPosition = transform.position;

        if (!(movementInput.x == 0 && movementInput.z == 0))
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new(movementInput.x, 0, movementInput.z)), rotationSpeed * Time.deltaTime);

        if (controller.isGrounded && movementInput.y < 0)
            movementInput.y = -1;
        movementInput.y -= gravity * Time.deltaTime;

        float i = states["Walk"] || states["Mud"] ? 0.4f : 1f; // Adjust speed based on walking or running state
        if (states["Slow"]) i /= 2;

        if (controller.enabled)
            controller.Move(movementInput * speed * i * Time.deltaTime);
    }

    private Transform FindTargetInView(float viewAngle = 50, float viewDistance = 10)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance); // Détection dans un rayon
        Transform bestTarget = null;
        float bestAngle = viewAngle; // Angle maximal autorisé

        foreach (Collider col in colliders)
        {
            if (!col.CompareTag("Player") || col.transform == transform)
                continue; // Ignorer les objets qui ne sont pas des joueurs ou soi-même

            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            if (angle < viewAngle * 0.5f && angle < bestAngle) // Si la cible est bien devant
            {
                bestTarget = col.transform; // Cible potentielle
                bestAngle = angle; // Garder la meilleure cible dans le champ de vision
            }
        }
        return bestTarget; // Retourner la meilleure cible ou null
    }

    private List<Transform> FindTargetsInView(float viewAngle = 100, float viewDistance = 2)
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

    private void FireArrow(Transform target, float predictionTime)
    {
        var targetPosition = target ? target.position + Vector3.up * 0.5f : transform.position + transform.forward * 50; // Si pas de cible, tirer droit devant
        var arrowDirection = targetPosition - transform.position;
        var arrowPosition = transform.position + Vector3.up * 0.5f;
        var arrowRotation = Quaternion.LookRotation(arrowDirection) * Quaternion.Euler(0, 90f, 0);
        
        // Instancier la flèche et lui appliquer une "force"
        if (nPlayer == null)
            Instantiate(ArrowPrefab, arrowPosition, arrowRotation).AddComponent<Arrow>().Initialize(targetPosition + arrowDirection * 20, this);
        else
            nPlayer.CmdSpawnArrow(arrowPosition, arrowRotation, targetPosition + arrowDirection * 20);
    }

    public virtual void EquipWeapon(string newWeapon)
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
    }

    public void LaunchArrow()
    {
        if (weapon != Weapon.Bow)
            return;

        var t = FindTargetInView();
        if (t != null && t.TryGetComponent(out Ennemy e))
            e.PlayerIsShootingBow = true; // Indiquer que le joueur tire une flèche
        FireArrow(t, 0.5f); // Tirer une flèche
    }

    public void SwordAttack()
    {
        if (weapon != Weapon.Sword || (nPlayer && !nPlayer.isLocalPlayer))
            return;

        foreach (Transform target in FindTargetsInView())
            if (target.TryGetComponent(out Entity e)) // Vérifier si la cible est un joueur
            {
                int damages = Mathf.Min(5, e.BonusPoints);
                e.TakeDamage(damages); // Infliger des dégâts au joueur
                AddPoints(damages); // Ajouter les points du joueur à soi-même
            }
    }

    public void Punch()
    {
        if (weapon != Weapon.None || (nPlayer && !nPlayer.isLocalPlayer))
            return;

        try
        {
            if (FindTargetInView(50, 2).TryGetComponent(out Entity e)) // Vérifier si la cible est un joueur
                e.TakeDamage(0);
        }
        catch { }
    }

    public void PlaySound(string name) => GameMaster.PlayClip2D(name);

    public void TakeDamage(int damage)
    {
        AddPoints(-damage); // Soustraire les points en fonction des dégâts subis
        states["Hit"] = true;
        PlayAnimation("Hit");
        StartCoroutine(ResetAnimator());
    }

    public virtual void AddPoints(int points) => BonusPoints = Mathf.Max(BonusPoints + points, 0);

    public virtual void ThrowTrap(string trapType)
    {
        if (trapQuantities.ContainsKey(trapType) && trapQuantities[trapType] > 0)
        {
            GameObject trapPrefab = trapType == "Mud" ? MudPrefab : SpikePrefab;
            if (trapPrefab != null)
            {
                trapQuantities[trapType]--;
                GameMaster.PlayClip2D(trapType);

                if (nPlayer == null)
                    Instantiate(trapPrefab, transform.position - transform.forward * 1.5f + Vector3.up * 0.1f, Quaternion.identity);
                else
                    nPlayer.CmdSpawnObject(trapType, transform.position - transform.forward * 1.5f + Vector3.up * 0.1f, Quaternion.identity);
            }
        }
    }

    public virtual void AddTrap(string trapType)
    {
        if (trapQuantities.ContainsKey(trapType))
            trapQuantities[trapType]++;
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Mud"))
        {
            foreach (var state in states.Keys.ToList())
                states[state] = false; // Réinitialiser tous les états
            states["Mud"] = true; // Activer l'état de boue
            animator.speed = 1; // Réinitialiser la vitesse de l'animation
            PlayAnimation("Idle");
            yield return new WaitForSeconds(3f); // Attendre 3 secondes pour simuler l'effet de la boue
            states["Mud"] = false; // Désactiver l'état de boue
            movementInput = Vector3.zero; // Arrêter le mouvement du joueur

            TrapAnimation(); // Démarrer l'animation de disparition du piège
        }
        else if (other.gameObject.CompareTag("Spike"))
        {
            states["Slow"] = true; // Activer l'état de ralentissement
            yield return new WaitForSeconds(3f); // Attendre 3 secondes pour simuler l'effet de ralentissement
            states["Slow"] = false; // Désactiver l'état de ralentissement

            TrapAnimation(); // Démarrer l'animation de disparition du piège
        }

        void TrapAnimation()
        {
            if (other && other.TryGetComponent(out Animator anim))
            {
                anim.Play("TrapFadeOut"); // Jouer l'animation de disparition du piège

                // Détruire l'objet bonus après application
                if (other)
                    if (nPlayer == null)
                        Destroy(other.gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
                    else
                        nPlayer.CmdDestroyObject(other.gameObject, 0);
            }
        }
    }

    protected void InitMove(Vector2 input)
    {
        if (states["Attack"] || states["Hit"] || states["Mud"])
            return;

        movementInput = new(input.x, 0, input.y);
        string state = weapon == Weapon.Bow ? "Walk" : "Run";
        states[state] = true;
        PlayAnimation(state);
        animator.speed = 1;
    }

    protected void StopMove()
    {
        if (states["Mud"])
            return;

        movementInput = Vector3.zero;
        states["Walk"] = states["Run"] = false;
        PlayAnimation("Idle");
        animator.speed = 1;
    }

    public void Jump()
    {
        if (!controller.isGrounded || states["Attack"] || states["Hit"] || states["Mud"])
            return;

        PlayAnimation("Jump");
        animator.speed = 2f / jumpForce;
        movementInput.y = Mathf.Sqrt(jumpForce * 2f * gravity);
        StartCoroutine(ResetAnimator());
    }

    public void Attack()
    {
        if (!controller.isGrounded || states["Attack"] || states["Hit"] || states["Mud"])
            return;

        string attackAnimation = weapon switch
        {
            Weapon.Sword => "Slash",
            Weapon.Bow => "Shot",
            _ => "Punch"
        };
        PlayAnimation(attackAnimation);
        animator.speed = 1;
        states["Attack"] = true;
        StartCoroutine(ResetAnimator());
    }

    public abstract string GetName();
}

enum Weapon
{
    None,
    Sword,
    Bow,
}
