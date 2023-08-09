using System.Net.Sockets;

namespace Server;

public interface IMessageHandler
{
    void HandleResponse(object request, Socket socket);
    
    object HandleRequest(MemoryStream stream);
}