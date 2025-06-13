using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;

    public static Type GameType = typeof(GameManager);

    private List<GameObject> PrefabsToModify;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load all prefabs
        PrefabsToModify = new()
        {
            Resources.Load<GameObject>("Props/Traps/Mud/Mud"),
            Resources.Load<GameObject>("Props/Traps/SpikeTrap/SpikeTrap"),
            Resources.Load<GameObject>("Props/Arrow/Arrow"),
            Resources.Load<GameObject>("Props/Token/Token"),
            Resources.Load<GameObject>("Props/Box/Box")
        };

        // Assign Prefabs
        Entity.MudPrefab = PrefabsToModify[0];
        Entity.SpikePrefab = PrefabsToModify[1];
        Entity.ArrowPrefab = PrefabsToModify[2];
        GameManager.CoinPrefab = PrefabsToModify[3];
        GameManager.TreasurePrefab = PrefabsToModify[4];
    }

    public void LaunchGame()
    {
        if (GameType == typeof(GameManager))
            SetSoloMode();
        else
            SetMultiplayerMode();
    }

    private void SetMultiplayerMode()
    {
        foreach (var prefab in PrefabsToModify)
        {
            if (prefab.GetComponent<NetworkIdentity>() == null)
                prefab.AddComponent<NetworkIdentity>();

            if (prefab.GetComponent<NetworkTransformUnreliable>() == null)
            {
                var nt = prefab.AddComponent<NetworkTransformUnreliable>();
                nt.syncPosition = true;
                nt.syncRotation = true;
                nt.syncScale = false; // Éviter de synchroniser l’échelle si ce n'est pas nécessaire

                nt.interpolatePosition = true;
                nt.interpolateRotation = true;
            }

            if (!NetworkingManager.networkManager.spawnPrefabs.Contains(prefab))
                NetworkingManager.networkManager.spawnPrefabs.Add(prefab);
        }
        Debug.Log("Multiplayer mode set, prefabs updated.");
    }

    private void SetSoloMode()
    {
        foreach (var prefab in PrefabsToModify)
        {
            if (prefab.TryGetComponent(out NetworkIdentity ni))
                Destroy(ni);

            if (prefab.TryGetComponent(out NetworkTransformUnreliable nt))
                Destroy(nt);
        }
        Debug.Log("Solo mode set, prefabs updated.");
    }
}