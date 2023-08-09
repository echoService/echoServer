using System.Net.Sockets;

namespace Server;

public class RoomRepository
{
    private static readonly Dictionary<int, RoomInfo> RoomInfoList = new();
    private static readonly object CounterLock = new();
    private static int _roomNumberCounter;

    public RoomRepository()
    {
        
    }
    
    public int GetRoomNumber()
    {
        var num = 0;
        lock (CounterLock)
        {
            _roomNumberCounter += 1;
            num = _roomNumberCounter;
        }
        return num;
    }

    public List<RoomInfo> GetAllRoomInfos()
    {
        var list = new List<RoomInfo>();
        foreach (var key in RoomInfoList.Keys)
        {
            RoomInfo roomInfo = RoomInfoList[key];
            list.Add(roomInfo);
        }

        return list;
    }

    public int GetRoomCount()
    {
        int count = RoomInfoList.Count;
        return count;
    }

    public void AddRoomInfoList(RoomInfo roomInfo)
    {
        RoomInfoList.Add(roomInfo.GetRoomNum(), roomInfo);
    }

    private RoomInfo FindRoomInfoBy(int roomNum)
    {
        RoomInfo roomInfo = RoomInfoList[roomNum];
        return roomInfo;
    }

    public List<Socket> CreateSocketList(Socket socket)
    {
        List<Socket> sockets = new List<Socket>();
        sockets.Add(socket);
        return sockets;
    }

    public List<Socket> AddToSocketList(Socket socket, int roomNum)
    {
        RoomInfo roomInfo = FindRoomInfoBy(roomNum);
        roomInfo.GetSocketList().Add(socket);
        return roomInfo.GetSocketList();
    }
}