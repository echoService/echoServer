using System.Net.Sockets;

namespace Server;

public class ClientHandler
{
    private readonly Dispatcher _dispatcher;
    private readonly HeaderService _headerService;

    public ClientHandler(Dispatcher dispatcher, HeaderService headerService)
    {
        _dispatcher = dispatcher;
        _headerService = headerService;
    }

    public async Task HandleClient(Socket socket)
    {
        try
        {
            while (socket.Connected)
            {
                var memoryStream = new MemoryStream();
                var buffer = new byte[1024];

                var receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);
                if (receivedBytes == 0)
                    break;

                memoryStream.Position = 0;
                memoryStream.Write(buffer, 0, receivedBytes);

                while (true)
                {
                    memoryStream.Position = 0;
                    if (memoryStream.Length < 8)
                        break;

                    var header = _headerService.DeserializeFrom(memoryStream);
                    if (memoryStream.Length < 8 + header.GetLength())
                        break;

                    var request = _dispatcher.DispatchRequest(header.GetCommand(), memoryStream);
                    _dispatcher.DispatchResponse(request, header.GetCommand(), socket);
                    memoryStream.SetLength(0);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Console.WriteLine("연결 해제");
        }
    }
}