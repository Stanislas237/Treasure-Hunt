using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{
    /// <summary>
    /// Input system for player actions.
    /// </summary>
    private PlayerInputActions inputActions;

    /// <summary>
    /// UI component to display player points.
    /// </summary>
    PlayerUI playerUI;

    protected override bool Start()
    {
        // Getting Player UI
        playerUI = GetComponent<PlayerUI>();

        // Parent's Start method
        if (!base.Start())
            return false;

        // Initialize input actions
        inputActions = new();
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => InitMove(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => StopMove();
        inputActions.Player.Jump.performed += ctx => Jump();
        inputActions.Player.Attack.performed += ctx => Attack();
        inputActions.Player.Interact.performed += ctx =>
        {
            if (trapQuantities["Mud"] > 0) ThrowTrap("Mud");
            else ThrowTrap("Spike");
        };
        return true;
    }

    private void OnDisable() => inputActions?.Player.Disable();

    public override void EquipWeapon(string newWeapon)
    {
        base.EquipWeapon(newWeapon);
        playerUI.UpdateWeaponIcon(newWeapon); // Mettre à jour l'icône de l'arme dans l'interface utilisateur
        nPlayer?.CmdSetWeapon(newWeapon);
    }

    public override void AddPoints(int points)
    {
        base.AddPoints(points);
        playerUI.UpdatePoints(BonusPoints); // Mettre à jour l'interface utilisateur avec les nouveaux points
        nPlayer?.CmdSetPoints(BonusPoints);
    }

    public override void ThrowTrap(string trapType)
    {
        base.ThrowTrap(trapType);
        playerUI.UpdateTrapCounts(trapQuantities["Mud"], trapQuantities["Spike"]); // Mettre à jour les compteurs de pièges dans l'interface utilisateur
    }

    public override void AddTrap(string trapType)
    {
        base.AddTrap(trapType);
        playerUI.UpdateTrapCounts(trapQuantities["Mud"], trapQuantities["Spike"]); // Mettre à jour les compteurs de pièges dans l'interface utilisateur
    }

    public override string GetName()
    {
        if (nPlayer == null)
            return GameManager.PlayerName;
        else
            return nPlayer.playerName;
    }
}
