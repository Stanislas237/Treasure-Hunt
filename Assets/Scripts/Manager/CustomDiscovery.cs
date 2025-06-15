using Mirror;
using Mirror.Discovery;
using System.Net;

public class CustomDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    // Quand un client envoie une requête → que doit répondre le serveur ?
    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint) =>
        new DiscoveryResponse
        {
            serverId = ServerId,
            uri = transport.ServerUri().ToString()
        };

    // Quand un client reçoit une réponse → que doit-il faire avec ?
    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint){}
}

public struct DiscoveryRequest : NetworkMessage { }

public struct DiscoveryResponse : NetworkMessage
{
    public long serverId;
    public string uri;
}
