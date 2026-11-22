using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthClient
{
    public class HeartbeatClient
    {
        private CancellationTokenSource _cts;
        private readonly int _intervalMs;

        public event Action OnConnectionLost;
        public event Action OnReconnected;

        public HeartbeatClient(int intervalMs = 5000)
        {
            _intervalMs = intervalMs;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => HeartbeatLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private async Task HeartbeatLoop(CancellationToken token)
        {
            bool isConnected = true;

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_intervalMs, token);

                bool currentPingSuccess = await SendPingAsync();

                if (!currentPingSuccess && isConnected)
                {
                    isConnected = false;
                    OnConnectionLost?.Invoke();
                }
                else if (currentPingSuccess && !isConnected)
                {
                    isConnected = true;
                    OnReconnected?.Invoke();
                }
            }
        }

        private async Task<bool> SendPingAsync()
        {
            await Task.Delay(100);
            return true;
        }
    }
}