using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ennemy : Entity
{
    private readonly int attackRange = 4;
    private readonly int escapeRange = 20;
    private readonly float decisionInterval = 0.5f;
    private readonly int jumpCooldown = 1;

    private float lastJumpTime;
    private Transform closestTreasure;
    private bool isDodging;
    private bool isJumping;
    private Vector2 escapeDirection = Vector2.zero;

    [HideInInspector]
    public bool PlayerIsShootingBow = false; // Placeholder for player bow shooting detection

    [SerializeField]
    private Transform player;

    protected override bool Start()
    {
        base.Start();
        InvokeRepeating(nameof(MakeDecision), 0f, decisionInterval);
        InvokeRepeating(nameof(ThrowTrap), 10f, 10f);
        return true;
    }

    void OnDisable() => CancelInvoke(nameof(MakeDecision));

    void ThrowTrap() => ThrowTrap(Random.Range(0, 2) == 0 ? "Mud" : "Spike");

    void MakeDecision()
    {
        if (isDodging) return;
        if (isJumping)
            if (controller.isGrounded)
                isJumping = false;
            else
                return;
        if (CheckForJump())
            return;

        FindClosestTreasure();

        // Priorité 1: Si un trésor est proche, aller vers lui
        if (closestTreasure != null)
        {
            InitMove(GetDirectionTo(closestTreasure.position));
            return;
        }

        // Priorité 2: Si le joueur attaque avec un arc, esquiver
        if (PlayerIsShootingBow)
        {
            StartCoroutine(Dodge());
            return;
        }

        float distanceToPlayer = (transform.position - player.position).sqrMagnitude;

        // Priorité 3: Si proche du joueur, attaquer
        if (distanceToPlayer <= attackRange)
        {
            Attack();
            return;
        }
        

        // Priorité 4: Comportement selon l'arme
        switch (CurrentWeapon)
        {
            case "None": default:
                // Si désarmé, fuir
                if (distanceToPlayer <= escapeRange)
                    Escape();
                else
                    StopMove();
                return;
            case "Sword":
                InitMove(GetDirectionTo(player.position) * 0.75f);
                return;
            case "Bow":
                InitMove(GetDirectionTo(player.position) * 0.75f);
                Attack();
                return;
        }
    }

    void FindClosestTreasure()
    {
        float closestDistance = Mathf.Infinity;
        closestTreasure = null;

        foreach (Transform treasure in GameManager.SpawnedTreasures)
        {
            float distance = Vector3.Distance(transform.position, treasure.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTreasure = treasure;
            }
        }
    }

    Vector2 GetDirectionTo(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        return new Vector2(direction.x, direction.z).normalized;
    }

    IEnumerator Dodge()
    {
        isDodging = true;
        // Déplacement latéral ou reculer rapidement
        Vector2 directionToPlayer = GetDirectionTo(player.position);
        Vector2 dodgeDirection = new Vector2(-directionToPlayer.y, directionToPlayer.x) * (Random.value > 0.5f ? 1 : -1); // Gauche ou droite

        float dodgeTime = .5f;
        float timer = 0f;

        while (timer < dodgeTime)
        {
            InitMove(dodgeDirection * 0.75f);
            timer += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
        PlayerIsShootingBow = false; // Réinitialiser l'état de tir du joueur
    }

    void Escape()
    {
        if (escapeDirection == Vector2.zero)
            escapeDirection = -GetDirectionTo(player.position);

        Vector2 targetPosition = new Vector2(transform.position.x - 3.5f, transform.position.z) + 8 * escapeDirection;
        if (77 * Mathf.Pow(targetPosition.x, 2) + 81 * Mathf.Pow(targetPosition.y, 2) >= 6237)
            escapeDirection = (Quaternion.Euler(0, 90f, 0) * escapeDirection).normalized;
        InitMove(escapeDirection);
    }

    bool CheckForJump()
    {
        if (!controller.isGrounded || Time.time - lastJumpTime < jumpCooldown) return false;
        // Créer un rayon vers l'avant pour détecter un obstacle
        Ray ray = new Ray(transform.position + Vector3.up * 0.35f, transform.forward);

        // Si un obstacle est détecté dans un rayon de 1 unité
        if (Physics.Raycast(ray, 1f))
            if (Time.time - lastJumpTime > jumpCooldown) // Pour éviter de sauter en boucle
            {
                lastJumpTime = Time.time;
                isJumping = true;
                Jump();
                return true;
            }
        return false;
    }

    public override string GetName() => "Computer";
}
