using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HealthShared;

namespace HealthServer
{
    internal class Program
    {
        private const int UDP_PORT = 8888;
        private const string DISCOVERY_REQUEST = "DISCOVER_HEALTH_SERVER";
        private const string DISCOVERY_RESPONSE = "HEALTH_SERVER_RESPONSE";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== HEALTH SERVER IS STARTING ===");

            _ = Task.Run(() => StartUdpDiscoveryListener());

            Console.WriteLine("Server listening for UDP Broadcast on port " + UDP_PORT);
            Console.WriteLine("Press Enter to stop server...");
            Console.ReadLine();
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