using TMPro;
using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float SpawnInterval;
    [SerializeField]
    private TextMeshProUGUI timerText;
    [SerializeField]
    private Transform EndGamePanel;
    private float timeLeft = 180;

    public static GameObject CoinPrefab;
    public static GameObject TreasurePrefab;
    public static List<Transform> SpawnedTreasures { get; private set; } = new();
    public static List<Entity> Players { get; private set; } = new();

    public static string PlayerName { get; private set; } = string.Empty;

    public static NetworkManager networkManager;

    protected virtual bool Awake()
    {
        networkManager = FindFirstObjectByType<NetworkManager>(FindObjectsInactive.Include);

        if (GetType() != GameMaster.GameType)
        {
            Destroy(this);
            return false;
        }

        try { GameMaster.Instance.LaunchGame(); }
        catch { FindFirstObjectByType<GameMaster>().LaunchGame(); }

        if (networkManager)
            Destroy(networkManager.gameObject);

        return true;
    }

    private void Start() => InvokeRepeating(nameof(InstantiateCoin), 3, SpawnInterval);

    private void Update()
    {
        if (timeLeft >= 0)
        {            
            timeLeft -= Time.deltaTime;
            if (timeLeft >= 0)
                timerText.text = $"{(int)timeLeft / 60}.{(int)timeLeft % 60:D2}";
            else
                StartCoroutine(EndGame());
        }
    }

    protected virtual void InstantiateCoin()
    {
        var prefab = Random.Range(0, 10) > 7 ? TreasurePrefab : CoinPrefab;
        // Instantiate a coin or treasure at a random point in the oval area
        SpawnedTreasures.Add(Instantiate(prefab, GenerateRandomPointInOval(), prefab.transform.rotation).transform);
    }

    protected Vector3 GenerateRandomPointInOval(float radiusX = 9, float radiusY = 8.8f)
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

    IEnumerator AnimateTextSizeCoroutine(TextMeshProUGUI textElement, string text, float targetSize, float duration)
    {
        float startSize = 300;
        float time = 0f;
        textElement.text = text;

        while (time < duration)
        {
            textElement.fontSize = Mathf.Lerp(startSize, targetSize, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        textElement.fontSize = targetSize;
    }

    private IEnumerator EndGame()
    {
        CancelInvoke();
        EndGamePanel.parent.GetChild(0).gameObject.SetActive(false);

        var orderedList = Players.OrderByDescending(e => e.BonusPoints).ToList();
        bool hasWon = orderedList[0].TryGetComponent(out Player p) && p.enabled;

        foreach (var item in Players)
            item.enabled = false;

        EndGamePanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(EndGamePanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);

        StartCoroutine(AnimateTextSizeCoroutine(EndGamePanel.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>(), "The End", 100, 0.3f));
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(AnimateTextSizeCoroutine(EndGamePanel.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>(), "Ranking", 50, 0.3f));
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < orderedList.Count; i++)
        {
            if (i > 3) break;

            StartCoroutine(AnimateTextSizeCoroutine(EndGamePanel.GetChild(1).GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>(), orderedList[i].GetName(), 36, 0.3f));
            StartCoroutine(AnimateTextSizeCoroutine(EndGamePanel.GetChild(1).GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>(), orderedList[i].BonusPoints.ToString(), 60, 0.3f));
            yield return new WaitForSeconds(0.5f);
        }

        var LastTextElement = EndGamePanel.GetChild(2).GetComponent<TextMeshProUGUI>();
        StartCoroutine(AnimateTextSizeCoroutine(LastTextElement, hasWon ? "You Won" : "You Lost", 150, 0.3f));
        LastTextElement.color = hasWon ? Color.green : Color.red;
    }

    public static void SetName(string newName) => PlayerName = newName;
}
