using UnityEngine;

public class Coin : SpawnableBonus
{
    private void Update() => transform.Rotate(Vector3.up, 90 * Time.deltaTime); // Rotate the bonus item
    protected override void ApplyBonus(Player p) => p.AddPoints(10); // Add points to the player
}