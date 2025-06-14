using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;
using TMPro;

public class RoomPlayerManager : NetworkBehaviour
{
    private readonly SyncDictionary<uint, string> playerNames = new();
    private readonly Dictionary<uint, GameObject> uiPlayers = new();

    private static RoomPlayerManager Instance;

    private NetworkRoomManager Singleton => (NetworkRoomManager)NetworkRoomManager.singleton;

    [SerializeField]
    private Transform playerListContainer;
    [SerializeField]
    private TextMeshProUGUI countText;

    private void Awake() => Instance = this;
    private void Start() => InvokeRepeating(nameof(UpdateUI), 1, 1);
    // private void OnDisable()
    // {
    //     if (!NetworkServer.active || Singleton == null)
    //         return;

    //     foreach (var roomPlayer in new List<NetworkRoomPlayer>(Singleton.roomSlots))
    //         if (roomPlayer != null && roomPlayer.gameObject != null)
    //             NetworkServer.Destroy(roomPlayer.gameObject);
    //     Singleton.roomSlots.Clear();
    // }

    void UpdateUI()
    {
        if (NetworkRoomManager.singleton == null) return;
        countText.text = $"{playerNames.Count}/4";

        // Supprime les UI des joueurs qui ne sont plus là
        foreach (var id in new List<uint>(uiPlayers.Keys))
        {
            if (!Singleton.roomSlots.Any(p => p.netId == id))
            {
                Destroy(uiPlayers[id]);
                uiPlayers.Remove(id);
                RemovePlayer(id);
            }
        }

        // Ajoute ou met à jour les UI des joueurs
        foreach (var player in Singleton.roomSlots)
        {
            if (!uiPlayers.ContainsKey(player.netId))
            {
                if (!playerNames.ContainsKey(player.netId))
                    continue;

                var obj = Instantiate(playerListContainer.GetChild(0).gameObject, playerListContainer);
                obj.SetActive(true);
                obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerNames[player.netId];
                uiPlayers[player.netId] = obj;

                if (player.isLocalPlayer)
                {
                    DisplayPlayerData("(not ready)", "Ready", Color.green, obj.transform, () =>
                    {
                        if (player.readyToBegin)
                            DisplayPlayerData("(not ready)", "Ready", Color.green, obj.transform, null);
                        else
                            DisplayPlayerData("(ready)", "Cancel", Color.yellow, obj.transform, null);
                        player.CmdChangeReadyState(!player.readyToBegin);
                    });
                }
                else if (NetworkServer.active)
                    DisplayPlayerData("", "Disconnect", Color.red, obj.transform, () => player.connectionToClient.Disconnect());
                else
                    DisplayPlayerData("", "", Color.clear, obj.transform, null);
            }
            else
            try
            {
                if (player.readyToBegin)
                    if (player.isLocalPlayer)
                        DisplayPlayerData("(ready)", "Cancel", Color.yellow, uiPlayers[player.netId].transform, null);
                    else
                        DisplayPlayerData("(ready)", null, null, uiPlayers[player.netId].transform, null);
                else if (player.isLocalPlayer)
                    DisplayPlayerData("(not ready)", "Ready", Color.green, uiPlayers[player.netId].transform, null);
                else
                    DisplayPlayerData("(not ready)", null, null, uiPlayers[player.netId].transform, null);
            }
            catch { }
        }
    }

    void DisplayPlayerData(string readyText, string buttonText, Color? buttonColor, Transform element, UnityAction action)
    {
        if (readyText != null)
            element.GetChild(1).GetComponent<TextMeshProUGUI>().text = readyText;

        if (buttonText != null)
            element.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = buttonText;

        if (buttonColor != null)
            element.GetChild(2).GetComponent<Image>().color = buttonColor.Value;

        if (action != null)
        {
            var b = element.GetChild(2).GetComponent<Button>().onClick;
            b.RemoveAllListeners();
            b.AddListener(action);
        }
    }

    public static void AddPlayer(uint netID, string name)
    {
        if (Instance.isServer)
            Instance.playerNames[netID] = name;
    }

    public static void RemovePlayer(uint netID)
    {
        if (Instance.isServer)
            if (Instance.playerNames.ContainsKey(netID))
                Instance.playerNames.Remove(netID);
    }

    public void ToMenu() => Tools.LoadScene("Menu");
}
