using System.Net;
using System.Net.Sockets;

namespace Server;

public class ChatServer
{
    private readonly ClientHandler _clientHandler;

    public ChatServer(ClientHandler clientHandler)
    {
        _clientHandler = clientHandler;
    }

    public void StartChatServer(Socket socket, IPEndPoint endPoint)
    {
        try
        {
            socket.Bind(endPoint);
            socket.Listen();
            Console.WriteLine("Listener 동작 시작");

            while (true)
            {
                Socket client = socket.Accept();
                Console.WriteLine("클라이언트의 연결 대기 및 수락");

                Console.WriteLine("메세지 핸들링 시작");
                Task.Run(() => _clientHandler.HandleClient(client));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message); 
        }
    }
}