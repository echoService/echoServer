using System.Net.Sockets;

namespace Server;

public class InquiryMessageHandler : IMessageHandler
{
    private readonly InquiryService _inquiryService;

    public InquiryMessageHandler(InquiryService inquiryService)
    {
        _inquiryService = inquiryService;
    }
    
    public void HandleResponse(object request, Socket socket)
    {
        var sendBuffer = new MemoryStream();
        
        _inquiryService.SerializeTo(request, sendBuffer);

        socket.SendAsync(sendBuffer.ToArray());
    }
    
    public object HandleRequest(MemoryStream stream)
    {
        var getRoomListReq = new Inquiry.GetRoomListReq();
        return getRoomListReq;
    }
}