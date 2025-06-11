using UnityEngine;

public class Coin : SpawnableBonus
{
    protected override void ApplyBonus(Player p) => p.AddPoints(10); // Add points to the player
}