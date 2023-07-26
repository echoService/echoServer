using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sock_console_server
{
    // 서버의 흐름: 소켓 생성 -> bind()로 소켓에 주소 할당 -> Socket이 연결 시도를 수신하는 상태가 되도록 Listen()으로 변경 -> Socket이 요청을 수락하도록 while문을 통해 accept()를 반복 -> Socket이 연결되면 StreamWriter 객체 생성 후 리스트에 넣음 -> 이후 메세지 주고 받기
    // struct / class 의 차이점.
    //   value type: 복사값으로 넘어감.
    //   class type: 레퍼런스로 넘어감.
    // Boxing / UnBoxing

    class Program
    {
        private static List<object> _list = new ();
        private static List<Socket> clients = new List<Socket>();

        static void Main(string[] args)
        {
            // 어떤 주소든 접근할 수 있도록 EndPoint 객체 생성
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5555);
            //  TCP 프로토콜을 사용하여 연결 지향 데이터 스트림을 구현하는 소켓을 생성
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // socket 객체에 앤드포인트 설정
                socket.Bind(endPoint);
                // socket에 클라이언트가 접근할 수 있도록 설정
                socket.Listen();
                Console.WriteLine("Listener 동작 시작");
                
                while (true)
                {
                    // 클라이언트의 요청을 승인하여 메세지 송수신 준비
                    Socket client = socket.Accept();
                    Console.WriteLine("클라이언트의 연결 대기 및 수락");

                    var buffer = new Memory<byte>(new byte[1024]);
                    ValueTask<int> receive = socket.ReceiveAsync(buffer);
                    
                    // 서버로 데이터가 들어왔을 때 모든 참가자들에게 메세지를 전달하기 위해 StreamWriter를 List에 담음
                    clients.Add(client);

                    // 클라이언트를 메인 스레드와 다른 별도의 스레드로 처리
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task HandleClient(Socket client)
        {
            // 버퍼 메모리 설정
            var buffer = new Memory<byte>(new byte[1024]);
            string nickname = "예비닉네임";

            // 스트림에 처음 들어온 문자열은 닉네임으로 지정
            while (nickname == "예비닉네임")
            {
                int receivedBytes = await client.ReceiveAsync(buffer);
                nickname = Encoding.UTF8.GetString(buffer.Span.Slice(0, receivedBytes));
            }
            
            // List에 담긴 StreamWriter들 각각 입장 알림 전송
            foreach (var socket in clients)
            {
                string welcomeMessage = string.Format("{0}님이 채팅방에 입장하셨습니다.", nickname);
                buffer = Encoding.UTF8.GetBytes(welcomeMessage);
                await socket.SendAsync(buffer);
            }

            try
            {
                while (client.Connected)
                {
                    int receivedBytes = await client.ReceiveAsync(buffer);
                    if (receivedBytes == 0)
                        break;

                    Console.WriteLine("클라이언트로부터 메시지 읽기");

                    string getMessage = Encoding.UTF8.GetString(buffer.Span.Slice(0, receivedBytes));
                    string formattedMessage = string.Format("{0}: {1} - {2}", nickname, getMessage, DateTime.Now.ToString("HH:mm:ss"));

                    foreach (var socket in clients)
                    {
                        // String을 바이트 코드로 변환
                        byte[] formattedBuffer = Encoding.UTF8.GetBytes(formattedMessage);
                        // 변환된 바이트 코드를 보내준다
                        await socket.SendAsync(formattedBuffer);
                    }

                    Console.WriteLine("클라이언트에게 메시지 보내기");
                    Console.WriteLine("버퍼의 내용을 클라이언트로 보냄");
                    Console.WriteLine(formattedMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (clients.Contains(client))
                {
                    clients.Remove(client);
                    client.Close();
                }
                Console.WriteLine("연결 해제");
            }
        }
    }
}
