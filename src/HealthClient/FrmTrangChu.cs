using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using HealthClient.Services;

namespace HealthClient
{
    public partial class FrmTrangChu : Form
    {
        private const string SERVER_IP = "127.0.0.1";
        private const int TCP_PORT = 5000;

        private readonly UserProfileHistoryService _userService = new();

        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private ListBox lstResults = null!;
        private RichTextBox rtbDetail = null!;
        private Button btnHoSo = null!;
        private Button btnLichSu = null!;

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

        private void InitSearchControls()
        {
            this.ClientSize = new Size(780, 520);

            if (lblGhiChu != null)
                lblGhiChu.Visible = false;

            if (lblXinChao != null)
            {
                lblXinChao.Location = new Point(28, 92);
                lblXinChao.Size = new Size(375, 44);
            }

            var btnFont = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnHoSo = new Button
            {
                Location = new Point(415, 98),
                Size = new Size(100, 34),
                Font = btnFont,
                Text = "Hồ sơ",
                UseVisualStyleBackColor = true,
                Cursor = Cursors.Hand
            };

            btnLichSu = new Button
            {
                Location = new Point(525, 98),
                Size = new Size(100, 34),
                Font = btnFont,
                Text = "Lịch sử",
                UseVisualStyleBackColor = true,
                Cursor = Cursors.Hand
            };

            if (btnDangXuat != null)
            {
                btnDangXuat.Location = new Point(635, 98);
                btnDangXuat.Size = new Size(115, 34);
                btnDangXuat.Font = btnFont;
            }

            txtSearch = new TextBox
            {
                Location = new Point(28, 155),
                Size = new Size(270, 30),
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "Nhập triệu chứng (VD: ho, sốt...)"
            };

            btnSearch = new Button
            {
                Location = new Point(308, 154),
                Size = new Size(95, 32),
                Font = btnFont,
                Text = "Tìm kiếm",
                UseVisualStyleBackColor = true,
                Cursor = Cursors.Hand
            };

            lstResults = new ListBox
            {
                Location = new Point(28, 198),
                Size = new Size(375, 210),
                Font = new Font("Segoe UI", 11F)
            };

            rtbDetail = new RichTextBox
            {
                Location = new Point(415, 198),
                Size = new Size(335, 210),
                Font = new Font("Segoe UI", 10.5F),
                ReadOnly = true
            };

            this.Controls.Add(btnHoSo);
            this.Controls.Add(btnLichSu);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnSearch);
            this.Controls.Add(lstResults);
            this.Controls.Add(rtbDetail);

            btnHoSo.BringToFront();
            btnLichSu.BringToFront();
            btnDangXuat?.BringToFront();
        }

        private void AttachEvents()
        {
            btnSearch.Click += async (s, e) => await PerformSearchAsync();
            lstResults.SelectedIndexChanged += async (s, e) => await LoadDiseaseDetailAsync();
            btnHoSo.Click += (s, e) => MoFormHoSo();
            btnLichSu.Click += async (s, e) => await MoFormLichSuAsync();
        }

        private void MoFormHoSo()
        {
            using var frm = new FrmHoSo();
            frm.ShowDialog(this);

            if (!string.IsNullOrWhiteSpace(frm.HoTenMoi) && lblXinChao != null)
            {
                lblXinChao.Text = $"Xin chào, {frm.HoTenMoi}";
            }
        }

        private async Task MoFormLichSuAsync()
        {
            using var frm = new FrmLichSu();
            if (frm.ShowDialog(this) == DialogResult.OK &&
                !string.IsNullOrWhiteSpace(frm.TuKhoaChonLai))
            {
                txtSearch.Text = frm.TuKhoaChonLai;
                await PerformSearchAsync();
            }
        }

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

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        await _userService.ThemLichSuAsync(keyword, null, _currentDiseases.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc dữ liệu Server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

                    string tuKhoaLuu = string.IsNullOrWhiteSpace(txtSearch.Text) ? name : txtSearch.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(tuKhoaLuu))
                    {
                        await _userService.ThemLichSuAsync(tuKhoaLuu, diseaseId, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy chi tiết bệnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> SendTcpRequestAsync(string jsonPayload)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(SERVER_IP, TCP_PORT);
                using NetworkStream stream = client.GetStream();
                var utf8NoBom = new UTF8Encoding(false);
                using StreamReader reader = new StreamReader(stream, utf8NoBom);
                using StreamWriter writer = new StreamWriter(stream, utf8NoBom) { AutoFlush = true };

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