using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class RoomPlayerManager : NetworkBehaviour
{
    private readonly SyncDictionary<uint, string> playerNames = new();

    private static RoomPlayerManager Instance;

    [SerializeField]
    private Transform PlayerList;

    private void Awake() => Instance = this;

    void OnDisable()
    {
        foreach (KeyValuePair<uint, string> pair in playerNames)
            NetworkingManager.playerNames.Add(pair.Key, pair.Value);
    }

    private static void UpdateUI()
    {
        for (int i = Instance.PlayerList.childCount - 1; i > 0; i--)
            Destroy(Instance.PlayerList.GetChild(i).gameObject);

        foreach (var k in Instance.playerNames.Keys)
        {
            var obj = Instantiate(Instance.PlayerList.GetChild(0).gameObject, Instance.PlayerList);
            obj.SetActive(true);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Instance.playerNames[k];

            var button = obj.transform.GetChild(1);
            var buttonImage = button.GetComponent<Image>();
            var buttonText = button.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (k == NetworkClient.localPlayer.netId)
            {
                buttonImage.color = Color.green;
                buttonText.text = "Ready";
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Instance.ChangeReadyState(k);
                    if (buttonText.text == "Ready")
                    {
                        buttonText.text = "Cancel";
                        buttonImage.color = Color.yellow;
                    }
                    else
                    {
                        buttonText.text = "Ready";
                        buttonImage.color = Color.green;
                    }
                });
            }
            else if (Instance.isServer)
            {
                buttonImage.color = Color.red;
                buttonText.text = "Remove";
                button.GetComponent<Button>().onClick.AddListener(() => Instance.DisconnectPlayer(k));
            }
            else
                Destroy(button.gameObject);
        }
    }

    public RoomPlayer FindRoomPlayer(uint netID)
    {
        foreach (var player in FindObjectsByType<RoomPlayer>(FindObjectsSortMode.None))
        {
            if (player.netId == netID)
                return player; // Retourne le bon joueur
        }
        return null; // Si aucun joueur avec ce netID n'est trouvé
    }
    
    void ChangeReadyState(uint netID)
    {
        var player = FindRoomPlayer(netID);
        if (player != null)
            player.CmdChangeReadyState(!player.readyToBegin);
    }

    void DisconnectPlayer(uint netID)
    {
        var player = FindRoomPlayer(netID);
        if (player != null)
            player.CmdRemoveSelf();
    }

    public static void AddPlayer(uint netID, string name)
    {
        if (Instance.isServer)
        {
            Instance.playerNames[netID] = name;
            UpdateUI();
        }
    }

    public static void RemovePlayer(uint netID)
    {
        if (Instance.isServer) 
        {
            if (Instance.playerNames.ContainsKey(netID))
            {
                Instance.playerNames.Remove(netID);
                UpdateUI();
            }
        }
    }

    public static string GetPlayer(uint netID) => Instance.playerNames[netID];
}
