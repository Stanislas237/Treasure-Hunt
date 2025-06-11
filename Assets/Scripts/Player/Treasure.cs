using UnityEngine;

public class Treasure : SpawnableBonus
{
    protected override void ApplyBonus(Player p)
    {
        p.AddPoints(40); // Add points to the player
        p.EquipWeapon(Random.Range(0, 2) == 0 ? "Sword" : "Bow"); // Equip a random weapon
    }
}