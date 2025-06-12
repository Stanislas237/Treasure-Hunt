using UnityEngine;

public class Treasure : SpawnableBonus
{
    protected override void ApplyBonus(Player p)
    {
        p.AddPoints(40); // Add points to the player

        var r = Random.Range(0f, 1f);
        if (r < 0.6f)
            return;
        else if (r < 0.7f)
            p.EquipWeapon("Sword"); // Equip a sword
        else if (r < 0.8f)
            p.EquipWeapon("Bow"); // Equip a bow
        else if (r < 0.9f)
            p.AddTrap("SpikeTrap"); // Add a spike trap
        else
            p.AddTrap("Mud"); // Add a mud trap
    }
}