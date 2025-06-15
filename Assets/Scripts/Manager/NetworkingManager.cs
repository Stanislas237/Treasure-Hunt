using System.Collections;
using UnityEngine;
using Mirror;

public class NetworkingManager : GameManager
{
    public static NetworkManager networkManager;

    public static NetworkingManager Instance;

    protected override bool Awake()
    {
        networkManager = FindFirstObjectByType<NetworkManager>(FindObjectsInactive.Include);

        if (!base.Awake())
            return false;

        foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
            Destroy(entity.gameObject);
        Instance = this;
        return true;
    }


    protected override void InstantiateCoin()
    {
        if (NetworkServer.active)
            Players[0].nPlayer.CmdSpawnObject(Random.Range(0, 10) > 7 ? "Treasure" : "Coin",
            GenerateRandomPointInOval(), Quaternion.Euler(0, 0, 90));
    }
    
    public static void NetworkDestroy(GameObject target, float delay) => Instance.StartCoroutine(Instance.DestroyCoroutine(target, delay));

    IEnumerator DestroyCoroutine(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null && NetworkServer.active)
            NetworkServer.Destroy(target);
    }
}