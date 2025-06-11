using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject CoinPrefab;
    [SerializeField]
    private float coinSpawnInterval = 2f; // Interval in seconds for spawning coins

    private void Start() => InvokeRepeating(nameof(InstantiateCoin), 0f, coinSpawnInterval);

    private void InstantiateCoin() => Instantiate(CoinPrefab, GenerateRandomPointInOval(), Quaternion.identity);
    
    Vector3 GenerateRandomPointInOval(float radiusX = 9, float radiusY = 8.8f)
    {
        float angle = Random.Range(0f, Mathf.PI * 2); // Angle aléatoire entre 0° et 360°
        float r = Mathf.Sqrt(Random.Range(0f, 1f));   // Distance aléatoire (distribution uniforme)

        // Calcul des coordonnées
        Vector2 center = new(3.7f, 0); // Centre de l'ovale
        // Utilisation des rayons pour obtenir les coordonnées x et z
        float x = center.x + r * radiusX * Mathf.Cos(angle);
        float z = center.y + r * radiusY * Mathf.Sin(angle);

        return new Vector3(x, 0, z);
    }
}
