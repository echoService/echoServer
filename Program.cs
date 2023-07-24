using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Sock_console_server
{
    class Program
    {
        static void Main(string[] args)

        {
            TcpListener Listener = null;
            NetworkStream NS = null;

            StreamReader SR = null;
            StreamWriter SW = null;
            TcpClient client = null;

            int PORT = 5555;

            try
            {   
                Listener = new TcpListener(PORT);
                Listener.Start();
                Console.WriteLine("Listener 동작 시작");

                while (true)
                {
                    client = Listener.AcceptTcpClient();
                    Console.WriteLine("클라이언트의 연결 대기 및 수락");
                    NS = client.GetStream();
                    Console.WriteLine("클라이언트와 통신하는 네트워크 스트림 생성");
                    SR = new StreamReader(NS, Encoding.UTF8);
                    Console.WriteLine("네트워크 스트림으로부터 메시지를 가져오는 스트림 생성");
                    SW = new StreamWriter(NS, Encoding.UTF8);
                    Console.WriteLine("네트워크 스트림으로 메시지를 보내는 스트림 생성");

                    while (client.Connected == true)
                    {
                        string GetMessage = SR.ReadLine();   
                        Console.WriteLine("클라이언트로부터 메시지 읽기");                
                        SW.WriteLine(GetMessage);
                        Console.WriteLine("클라이언트에게 메시지 보내기");        
                        SW.Flush();
                        Console.WriteLine("버퍼의 내용을 클라이언트로 보냄");  
                        Console.WriteLine(GetMessage);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            finally
            {
                SW.Close();
                SR.Close();
                client.Close();
                NS.Close();
                Console.WriteLine("연결 해제");
            }
        }
    }
}
