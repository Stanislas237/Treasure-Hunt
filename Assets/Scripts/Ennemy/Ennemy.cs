using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ennemy : Entity
{
    private string currentWeapon = "None";
    private float attackRange = 2f;
    private float decisionInterval = 0.5f;

    [SerializeField]
    private Transform player;
    private Transform closestTreasure;
    private bool isDodging;
    public bool PlayerIsShootingBow = false; // Placeholder for player bow shooting detection

    void Start() => InvokeRepeating(nameof(MakeDecision), 0f, decisionInterval);

    void OnDisable() => CancelInvoke(nameof(MakeDecision));

    void MakeDecision()
    {
        if (isDodging) return;

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
        InitMove(GetDirectionTo(player.position));
        if (CurrentWeapon == "Bow")
        {
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
        Vector2 dodgeDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        float dodgeTime = 3f;
        float timer = 0f;

        while (timer < dodgeTime)
        {
            InitMove(dodgeDirection * 2f); // Dodge plus rapide
            timer += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
        PlayerIsShootingBow = false; // Réinitialiser l'état de tir du joueur
    }
}
