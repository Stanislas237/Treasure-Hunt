using UnityEngine;
using Mirror;

public class NetworkingManager : GameManager
{
    public static NetworkManager networkManager;

    protected override void Awake()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        base.Awake();
    }

    private void OnDisable() => Destroy(networkManager.gameObject);
}