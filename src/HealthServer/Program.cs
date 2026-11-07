using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HealthServer
{
    internal class Program
    {
        private const int UDP_PORT = 8888;
        private const int TCP_PORT = 5000;
        private const string DISCOVERY_REQUEST = "DISCOVER_HEALTH_SERVER";
        private const string DISCOVERY_RESPONSE = "HEALTH_SERVER_RESPONSE";

        private static HealthRepository _repository = new HealthRepository();
        private static RequestRouter _router = new RequestRouter(_repository);

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== HEALTH SERVER IS STARTING ===");

            // 1. Lắng nghe UDP Discovery
            _ = Task.Run(() => StartUdpDiscoveryListener());

            // 2. Lắng nghe TCP Requests
            _ = Task.Run(() => StartTcpListenerAsync());

            Console.WriteLine($"Server listening for UDP Broadcast on port {UDP_PORT}");
            Console.WriteLine($"Server listening for TCP Requests on port {TCP_PORT}");
            Console.WriteLine("Press Enter to stop server...");
            Console.ReadLine();
        }

        private static async Task StartTcpListenerAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, TCP_PORT);
            listener.Start();

            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[TCP Server Error] " + ex.Message);
                }
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"[TCP] Client connected: {client.Client.RemoteEndPoint}");

            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            try
            {
                string? requestJson;
                while ((requestJson = await reader.ReadLineAsync()) != null)
                {
                    Console.WriteLine($"[TCP Request] {requestJson}");

                    // Điều phối gói tin qua RequestRouter
                    string responseJson = await _router.ProcessRequestAsync(requestJson);

                    // Trả kết quả JSON lại cho Client
                    await writer.WriteLineAsync(responseJson);
                    Console.WriteLine($"[TCP Response] {responseJson}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP Client Error] {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("[TCP] Client disconnected.");
            }
        }

        private static void StartUdpDiscoveryListener()
        {
            using (UdpClient udpServer = new UdpClient(UDP_PORT))
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    try
                    {
                        byte[] data = udpServer.Receive(ref remoteEP);
                        string message = Encoding.UTF8.GetString(data);

                        if (message == DISCOVERY_REQUEST)
                        {
                            Console.WriteLine($"[UDP] Received discovery from: {remoteEP.Address}");
                            byte[] responseBytes = Encoding.UTF8.GetBytes(DISCOVERY_RESPONSE);
                            udpServer.Send(responseBytes, responseBytes.Length, remoteEP);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[UDP Error] " + ex.Message);
                    }
                }
            }
        }
    }
}