using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    public override void OnStartLocalPlayer() => CmdRegisterPlayer(GameManager.PlayerName);

    [Command]
    void CmdRegisterPlayer(string name) => RoomPlayerManager.AddPlayer(netId, name);

    public override void OnStopLocalPlayer()
    {
        CmdRemovePlayer(netId);
        base.OnStopLocalPlayer();
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer)
            Tools.LoadScene(name, "Menu");
    }

    [Command]
    void CmdRemovePlayer(uint netID) => RoomPlayerManager.RemovePlayer(netID);
}
