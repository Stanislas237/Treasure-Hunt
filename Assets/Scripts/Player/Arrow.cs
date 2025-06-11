using UnityEngine;

public class Arrow : MonoBehaviour
{
    /// <summary>
    /// Speed of the arrow when launched.
    /// </summary>
    [SerializeField]
    private float speed = 10f;

    /// <summary>
    /// Direction in which the arrow is launched.
    /// </summary>
    private Vector3 target;

    /// <summary>
    /// The GameObject that launched the arrow.
    /// </summary>
    private GameObject Launcher;

    /// <summary>
    /// Initializes the arrow with a specified direction.
    /// </summary>
    /// <param name="dir">Direction in which the arrow should be launched.</param>
    public void Initialize(Vector3 pos, GameObject launcher)
    {
        target = pos;
        Launcher = launcher;
        Debug.Log($"Arrow initialized with target position: {target}");
        Destroy(gameObject, 5f); // Destroy the arrow after 5 seconds
    }

    private void Update() => transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime); // Move the arrow

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Arrow collided with: {other.gameObject.name}");
        if (other.gameObject == Launcher)
            return; // Ignore collision with the launcher
        if (other.gameObject.TryGetComponent(out Player player))
        {
            player.TakeDamage(10); // Deal damage to the player
            Destroy(gameObject); // Destroy the arrow after hitting the player
        }
        else if (other.gameObject.CompareTag("Terrain"))
            Destroy(gameObject); // Destroy the arrow when it hits the ground
    }
}