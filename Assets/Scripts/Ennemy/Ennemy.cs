using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ennemy : Entity
{
    private readonly float attackRange = 2f;
    private readonly float decisionInterval = 0.5f;
    private float lastJumpTime;

    private readonly float jumpCooldown = 1;
    [SerializeField]
    private Transform player;
    private Transform closestTreasure;
    private bool isDodging;
    private bool isJumping;
    [HideInInspector]
    public bool PlayerIsShootingBow = false; // Placeholder for player bow shooting detection

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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Priorité 2: Si le joueur attaque avec un arc, esquiver
        if (PlayerIsShootingBow)
        {
            StartCoroutine(Dodge());
            return;
        }

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
