using UnityEngine;
using Mirror;

public class NetworkingManager : GameManager
{
    public static NetworkManager networkManager;

    protected override bool Awake()
    {
        networkManager = FindFirstObjectByType<NetworkManager>(FindObjectsInactive.Include);

        if (!base.Awake())
            return false;

        foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
            Destroy(entity.gameObject);
        return true;
    }

    
    protected override void InstantiateCoin()
    {
        var prefabName = Random.Range(0, 10) > 7 ? "Treasure" : "Coin";
        if (NetworkServer.active)
            Players[0].nPlayer.CmdSpawnObject(prefabName, GenerateRandomPointInOval(), Quaternion.Euler(0, 0, 90));
    }
}