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
    
    class Program
    {
        static List<StreamWriter> clientWriters = new List<StreamWriter>();

        static void Main(string[] args)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 5555);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(endPoint);
                socket.Listen();
                Console.WriteLine("Listener 동작 시작");

                while (true)
                {
                    Socket client = socket.Accept();
                    Console.WriteLine("클라이언트의 연결 대기 및 수락");

                    NetworkStream NS = new NetworkStream(client);
                    StreamWriter SW = new StreamWriter(NS, Encoding.UTF8);

                    clientWriters.Add(SW);

                    // 클라이언트를 메인 스레드와 다른 별도의 스레드로 처리
                    Task.Run(() => HandleClient(client, SW));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void HandleClient(Socket client, StreamWriter writer)
        {
            StreamReader SR = new StreamReader(new NetworkStream(client), Encoding.UTF8);
            string nickname = "예비닉네임";

            while (nickname == "예비닉네임")
            {
                nickname = SR.ReadLine();
            }
            
            foreach (var clientWriter in clientWriters)
            {
                clientWriter.WriteLine("{0}님이 채팅방에 입장하셨습니다.", nickname);
                clientWriter.Flush();
            }

            try
            {
                while (client.Connected)
                {
                    string GetMessage = SR.ReadLine();
                    if (GetMessage == null)
                        break;

                    Console.WriteLine("클라이언트로부터 메시지 읽기");

                    string formattedMessage = string.Format("{0}: {1}", nickname, GetMessage);

                    // 모든 클라이언트에게 브로드캐스트
                    foreach (var clientWriter in clientWriters)
                    {
                        clientWriter.WriteLine(formattedMessage);
                        clientWriter.Flush();
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
                if (writer != null)
                {
                    clientWriters.Remove(writer);
                    writer.Close();
                }
                SR.Close();
                client.Close();
                Console.WriteLine("연결 해제");
            }
        }
    }
}
