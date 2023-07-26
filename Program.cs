using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Sock_console_server
{
    class Program
    {
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

                    // 클라이언트를 메인 스레드와 다른 별도의 스레드로 처리
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void HandleClient(Socket client)
        {
            StreamReader SR = null;
            StreamWriter SW = null;
            Guid uuid = Guid.NewGuid();

            try
            {
                NetworkStream NS = new NetworkStream(client);
                Console.WriteLine("클라이언트와 통신하는 네트워크 스트림 생성");
                SR = new StreamReader(NS, Encoding.UTF8);
                Console.WriteLine("네트워크 스트림으로부터 메시지를 가져오는 스트림 생성");
                SW = new StreamWriter(NS, Encoding.UTF8);
                Console.WriteLine("네트워크 스트림으로 메시지를 보내는 스트림 생성");

                while (client.Connected)
                {
                    string GetMessage = SR.ReadLine();
                    if (GetMessage == null)
                        break;

                    Console.WriteLine("클라이언트로부터 메시지 읽기");
                    SW.WriteLine("{0}: {1}", uuid.ToString(), GetMessage);
                    Console.WriteLine("클라이언트에게 메시지 보내기");
                    SW.Flush();
                    Console.WriteLine("버퍼의 내용을 클라이언트로 보냄");
                    Console.WriteLine("{0}: {1}", uuid.ToString(), GetMessage);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            finally
            {
                if (SW != null)
                    SW.Close();
                if (SR != null)
                    SR.Close();
                client.Close();
                Console.WriteLine("연결 해제");
            }
        }
    }
}