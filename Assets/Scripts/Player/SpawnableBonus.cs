using UnityEngine;
using Mirror;

public abstract class SpawnableBonus : MonoBehaviour
{
    private void Start()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5))
            transform.position = hit.point + Vector3.up; // Adjust position to be above the ground
        else
            Destroy(gameObject); // Destroy if no ground is found
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Entity e))
        {
            if (!e.nPlayer || e.nPlayer.isLocalPlayer)
                ApplyBonus(e);
            if (NetworkServer.active)
                NetworkServer.Destroy(gameObject);
            else
                Destroy(gameObject); // Destroy the bonus item after applying it
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance.SpawnedTreasures.Contains(transform))
            GameManager.Instance.SpawnedTreasures.Remove(transform); // Remove from the list of spawned treasures when disabled
    }

    protected abstract void ApplyBonus(Entity e);
}
