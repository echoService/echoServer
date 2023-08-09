using System.Net.Sockets;

namespace Server;

public class JoinMessageHandler : IMessageHandler
{
    private readonly JoinService _joinService;

    public JoinMessageHandler(JoinService joinService)
    {
        _joinService = joinService;
    }
    
    public void HandleResponse(object request, Socket socket)
    {
        JoinRoom.JoinRoomReq joinRoomReq = (JoinRoom.JoinRoomReq)request;
        JoinRoom.JoinRoomAns joinRoomAns = new JoinRoom.JoinRoomAns(joinRoomReq.GetRoomNum());
        MemoryStream sendBuffer = new MemoryStream();
        _joinService.SerializeTo(joinRoomAns, sendBuffer);
        var socketList = _joinService.JoinSocket(joinRoomAns.GetRoomNum(), socket);
        foreach (var client in socketList)
        {
            client.Send(sendBuffer.ToArray());
        }
    }

    public object HandleRequest(MemoryStream stream)
    {
        var joinRoomReq = _joinService.DeserializeFrom(stream);
        return joinRoomReq;
    }
}