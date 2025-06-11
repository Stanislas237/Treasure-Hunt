using UnityEngine;

public abstract class SpawnableBonus : MonoBehaviour
{
    private void Start()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5))
            transform.position = hit.point + Vector3.up; // Adjust position to be above the ground
        else
            Destroy(gameObject); // Destroy if no ground is found
    }

    private void Update() => transform.Rotate(Vector3.up, 90 * Time.deltaTime); // Rotate the bonus item

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player p))
        {
            ApplyBonus(p);
            Destroy(gameObject); // Destroy the bonus item after applying it
        }
    }

    protected abstract void ApplyBonus(Player p);
}
