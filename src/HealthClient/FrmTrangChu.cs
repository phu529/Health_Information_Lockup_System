using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HealthClient
{
    public partial class FrmTrangChu : Form
    {
        private const string SERVER_IP = "127.0.0.1";
        private const int TCP_PORT = 5000;

        // Các ô giao diện
        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private ListBox lstResults = null!;
        private RichTextBox rtbDetail = null!;

        // Lưu danh sách kết quả tra cứu tạm thời
        private List<DiseaseItem> _currentDiseases = new();

        public FrmTrangChu()
        {
            InitializeComponent();
            InitSearchControls();
            AttachEvents();
        }

        public FrmTrangChu(string hoTen) : this()
        {
            if (lblXinChao != null)
                lblXinChao.Text = $"Xin chào, {hoTen}";
        }

        /// <summary>
        /// Tạo và cân chỉnh vị trí các ô giao diện tra cứu
        /// </summary>
        private void InitSearchControls()
        {
            this.Width = 800;
            this.Height = 520;

            // Ẩn dòng chữ tạm của bước cũ
            foreach (Control c in this.Controls)
            {
                if (c is Label lbl && lbl.Text.Contains("Bước 1.4"))
                {
                    lbl.Visible = false;
                }
            }

            // Đưa nút Đăng xuất lên góc trên bên phải
            if (btnDangXuat != null)
            {
                btnDangXuat.Location = new Point(640, 105);
                btnDangXuat.Size = new Size(100, 32);
            }

            // 1. Ô nhập triệu chứng
            txtSearch = new TextBox
            {
                Location = new Point(30, 160),
                Size = new Size(260, 27),
                PlaceholderText = "Nhập triệu chứng (VD: Ho, Sốt, Đau bụng...)"
            };

            // 2. Nút Tìm kiếm
            btnSearch = new Button
            {
                Location = new Point(300, 158),
                Size = new Size(90, 30),
                Text = "Tìm kiếm",
                UseVisualStyleBackColor = true
            };

            // 3. Danh sách bệnh kết quả
            lstResults = new ListBox
            {
                Location = new Point(30, 200),
                Size = new Size(360, 200)
            };

            // 4. Khung xem thông tin chi tiết
            rtbDetail = new RichTextBox
            {
                Location = new Point(400, 200),
                Size = new Size(340, 200),
                ReadOnly = true
            };

            this.Controls.Add(txtSearch);
            this.Controls.Add(btnSearch);
            this.Controls.Add(lstResults);
            this.Controls.Add(rtbDetail);
        }

        private void AttachEvents()
        {
            btnSearch.Click += async (s, e) => await PerformSearchAsync();
            lstResults.SelectedIndexChanged += async (s, e) => await LoadDiseaseDetailAsync();
        }

        /// <summary>
        /// Gửi lệnh SEARCH lên TCP Server
        /// </summary>
        private async Task PerformSearchAsync()
        {
            string keyword = txtSearch.Text.Trim();
            var request = new
            {
                Command = "SEARCH",
                Payload = new { Keyword = keyword }
            };

            string jsonResponse = await SendTcpRequestAsync(JsonSerializer.Serialize(request));
            if (string.IsNullOrEmpty(jsonResponse)) return;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                if (root.GetProperty("Status").GetString() == "SUCCESS")
                {
                    lstResults.Items.Clear();
                    _currentDiseases.Clear();

                    foreach (var elem in root.GetProperty("Data").EnumerateArray())
                    {
                        var item = new DiseaseItem
                        {
                            Id = elem.GetProperty("Id").GetInt32(),
                            Name = elem.GetProperty("Name").GetString() ?? ""
                        };
                        _currentDiseases.Add(item);
                        lstResults.Items.Add(item.Name);
                    }

                    if (_currentDiseases.Count == 0)
                    {
                        rtbDetail.Text = "Không tìm thấy kết quả phù hợp.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc dữ liệu Server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gửi lệnh GET_DETAIL khi click vào 1 bệnh trong ListBox
        /// </summary>
        private async Task LoadDiseaseDetailAsync()
        {
            int index = lstResults.SelectedIndex;
            if (index < 0 || index >= _currentDiseases.Count) return;

            int diseaseId = _currentDiseases[index].Id;
            var request = new
            {
                Command = "GET_DETAIL",
                Payload = new { DiseaseId = diseaseId }
            };

            string jsonResponse = await SendTcpRequestAsync(JsonSerializer.Serialize(request));
            if (string.IsNullOrEmpty(jsonResponse)) return;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                if (root.GetProperty("Status").GetString() == "SUCCESS")
                {
                    var data = root.GetProperty("Data");
                    string name = data.GetProperty("Name").GetString() ?? "";
                    string desc = data.GetProperty("Description").GetString() ?? "";
                    string treat = data.GetProperty("Treatment").GetString() ?? "";

                    rtbDetail.Text = $"=== {name.ToUpper()} ===\n\n📌 Mô tả bệnh:\n{desc}\n\n💊 Hướng điều trị:\n{treat}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy chi tiết bệnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Kết nối TCP gửi/nhận dữ liệu
        /// </summary>
        private async Task<string> SendTcpRequestAsync(string jsonPayload)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(SERVER_IP, TCP_PORT);
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                await writer.WriteLineAsync(jsonPayload);
                return await reader.ReadLineAsync() ?? string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            Close();
        }

        private class DiseaseItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}