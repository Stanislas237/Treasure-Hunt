using UnityEngine;

public class Coin : SpawnableBonus
{
    private void Update() => transform.Rotate(Vector3.up, 90 * Time.deltaTime, Space.World); // Rotate the bonus item
    protected override void ApplyBonus(Entity e) => e.AddPoints(10); // Add points to the entity
}