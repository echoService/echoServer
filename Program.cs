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
                // 메모리를 직접 컨트롤하기 위한 MemoryStream 객체 생성
                MemoryStream memoryStream = new MemoryStream();
                // memoryStream의 offset을 컨트롤하기 위해서 read / writer용 포지션 변수 선언
                int readPosition = 0;
                int writePosition = 0;

                // 클라이언트 소켓과 연결되어 있는 동안 무한 루프
                while (client.Connected)
                {
                    // 전달 받은 데이터를 담을 buffer 메모리 할당
                    byte[] buffer = new byte[1024];
                    // 클라이언트로부터 전달받은 데이터의 길이를 저장하는 receivedBytes 변수 초기화
                    int receivedBytes = await client.ReceiveAsync(buffer, SocketFlags.None);
                    Console.WriteLine(receivedBytes);
                    if (receivedBytes == 0)
                        break;
                    // 데이터를 옮기기 위해서 Offset 변경 및 buffer 메모리에 데이터 저장
                    memoryStream.Position = writePosition;
                    memoryStream.Write(buffer, 0, receivedBytes);
                    // write용 포지션 데이터 변경
                    writePosition += receivedBytes;

                    // 무한 반복
                    while (true)
                    {
                        // 데이터를 다시 읽기 위해 Offset 변경
                        memoryStream.Position = readPosition;
                        if (memoryStream.Length < 4)
                            break;

                        // 정수(4바이트)를 읽기 위해 lengthBytes 변수에 메모리 할당
                        byte[] lengthBytes = new byte[4];
                        // 실제 데이터가 몇 글자인지 확인 후 length 변수에 정수값으로 저장
                        memoryStream.Read(lengthBytes, 0, 4);
                        int length = BitConverter.ToInt32(lengthBytes, 0);
                        Console.WriteLine("========================================= {0}", length);

                        // 데이터가 모두 도착했는지 확인
                        if (memoryStream.Length < (length + 4))
                        {
                            break;
                        }

                        // 실제 데이터를 읽어오기 위해 Offset 값 변경
                        readPosition += 4;
                        memoryStream.Position = readPosition;
                        // 실제 길이 만큼 데이터 할당
                        byte[] dataBytes = new byte[length];
                        memoryStream.Read(dataBytes, 0, length);

                        // 바이트 데이터를 문자열로 역직렬화 해서 변수에 저장
                        string message = Encoding.UTF8.GetString(dataBytes);
                        Console.WriteLine("Received: " + message);

                        // 처리한 데이터를 연결된 모든 Socket에게 전달
                        // 클라이언트 측에서도 동일한 작업을 수행할 수 있게 길이와 실제 데이터값을 나눠숴 전달해줌
                        foreach (var socket in clients)
                        {
                            socket.Send(lengthBytes);
                            socket.Send(dataBytes);
                        }

                        // 뒤에 남아있는 데이터가 있을 수 있음으로 확인하는 절차 필요 -> memoryStream의 전체 길이에서 현재까지 읽어온 데이터의 길이를 비교
                        long remainingLength = memoryStream.Length - (length + 4);
                        // 남아있는 데이터가 있다면 if문 실행
                        if (remainingLength > 0)
                        {
                            // 남아있는 데이터 만큼 메모리 할당
                            byte[] remainingData = new byte[remainingLength];
                            // Offset 변경을 위해 readPosition 변수값 변경
                            readPosition += length;
                            memoryStream.Position = readPosition;
                            memoryStream.Read(remainingData, 0, (int)remainingLength);

                            // 남은 데이터를 메모리 내에서 이동
                            byte[] innerBuffer = memoryStream.GetBuffer();
                            Buffer.BlockCopy(remainingData, 0, innerBuffer, 0, (int)remainingLength);

                            // MemoryStream의 길이를 조정하여 남은 데이터를 삭제
                            memoryStream.SetLength(remainingLength);
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
