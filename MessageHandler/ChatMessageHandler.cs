using System.Net.Sockets;

namespace Server;

public class ChatMessageHandler : IMessageHandler
{
    private readonly ChatService _chatService;

    public ChatMessageHandler(ChatService chatService)
    {
        _chatService = chatService;
    }
    
    public void HandleResponse(object request, Socket socket)
    {
        Chat chat = (Chat) request;
        MemoryStream sendBuffer = new MemoryStream();
        _chatService.SerializeTo(chat, sendBuffer);
        socket.Send(sendBuffer.ToArray());
    }

    public object HandleRequest(MemoryStream stream)
    {
        var chat = _chatService.DeserializeFrom(stream);
        return chat;
    }
}