using UnityEngine;
using Mirror;

public class NetworkingManager : GameManager
{
    public static NetworkManager networkManager;
    public static System.Collections.Generic.Dictionary<uint, string> playerNames;

    protected override bool Awake()
    {
        networkManager = FindFirstObjectByType<NetworkManager>(FindObjectsInactive.Include);

        if (!base.Awake())
            return false;

        foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
            Destroy(entity.gameObject);

        networkManager.gameObject.SetActive(true);
        return true;
    }

    private void OnDisable() => Destroy(networkManager.gameObject);
}