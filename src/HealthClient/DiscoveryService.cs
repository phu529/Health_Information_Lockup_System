using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HealthClient
{
    public static class DiscoveryService
    {
        private const int UDP_PORT = 8888;
        private const string DISCOVERY_REQUEST = "DISCOVER_HEALTH_SERVER";
        private const string DISCOVERY_RESPONSE = "HEALTH_SERVER_RESPONSE";

        public static async Task<string> DiscoverServerIpAsync(int timeoutMs = 3000)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;
                byte[] requestBytes = Encoding.UTF8.GetBytes(DISCOVERY_REQUEST);

                IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Broadcast, UDP_PORT);
                await udpClient.SendAsync(requestBytes, requestBytes.Length, broadcastEP);

                IPEndPoint localEP = new IPEndPoint(IPAddress.Loopback, UDP_PORT);
                await udpClient.SendAsync(requestBytes, requestBytes.Length, localEP);

                var receiveTask = udpClient.ReceiveAsync();
                if (await Task.WhenAny(receiveTask, Task.Delay(timeoutMs)) == receiveTask)
                {
                    var result = await receiveTask;
                    string responseStr = Encoding.UTF8.GetString(result.Buffer);
                    if (responseStr == DISCOVERY_RESPONSE)
                    {
                        return result.RemoteEndPoint.Address.ToString();
                    }
                }
            }

            return null;
        }
    }
}