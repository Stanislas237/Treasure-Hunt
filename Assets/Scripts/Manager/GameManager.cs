using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject CoinPrefab;
    [SerializeField]
    private GameObject TreasurePrefab;
    [SerializeField]
    private float SpawnInterval;

    public static string PlayerName { get; private set; } = "RandomPlayer";

    private void Start() => InvokeRepeating(nameof(InstantiateCoin), 3, SpawnInterval);

    private void InstantiateCoin()
    {
        var prefab = Random.Range(0f, 1f) > 0.8f ? TreasurePrefab : CoinPrefab;
        // Instantiate a coin or treasure at a random point in the oval area
        Instantiate(prefab, GenerateRandomPointInOval(), prefab.transform.rotation);
    }
    
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
