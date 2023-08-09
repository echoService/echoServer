using System.Net.Sockets;

namespace Server;

public class CreateMessageHandler : IMessageHandler
{
    private readonly CreateService _createService;

    public CreateMessageHandler(CreateService createService)
    {
        _createService = createService;
    }
    
    public void HandleResponse(object request, Socket client)
    {
        var createRoomReq = (CreateRoom.CreateRoomReq)request;
        var roomInfo = _createService.CreateRoomInfo(createRoomReq, client);
        var createRoomAns = new CreateRoom.CreateRoomAns(roomInfo.GetRoomNum());
        var sendBuff = new MemoryStream();
        _createService.SerializeTo(createRoomAns, sendBuff);
        client.Send(sendBuff.ToArray());
    }

    public object HandleRequest(MemoryStream stream)
    {
        var createRoomReq = _createService.DeserializeFrom(stream);
        return createRoomReq;
    }
}