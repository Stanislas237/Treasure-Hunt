using UnityEngine;

public class Coin : SpawnableBonus
{
    private void Update() => transform.Rotate(Vector3.up, 90 * Time.deltaTime, Space.World); // Rotate the bonus item
    protected override void ApplyBonus(Entity e)
    {
        if (e is Player p && e.enabled)
            GameMaster.PlayClip2D("Coin");

        e.AddPoints(10); // Add points to the entity
    }
}