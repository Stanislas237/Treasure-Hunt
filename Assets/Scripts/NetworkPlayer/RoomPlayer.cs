using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar, HideInInspector]
    public string PlayerName = GameManager.PlayerName;

    public override void OnStartLocalPlayer() => CmdRegisterPlayer(netId);

    [Command]
    void CmdRegisterPlayer(uint netID) => RoomPlayerManager.AddPlayer(netID, PlayerName);

    public override void OnStopLocalPlayer() => CmdRemovePlayer(netId);

    [Command]
    void CmdRemovePlayer(uint netID) => RoomPlayerManager.RemovePlayer(netID);

    [Command]
    public void CmdRemoveSelf()
    {
        NetworkRoomManager manager = (NetworkRoomManager)NetworkRoomManager.singleton;
        if (manager != null)
        {
            manager.roomSlots.Remove(this); // Supprime ce joueur du lobby
            NetworkServer.Destroy(gameObject); // Supprime l'objet réseau
        }
    }
}
