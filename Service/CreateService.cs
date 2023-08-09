using System.Net.Sockets;
using System.Text;

namespace Server;

public class CreateService
{
    private readonly RoomRepository _roomRepository;
    private readonly HeaderService _headerService;

    public CreateService(RoomRepository roomRepository, HeaderService headerService)
    {
        _roomRepository = roomRepository;
        _headerService = headerService;
    }

    public void SerializeTo(CreateRoom.CreateRoomAns response, MemoryStream sendBuffer)
    {
        Header header = new Header(Command.Create, sizeof(int));
        var responseBytes = BitConverter.GetBytes(response.GetRoomNum());
        _headerService.SerializeTo(header, sendBuffer);
        sendBuffer.Write(responseBytes, 0 , sizeof(int));
    }

    public CreateRoom.CreateRoomReq DeserializeFrom(MemoryStream stream)
    {
        var lengthBytes = new byte[4];
        stream.Read(lengthBytes, 0, 4);
        var titleBytes = new byte[BitConverter.ToInt32(lengthBytes)];
        stream.Read(titleBytes, 0, titleBytes.Length);
        CreateRoom.CreateRoomReq createRoomReq = new CreateRoom.CreateRoomReq(Encoding.UTF8.GetString(titleBytes));
        return createRoomReq;
    }

    public RoomInfo CreateRoomInfo(CreateRoom.CreateRoomReq createRoomReq, Socket socket)
    {
        int roomNum = _roomRepository.GetRoomNumber();
        List<Socket> socketList = _roomRepository.CreateSocketList(socket);
        var roomInfo = new RoomInfo(createRoomReq.GetTitle(), roomNum, socketList);
        _roomRepository.AddRoomInfoList(roomInfo);

        return roomInfo;
    }
}