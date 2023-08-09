using System.Net.Sockets;

namespace Server;

public class JoinService
{
    private readonly RoomRepository _roomRepository;
    private readonly HeaderService _headerService;

    public JoinService(RoomRepository roomRepository, HeaderService headerService)
    {
        _roomRepository = roomRepository;
        _headerService = headerService;
    }

    public void SerializeTo(JoinRoom.JoinRoomAns response, MemoryStream sendBuffer)
    {
        Header header = new Header(Command.Create, sizeof(int));
        var responseBytes = BitConverter.GetBytes(response.GetRoomNum());
        _headerService.SerializeTo(header, sendBuffer);
        sendBuffer.Write(responseBytes, 0 , sizeof(int));
    }

    public JoinRoom.JoinRoomReq DeserializeFrom(MemoryStream stream)
    {
        var roomNumBytes = new byte[4];
        stream.Read(roomNumBytes, 0, sizeof(int));
        var roomNum = BitConverter.ToInt32(roomNumBytes);
        JoinRoom.JoinRoomReq joinRoomReq = new JoinRoom.JoinRoomReq(roomNum);
        
        return joinRoomReq;
    }

    public List<Socket> JoinSocket(int roomNum, Socket socket)
    {
        var sockets = _roomRepository.AddToSocketList(socket, roomNum);
        
        return sockets;
    }
}