using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar, HideInInspector]
    public string PlayerName = GameManager.PlayerName;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdRegisterPlayer(netId);
    }

    [Command]
    void CmdRegisterPlayer(uint netID) => RoomPlayerManager.AddPlayer(netID, PlayerName);

    public override void OnStopLocalPlayer()
    {
        CmdRemovePlayer(netId);
        base.OnStopLocalPlayer();
    }

    [Command]
    void CmdRemovePlayer(uint netID) => RoomPlayerManager.RemovePlayer(netID);
}
