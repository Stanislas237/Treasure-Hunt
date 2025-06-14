using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;

    public static Type GameType = typeof(NetworkingManager);
    // public static Type GameType = typeof(GameManager);

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

    private void Init()
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
        Init();
        if (GameType == typeof(GameManager))
            SetSoloMode();
        else
            SetMultiplayerMode();
    }

    private void SetMultiplayerMode()
    {
        foreach (var prefab in PrefabsToModify)
        {
            if (prefab.TryGetComponent(out NetworkIdentity ni))
            {
                ni.enabled = true;
                NetworkClient.RegisterPrefab(prefab);
            }

            if (prefab.TryGetComponent(out NetworkTransformUnreliable nt))
                nt.enabled = true;

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
                ni.enabled = false;

            if (prefab.TryGetComponent(out NetworkTransformUnreliable nt))
                nt.enabled = false;
        }
        Debug.Log("Solo mode set, prefabs updated.");
    }
}