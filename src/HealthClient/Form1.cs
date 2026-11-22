using System;
using System.Windows.Forms;

namespace HealthClient
{
    public partial class Form1 : Form
    {
        private HeartbeatClient _heartbeatClient;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            string serverIp = await DiscoveryService.DiscoverServerIpAsync();
            if (serverIp != null)
            {
                Text = $"HealthClient - Đã tìm thấy Server tại IP: {serverIp}";
            }
            else
            {
                Text = "HealthClient - Không tìm thấy Server trong mạng LAN";
            }

            _heartbeatClient = new HeartbeatClient();
            _heartbeatClient.OnConnectionLost += () =>
            {
                if (IsHandleCreated)
                {
                    Invoke(new Action(() =>
                    {
                        Text = "HealthClient - [MẤT KẾT NỐI - Đang thử Reconnect...]";
                    }));
                }
            };

            _heartbeatClient.OnReconnected += () =>
            {
                if (IsHandleCreated)
                {
                    Invoke(new Action(() =>
                    {
                        Text = "HealthClient - [ĐÃ KẾT NỐI LẠI]";
                    }));
                }
            };

            _heartbeatClient.Start();
        }
    }
}