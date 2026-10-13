using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace heathl_Infor_Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            try
            {
                using TcpClient client = new TcpClient();

                // Server TCP chạy trên cùng máy, cổng 5000.
                await client.ConnectAsync("127.0.0.1", 5000);

                using NetworkStream stream = client.GetStream();
                using StreamReader reader =
                    new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer =
                    new StreamWriter(stream, Encoding.UTF8);

                writer.AutoFlush = true;

                // Gửi một request đến Server.
                string request = "TEST tu WinForms lan 1";
                await writer.WriteLineAsync(request);

                // Đọc phản hồi từ Server.
                string? response = await reader.ReadLineAsync();

                if (response == null)
                {
                    throw new IOException(
                        "Server đã đóng kết nối mà chưa trả phản hồi."
                    );
                }

                MessageBox.Show(
                    response,
                    "Phản hồi từ Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi gửi request",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                button1.Enabled = true;
            }
        }
    }
}