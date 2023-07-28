using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sock_console_server
{
    class Program
    {
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

                    // 서버로 데이터가 들어왔을 때 모든 참가자들에게 메세지를 전달하기 위해 리스트에 담음
                    clients.Add(client);

                    // 클라이언트를 메인 스레드와 다른 별도의 스레드로 처리
                    Console.WriteLine("메세지 핸들링 시작");
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
            try
            {
                // 데이터를 받아서 저장할 MemoryStream 객체 생성
                MemoryStream memoryStream = new MemoryStream();
                int readPosition = 0;
                int writePosition = 0;

                while (client.Connected)
                {
                    // 클라이언트로부터 데이터를 수신하여 MemoryStream에 추가
                    byte[] buffer = new byte[1024];
                    int receivedBytes = await client.ReceiveAsync(buffer, SocketFlags.None);
                    Console.WriteLine(receivedBytes);
                    if (receivedBytes == 0)
                        break;
                    memoryStream.Position = writePosition;
                    memoryStream.Write(buffer, 0, receivedBytes);
                    writePosition += receivedBytes;

                    // 데이터를 처리하는 로직
                    while (true)
                    {
                        // 데이터 길이를 읽어옴
                        memoryStream.Position = 0;
                        if (memoryStream.Length < 4)
                            break;

                        byte[] lengthBytes = new byte[4];
                        memoryStream.Read(lengthBytes, 0, 4);
                        int length = BitConverter.ToInt32(lengthBytes, 0);
                        Console.WriteLine("========================================= {0}", length);

                        // 데이터가 모두 도착했는지 확인
                        if (memoryStream.Length < (length + 4))
                        {
                            break;
                        }

                        // 실제 데이터를 읽어옴
                        readPosition += 4;
                        memoryStream.Position = readPosition;
                        byte[] dataBytes = new byte[length];
                        memoryStream.Read(dataBytes, 0, length);

                        // 바이트 데이터를 문자열로 디코딩하여 콘솔에 표시
                        string message = Encoding.UTF8.GetString(dataBytes);
                        Console.WriteLine("Received: " + message);

                        // 처리한 데이터를 다시 클라이언트에 전송
                        foreach (var socket in clients)
                        {
                            socket.Send(dataBytes);
                        }

                        // 처리한 데이터는 MemoryStream에서 제거
                        long remainingLength = memoryStream.Length - (length + 4);
                        if (remainingLength > 0)
                        {
                            byte[] remainingData = new byte[remainingLength];
                            readPosition += length;
                            memoryStream.Position = readPosition;
                            memoryStream.Read(remainingData, 0, (int)remainingLength);

                            // 남은 데이터를 메모리 내에서 이동
                            byte[] innerBuffer = memoryStream.GetBuffer();
                            Buffer.BlockCopy(remainingData, 0, innerBuffer, 0, (int)remainingLength);

                            // MemoryStream의 길이를 조정하여 남은 데이터를 삭제
                            memoryStream.SetLength(remainingLength);
                            
                            readPosition = 0;
                            writePosition = 0;
                        }
                        else
                        {
                            memoryStream.SetLength(0);
                            
                            readPosition = 0;
                            writePosition = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
