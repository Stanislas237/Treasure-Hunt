using UnityEngine;
using Mirror;

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
    private Entity Launcher;

    /// <summary>
    /// Initializes the arrow with a specified direction.
    /// </summary>
    public void Initialize(Vector3 pos, Entity launcher)
    {
        target = pos;
        Launcher = launcher;

        if (launcher.nPlayer == null)
            Destroy(gameObject, 5f); // Destroy the arrow after 5 seconds
        else
            Invoke(nameof(Destroy), 5f);
    }

    private void Destroy()
    {
        if (gameObject != null)
            Launcher.nPlayer.CmdDestroyObject(gameObject);
    }

    private void Update() => transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime); // Move the arrow

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Launcher.gameObject)
            return; // Ignore collision with the launcher
        if (other.gameObject.CompareTag("Terrain"))
        {
            if (Launcher.nPlayer == null)
                Destroy(gameObject); // Détruire la flèche lorsqu'elle touche le sol
            else
                Destroy();
        }
        else if (other.gameObject.TryGetComponent(out Entity e))
        {
            int damages = Mathf.Min(5, e.BonusPoints);
            e.TakeDamage(damages); // Infliger des dégâts au joueur
            Launcher.AddPoints(damages); // Ajouter les points du joueur à soi-même
            if (Launcher.nPlayer == null)
                Destroy(gameObject); // Détruire la flèche après avoir touché le joueur
            else
                Destroy();
        }
    }
}