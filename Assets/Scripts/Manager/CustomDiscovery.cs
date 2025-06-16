using kcp2k;
using Mirror;
using Mirror.Discovery;
using System.Net;
using UnityEngine;

public class CustomDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    public System.Action<string> onServerDiscovered;
    private const ushort kcpPort = 9999;
    private const ushort discoveryPort = 49999;

    public override void Start()
    {
        FindFirstObjectByType<RoomPlayerManager>(FindObjectsInactive.Include)?.gameObject.SetActive(true);

        var kcpTransport = FindFirstObjectByType<KcpTransport>();
        if (kcpTransport != null && kcpTransport.Port != kcpPort)
            kcpTransport.Port = kcpPort;

        if (serverBroadcastListenPort != discoveryPort)
            serverBroadcastListenPort = discoveryPort;
        Debug.Log($"[Final] Port Transport = {kcpTransport.Port}, Port Discovery = {serverBroadcastListenPort}");
    }

    // Quand un client envoie une requête, que doit répondre le serveur ?
    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest _, IPEndPoint endpoint)
    {
        // Debug.LogError("Demande adressée au serveur à l'adresse " + endpoint.Address.ToString());
        return new DiscoveryResponse();
    }

    // Quand un client reçoit une réponse, que doit-il faire avec ?
    protected override void ProcessResponse(DiscoveryResponse _, IPEndPoint endPoint)
    {
        // Debug.LogError("Serveur détecté à l'adresse " + endPoint.Address.ToString());
        onServerDiscovered.Invoke(endPoint.Address.ToString());
    }
}

[System.Serializable]
public class DiscoveryResponse : NetworkMessage { }


[System.Serializable]
public class DiscoveryRequest : NetworkMessage { }
