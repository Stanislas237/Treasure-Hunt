using UnityEngine;

public class Treasure : SpawnableBonus
{
    protected override void ApplyBonus(Entity e)
    {
        if (e is Player p && e.enabled)
            GameMaster.PlayClip2D("Treasure");
            
        e.AddPoints(40); // Add points to the entity

        var r = Random.Range(0f, 1f);
        if (r < 0.1f)
            return;
        else if (r < 0.45f)
            e.EquipWeapon("Sword"); // Equip a sword
        else if (r < 0.8f)
            e.EquipWeapon("Bow"); // Equip a bow
        else if (r < 0.9f)
            e.AddTrap("SpikeTrap"); // Add a spike trap
        else
            e.AddTrap("Mud"); // Add a mud trap
    }
}