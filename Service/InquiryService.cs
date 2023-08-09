using System.Text;

namespace Server;

public class InquiryService
{
    private readonly RoomRepository _roomRepository;
    private readonly HeaderService _headerService;

    public InquiryService(RoomRepository roomRepository, HeaderService headerService)
    {
        _roomRepository = roomRepository;
        _headerService = headerService;
    }

    public void SerializeTo(object request, MemoryStream stream)
    {
        var tempBuffer = new MemoryStream();
        var roomInfoList = _roomRepository.GetAllRoomInfos();
        
        tempBuffer.Write(BitConverter.GetBytes(roomInfoList.Count), 0, 4);
        Conversion(roomInfoList, tempBuffer);
        
        var header = new Header(Command.Inquiry, (int)tempBuffer.Length);
        _headerService.SerializeTo(header, stream);
        stream.Write(BitConverter.GetBytes(roomInfoList.Count), 0, 4);
        Conversion(roomInfoList, stream);
    }

    private void Conversion(List<RoomInfo> roomInfoList, MemoryStream stream)
    {
        foreach (var roomInfo in roomInfoList)
        {
            // 타이틀 바이트로 변환
            var titleBytes = Encoding.UTF8.GetBytes(roomInfo.GetTitle());
            var roomIdBytes = BitConverter.GetBytes(roomInfo.GetRoomNum());
            // 타이틀 길이를 int로 쓰기 (4바이트)
            stream.Write(BitConverter.GetBytes(titleBytes.Length), 0, sizeof(int));
            // 타이틀 바이트 쓰기
            stream.Write(titleBytes, 0, titleBytes.Length);
            stream.Write(roomIdBytes);
        }
    }
}