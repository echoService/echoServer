using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            var headerService = new HeaderService();
            var roomRepository = new RoomRepository();
            var joinService = new JoinService(roomRepository, headerService);
            var inquiryService = new InquiryService(roomRepository, headerService);
            var chatService = new ChatService(headerService);
            var createService = new CreateService(roomRepository, headerService);
            var chatHandler = new ChatMessageHandler(chatService);
            var createHandler = new CreateMessageHandler(createService);
            var joinHandler = new JoinMessageHandler(joinService);
            var inquiryHandler = new InquiryMessageHandler(inquiryService);
            var dispatcher = new Dispatcher(chatHandler, createHandler, joinHandler, inquiryHandler);
            var clientHandler = new ClientHandler(dispatcher, headerService);
            var endPoint = new IPEndPoint(IPAddress.Any, 5555);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var chatServer = new ChatServer(clientHandler);
            
            chatServer.StartChatServer(socket, endPoint);
        }
    }
}