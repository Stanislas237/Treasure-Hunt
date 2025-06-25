using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;

    public static Type GameType = typeof(GameManager);

    public static bool IsHost;

    public static string Address;

    private List<GameObject> PrefabsToModify;

    public Dictionary<string, AudioClip> Sounds;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load all sounds
        Sounds = new()
        {
            { "Click", Resources.Load<AudioClip>("Sounds/Click") },
            { "End", Resources.Load<AudioClip>("Sounds/EndGame") },
            { "Error", Resources.Load<AudioClip>("Sounds/Error") },
            { "Text", Resources.Load<AudioClip>("Sounds/TextPaste") },
            { "LastText", Resources.Load<AudioClip>("Sounds/LastTextPaste") },
            { "Bow", Resources.Load<AudioClip>("Sounds/Bow") },
            { "Sword", Resources.Load<AudioClip>("Sounds/Sword") },
            { "Hit", Resources.Load<AudioClip>("Sounds/Hit") },
            { "Coin", Resources.Load<AudioClip>("Sounds/Coin") },
            { "Treasure", Resources.Load<AudioClip>("Sounds/Treasure") },
            { "Mud", Resources.Load<AudioClip>("Sounds/Mud") },
            { "Spike", Resources.Load<AudioClip>("Sounds/Spike") }
        };

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

    public static void PlayClip2D(string clip, float volume = 1f)
    {
        GameObject tempGO = new GameObject("TempAudio2D");
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.volume = volume;
        aSource.spatialBlend = 0f;
        aSource.PlayOneShot(Instance.Sounds[clip]);
        Destroy(tempGO, Instance.Sounds[clip].length);
    }

    public void AddSoundOnButtons()
    {
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            b.onClick.AddListener(() => PlayClip2D("Click"));
            b.navigation = new(){ mode = Navigation.Mode.None };
        }
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
            if (prefab.TryGetComponent(out NetworkIdentity ni))
            {
                ni.enabled = true;
                NetworkClient.RegisterPrefab(prefab);
            }

            if (prefab.TryGetComponent(out NetworkTransformUnreliable nt))
                nt.enabled = true;

            if (!NetworkingManager.Instance.networkManager.spawnPrefabs.Contains(prefab))
                NetworkingManager.Instance.networkManager.spawnPrefabs.Add(prefab);
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