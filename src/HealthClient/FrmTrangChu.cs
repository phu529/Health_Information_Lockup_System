using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private Button btnFilter = null!;
        private ContextMenuStrip menuFilter = null!;
        private FlowLayoutPanel flpActiveChips = null!;
        private Label lblResultCount = null!;
        private ListBox lstResults = null!;
        private RichTextBox rtbDetail = null!;
        private Button btnKhaoSatNhanh = null!;
        private Button btnNhanBacSi = null!;
        private Button btnHoSo = null!;
        private Button btnLichSu = null!;

        private readonly List<string> _activeFilters = new();
        private List<DiseaseItem> _currentDiseases = new();
        private string? _phieuKhaoSatTomTat;

        private static readonly (string Nhom, string Nhan, string TuKhoa)[] DanhMucBoLoc =
        {
            ("Nhóm cơ quan", "Hô hấp", "hô hấp"),
            ("Nhóm cơ quan", "Tiêu hóa", "dạ dày"),
            ("Nhóm cơ quan", "Truyền nhiễm", "sốt xuất huyết"),
            ("Triệu chứng", "Ho", "ho"),
            ("Triệu chứng", "Sốt", "sốt"),
            ("Triệu chứng", "Đau họng", "đau họng"),
            ("Triệu chứng", "Đau đầu", "đau đầu"),
            ("Triệu chứng", "Đau bụng", "đau bụng"),
            ("Triệu chứng", "Buồn nôn / Ói", "buồn nôn"),
            ("Triệu chứng", "Phát ban", "phát ban")
        };

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
            ClientSize = new Size(880, 590);
            BackColor = Color.FromArgb(248, 250, 252);

            if (pnlTieuDe != null)
            {
                pnlTieuDe.BackColor = Color.FromArgb(30, 78, 140);
                pnlTieuDe.Height = 60;
            }
            if (lblTieuDe != null)
            {
                lblTieuDe.Text = "Hệ thống tra cứu triệu chứng và thông tin sức khỏe";
                lblTieuDe.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
                lblTieuDe.Size = new Size(820, 36);
            }

            if (lblGhiChu != null)
                lblGhiChu.Visible = false;

            if (lblXinChao != null)
            {
                lblXinChao.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
                lblXinChao.Location = new Point(24, 74);
                lblXinChao.Size = new Size(300, 36);
            }

            var btnFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            // Nut Khao sat suc khoe nhanh: TUYET DOI KHONG tu dong bat popup khi vua vao Trang chu
            btnKhaoSatNhanh = new Button
            {
                Location = new Point(332, 75),
                Size = new Size(148, 34),
                Font = btnFont,
                Text = "📋 Khảo sát nhanh",
                BackColor = Color.FromArgb(230, 244, 241),
                ForeColor = Color.FromArgb(21, 101, 95),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnKhaoSatNhanh.FlatAppearance.BorderColor = Color.FromArgb(153, 209, 202);

            btnNhanBacSi = new Button
            {
                Location = new Point(488, 75),
                Size = new Size(134, 34),
                Font = btnFont,
                Text = "💬 Nhắn bác sĩ",
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 78, 140),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNhanBacSi.FlatAppearance.BorderColor = Color.FromArgb(196, 212, 232);

            btnHoSo = new Button
            {
                Location = new Point(630, 75),
                Size = new Size(74, 34),
                Font = btnFont,
                Text = "Hồ sơ",
                UseVisualStyleBackColor = true,
                Cursor = Cursors.Hand
            };

            btnLichSu = new Button
            {
                Location = new Point(710, 75),
                Size = new Size(74, 34),
                Font = btnFont,
                Text = "Lịch sử",
                UseVisualStyleBackColor = true,
                Cursor = Cursors.Hand
            };

            if (btnDangXuat != null)
            {
                btnDangXuat.Location = new Point(790, 75);
                btnDangXuat.Size = new Size(68, 34);
                btnDangXuat.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                btnDangXuat.Text = "Thoát";
            }

            // Hang tim kiem + nut Bo loc (hien so bo loc dang ap dung)
            txtSearch = new TextBox
            {
                Location = new Point(24, 124),
                Size = new Size(340, 32),
                Font = new Font("Segoe UI", 10.5F),
                PlaceholderText = "Nhập triệu chứng, tên bệnh hoặc thuốc..."
            };

            btnSearch = new Button
            {
                Location = new Point(372, 123),
                Size = new Size(102, 32),
                Font = btnFont,
                Text = "Tìm kiếm",
                BackColor = Color.FromArgb(30, 78, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;

            btnFilter = new Button
            {
                Location = new Point(482, 123),
                Size = new Size(138, 32),
                Font = btnFont,
                Text = "≡ Bộ lọc  ▾",
                BackColor = Color.White,
                ForeColor = Color.FromArgb(28, 43, 64),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            menuFilter = BuildFilterMenu();

            // Dai chip dieu kien dang ap dung ("Loc xong roi, dung giau bo loc")
            flpActiveChips = new FlowLayoutPanel
            {
                Location = new Point(24, 161),
                Size = new Size(832, 34),
                BackColor = Color.Transparent,
                WrapContents = false,
                AutoScroll = true
            };

            lblResultCount = new Label
            {
                Location = new Point(24, 198),
                Size = new Size(410, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text = "Gợi ý tham khảo (chọn bộ lọc hoặc nhập triệu chứng để tra cứu)"
            };

            lstResults = new ListBox
            {
                Location = new Point(24, 224),
                Size = new Size(396, 252),
                Font = new Font("Segoe UI", 10.5F)
            };

            rtbDetail = new RichTextBox
            {
                Location = new Point(436, 224),
                Size = new Size(420, 252),
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BackColor = Color.White
            };

            Controls.Add(btnKhaoSatNhanh);
            Controls.Add(btnNhanBacSi);
            Controls.Add(btnHoSo);
            Controls.Add(btnLichSu);
            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
            Controls.Add(btnFilter);
            Controls.Add(flpActiveChips);
            Controls.Add(lblResultCount);
            Controls.Add(lstResults);
            Controls.Add(rtbDetail);

            RenderActiveFilterChips();

            btnKhaoSatNhanh.BringToFront();
            btnNhanBacSi.BringToFront();
            btnHoSo.BringToFront();
            btnLichSu.BringToFront();
            btnDangXuat?.BringToFront();
        }

        private ContextMenuStrip BuildFilterMenu()
        {
            var cms = new ContextMenuStrip
            {
                Font = new Font("Segoe UI", 9.5F),
                ShowCheckMargin = true
            };

            string lastGroup = string.Empty;
            foreach (var item in DanhMucBoLoc)
            {
                if (item.Nhom != lastGroup)
                {
                    if (lastGroup.Length > 0)
                        cms.Items.Add(new ToolStripSeparator());
                    var header = new ToolStripMenuItem($"— {item.Nhom} —")
                    {
                        Enabled = false,
                        Font = new Font("Segoe UI", 8.8F, FontStyle.Bold)
                    };
                    cms.Items.Add(header);
                    lastGroup = item.Nhom;
                }

                var mi = new ToolStripMenuItem(item.Nhan)
                {
                    Tag = item.Nhan,
                    CheckOnClick = false
                };
                mi.Click += async (_, _) => await ToggleFilterConditionAsync(item.Nhan);
                cms.Items.Add(mi);
            }

            cms.Items.Add(new ToolStripSeparator());
            var clearItem = new ToolStripMenuItem("Xóa toàn bộ bộ lọc");
            clearItem.Click += async (_, _) =>
            {
                if (_activeFilters.Count == 0) return;
                _activeFilters.Clear();
                SyncMenuChecks();
                RenderActiveFilterChips();
                await PerformSearchAsync();
            };
            cms.Items.Add(clearItem);
            return cms;
        }

        private void SyncMenuChecks()
        {
            foreach (ToolStripItem tsi in menuFilter.Items)
            {
                if (tsi is ToolStripMenuItem mi && mi.Tag is string label)
                {
                    mi.Checked = _activeFilters.Contains(label);
                }
            }
            btnFilter.Text = _activeFilters.Count > 0
                ? $"≡ Bộ lọc ({_activeFilters.Count})  ▾"
                : "≡ Bộ lọc  ▾";
        }

        private void RenderActiveFilterChips()
        {
            flpActiveChips.SuspendLayout();
            flpActiveChips.Controls.Clear();

            if (_activeFilters.Count == 0)
            {
                var lblEmpty = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    Margin = new Padding(0, 6, 0, 0),
                    Text = "Chưa áp dụng bộ lọc — khi chọn bộ lọc, từng điều kiện sẽ hiện ngay tại đây để bỏ riêng dễ dàng."
                };
                flpActiveChips.Controls.Add(lblEmpty);
            }
            else
            {
                foreach (string cond in _activeFilters.ToList())
                {
                    var chip = new Button
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(224, 242, 241),
                        ForeColor = Color.FromArgb(15, 90, 84),
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        Text = $"{cond}  ✕",
                        Padding = new Padding(6, 1, 6, 1),
                        Margin = new Padding(0, 2, 8, 2),
                        Cursor = Cursors.Hand
                    };
                    chip.FlatAppearance.BorderColor = Color.FromArgb(153, 214, 208);
                    string captured = cond;
                    chip.Click += async (_, _) => await RemoveFilterConditionAsync(captured);
                    flpActiveChips.Controls.Add(chip);
                }

                if (_activeFilters.Count >= 2)
                {
                    var btnClearAll = new Button
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.White,
                        ForeColor = Color.FromArgb(185, 28, 28),
                        Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                        Text = "Xóa tất cả",
                        Padding = new Padding(5, 1, 5, 1),
                        Margin = new Padding(2, 2, 0, 2),
                        Cursor = Cursors.Hand
                    };
                    btnClearAll.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);
                    btnClearAll.Click += async (_, _) =>
                    {
                        _activeFilters.Clear();
                        SyncMenuChecks();
                        RenderActiveFilterChips();
                        await PerformSearchAsync();
                    };
                    flpActiveChips.Controls.Add(btnClearAll);
                }
            }

            flpActiveChips.ResumeLayout();
        }

        private async Task ToggleFilterConditionAsync(string label)
        {
            if (_activeFilters.Contains(label))
                _activeFilters.Remove(label);
            else
                _activeFilters.Add(label);

            SyncMenuChecks();
            RenderActiveFilterChips();
            await PerformSearchAsync();
        }

        private async Task RemoveFilterConditionAsync(string label)
        {
            if (!_activeFilters.Remove(label)) return;
            SyncMenuChecks();
            RenderActiveFilterChips();
            await PerformSearchAsync();
        }

        private void AttachEvents()
        {
            btnSearch.Click += async (_, _) => await PerformSearchAsync();
            txtSearch.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await PerformSearchAsync();
                }
            };
            btnFilter.Click += (_, _) => menuFilter.Show(btnFilter, new Point(0, btnFilter.Height + 2));
            lstResults.SelectedIndexChanged += async (_, _) => await LoadDiseaseDetailAsync();
            btnKhaoSatNhanh.Click += async (_, _) => await MoHopKhaoSatNhanhAsync();
            btnNhanBacSi.Click += (_, _) => MoHopNhanBacSi();
            btnHoSo.Click += (_, _) => MoFormHoSo();
            btnLichSu.Click += async (_, _) => await MoFormLichSuAsync();
        }

        private async Task MoHopKhaoSatNhanhAsync()
        {
            using var dlg = new Form
            {
                Text = "Bảng khảo sát sức khỏe nhanh (Gợi ý tham khảo & Gửi trước cho Bác sĩ)",
                Size = new Size(540, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            var lblIntro = new Label
            {
                Location = new Point(20, 14),
                Size = new Size(485, 40),
                ForeColor = Color.FromArgb(51, 65, 85),
                Text = "Trả lời nhanh tình trạng hiện tại để nhận gợi ý bệnh tham khảo. Dữ liệu khảo sát sẽ được đính kèm sẵn nếu bạn chọn nhắn tin với bác sĩ."
            };

            var lblQ1 = new Label
            {
                Location = new Point(20, 60),
                Size = new Size(480, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "1. Các triệu chứng bạn đang gặp (có thể chọn nhiều):"
            };

            var clbSymptoms = new CheckedListBox
            {
                Location = new Point(20, 84),
                Size = new Size(484, 108),
                CheckOnClick = true,
                MultiColumn = true,
                ColumnWidth = 155
            };
            clbSymptoms.Items.AddRange(new object[]
            {
                "Ho", "Sốt", "Đau họng", "Đau đầu", "Mệt mỏi",
                "Đau bụng", "Buồn nôn", "Ợ chua", "Phát ban"
            });

            var lblQ2 = new Label
            {
                Location = new Point(20, 202),
                Size = new Size(230, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "2. Thời gian khởi phát / kéo dài:"
            };
            var cboDuration = new ComboBox
            {
                Location = new Point(20, 226),
                Size = new Size(225, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDuration.Items.AddRange(new object[] { "Dưới 24 giờ", "Từ 1 – 3 ngày", "Trên 3 ngày" });
            cboDuration.SelectedIndex = 1;

            var lblQ3 = new Label
            {
                Location = new Point(265, 202),
                Size = new Size(235, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "3. Mức độ khó chịu hiện tại:"
            };
            var cboSeverity = new ComboBox
            {
                Location = new Point(265, 226),
                Size = new Size(239, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboSeverity.Items.AddRange(new object[] { "Nhẹ (sinh hoạt bình thường)", "Vừa (mệt, ảnh hưởng nhẹ)", "Nhiều (cần tư vấn sớm)" });
            cboSeverity.SelectedIndex = 1;

            var lblQ4 = new Label
            {
                Location = new Point(20, 266),
                Size = new Size(480, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "4. Ghi chú thêm cho bác sĩ (tiền sử dị ứng, bệnh nền, thuốc đã dùng):"
            };
            var txtNote = new TextBox
            {
                Location = new Point(20, 290),
                Size = new Size(484, 52),
                Multiline = true,
                PlaceholderText = "Ví dụ: Sốt nhẹ về chiều, chưa dùng thuốc, không có tiền sử dị ứng..."
            };

            var btnApply = new Button
            {
                Location = new Point(196, 358),
                Size = new Size(210, 36),
                BackColor = Color.FromArgb(21, 101, 95),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Xem gợi ý & Lưu cho Bác sĩ",
                Cursor = Cursors.Hand
            };
            btnApply.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Location = new Point(414, 358),
                Size = new Size(90, 36),
                Text = "Đóng",
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };

            List<string> chosen = new();
            btnApply.Click += (_, _) =>
            {
                chosen = clbSymptoms.CheckedItems.Cast<string>().ToList();
                if (chosen.Count == 0)
                {
                    MessageBox.Show(dlg, "Vui lòng tích chọn ít nhất 1 triệu chứng đang gặp để nhận gợi ý tham khảo.",
                        "Thiếu thông tin khảo sát", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string ghiChu = string.IsNullOrWhiteSpace(txtNote.Text) ? "Chưa ghi nhận thêm" : txtNote.Text.Trim();
                _phieuKhaoSatTomTat =
                    $"• Triệu chứng khảo sát: {string.Join(", ", chosen)}\n" +
                    $"• Thời gian kéo dài: {cboDuration.SelectedItem}\n" +
                    $"• Mức độ khó chịu: {cboSeverity.SelectedItem}\n" +
                    $"• Ghi chú tiền sử / thuốc: {ghiChu}\n" +
                    $"• Thời điểm khảo sát: {DateTime.Now:dd/MM/yyyy HH:mm}";

                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };

            dlg.Controls.AddRange(new Control[]
            {
                lblIntro, lblQ1, clbSymptoms, lblQ2, cboDuration, lblQ3, cboSeverity, lblQ4, txtNote, btnApply, btnCancel
            });

            if (dlg.ShowDialog(this) == DialogResult.OK && chosen.Count > 0)
            {
                btnKhaoSatNhanh.Text = "📋 Khảo sát (Đã lưu)";
                btnNhanBacSi.Text = "💬 Nhắn BS (Kèm KS)";

                _activeFilters.Clear();
                foreach (string s in chosen)
                {
                    var match = DanhMucBoLoc.FirstOrDefault(x =>
                        string.Equals(x.Nhan, s, StringComparison.OrdinalIgnoreCase) ||
                        x.Nhan.StartsWith(s, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(match.Nhan) && !_activeFilters.Contains(match.Nhan))
                        _activeFilters.Add(match.Nhan);
                }
                txtSearch.Text = chosen[0];
                SyncMenuChecks();
                RenderActiveFilterChips();
                await PerformSearchAsync();

                rtbDetail.Text =
                    "=== ĐÃ LƯU PHIẾU KHẢO SÁT SỨC KHỎE NHANH ===\n\n" +
                    _phieuKhaoSatTomTat +
                    "\n\n👉 Danh sách bên trái đã được lọc theo kết quả khảo sát (gợi ý tham khảo).\n" +
                    "👉 Khi bạn bấm nút [💬 Nhắn BS (Kèm KS)], bản tóm tắt khảo sát này sẽ tự động gửi trước cho bác sĩ nắm tình hình.";
            }
        }

        private void MoHopNhanBacSi()
        {
            using var dlg = new Form
            {
                Text = "Nhắn tin tư vấn Bác sĩ (Đính kèm Khảo sát nhanh)",
                Size = new Size(520, 390),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            var chkAttach = new CheckBox
            {
                Location = new Point(20, 16),
                Size = new Size(465, 26),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 101, 95),
                Checked = !string.IsNullOrWhiteSpace(_phieuKhaoSatTomTat),
                Enabled = !string.IsNullOrWhiteSpace(_phieuKhaoSatTomTat),
                Text = !string.IsNullOrWhiteSpace(_phieuKhaoSatTomTat)
                    ? "Gửi kèm Phiếu khảo sát sức khỏe nhanh để bác sĩ nắm tình hình trước"
                    : "Chưa có Phiếu khảo sát nhanh (bạn có thể làm khảo sát ở Trang chủ trước khi gửi)"
            };

            var rtbSurveyPreview = new RichTextBox
            {
                Location = new Point(20, 46),
                Size = new Size(464, 118),
                ReadOnly = true,
                BackColor = Color.FromArgb(241, 245, 249),
                Text = _phieuKhaoSatTomTat ?? "Chưa có dữ liệu khảo sát sức khỏe nhanh."
            };

            var lblMsg = new Label
            {
                Location = new Point(20, 174),
                Size = new Size(460, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Nội dung câu hỏi gửi bác sĩ:"
            };

            var txtMsg = new TextBox
            {
                Location = new Point(20, 198),
                Size = new Size(464, 86),
                Multiline = true,
                PlaceholderText = "Nhập câu hỏi cần bác sĩ tư vấn tham khảo..."
            };

            var btnSend = new Button
            {
                Location = new Point(276, 298),
                Size = new Size(116, 36),
                BackColor = Color.FromArgb(30, 78, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Gửi tin nhắn",
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;

            var btnClose = new Button
            {
                Location = new Point(400, 298),
                Size = new Size(84, 36),
                Text = "Đóng",
                DialogResult = DialogResult.Cancel
            };

            btnSend.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtMsg.Text) && !chkAttach.Checked)
                {
                    MessageBox.Show(dlg, "Vui lòng nhập nội dung câu hỏi hoặc đính kèm phiếu khảo sát nhanh.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string kemKs = chkAttach.Checked ? " (đã đính kèm Phiếu khảo sát sức khỏe nhanh cho bác sĩ xem trước)" : string.Empty;
                MessageBox.Show(dlg,
                    $"Đã ghi nhận yêu cầu tư vấn{kemKs}.\nBác sĩ chuyên khoa sẽ tiếp nhận thông tin khi trực tuyến.",
                    "Đã chuẩn bị gửi Bác sĩ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.Close();
            };

            dlg.Controls.AddRange(new Control[] { chkAttach, rtbSurveyPreview, lblMsg, txtMsg, btnSend, btnClose });
            dlg.ShowDialog(this);
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
            var filterKeywords = _activeFilters
                .Select(lbl => DanhMucBoLoc.FirstOrDefault(x => x.Nhan == lbl).TuKhoa ?? lbl.ToLowerInvariant())
                .ToList();

            string queryToSend = !string.IsNullOrWhiteSpace(keyword)
                ? keyword
                : (filterKeywords.Count > 0 ? filterKeywords[0] : string.Empty);

            var request = new
            {
                Command = "SEARCH",
                Payload = new { Keyword = queryToSend }
            };

            string jsonResponse = await SendTcpRequestAsync(JsonSerializer.Serialize(request));
            if (string.IsNullOrEmpty(jsonResponse)) return;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                if (root.GetProperty("Status").GetString() == "SUCCESS")
                {
                    var fetched = new List<DiseaseItem>();
                    foreach (var elem in root.GetProperty("Data").EnumerateArray())
                    {
                        fetched.Add(new DiseaseItem
                        {
                            Id = elem.GetProperty("Id").GetInt32(),
                            Name = elem.GetProperty("Name").GetString() ?? ""
                        });
                    }

                    // Neu co nhieu dieu kien bo loc dang ap dung, giao nhom ket qua theo tung dieu kien loc tren Server
                    if (filterKeywords.Count > 0)
                    {
                        foreach (string fk in filterKeywords)
                        {
                            if (string.Equals(fk, queryToSend, StringComparison.OrdinalIgnoreCase))
                                continue;

                            string subResp = await SendTcpRequestAsync(JsonSerializer.Serialize(new
                            {
                                Command = "SEARCH",
                                Payload = new { Keyword = fk }
                            }));
                            if (string.IsNullOrEmpty(subResp)) continue;

                            using JsonDocument subDoc = JsonDocument.Parse(subResp);
                            if (subDoc.RootElement.GetProperty("Status").GetString() == "SUCCESS")
                            {
                                var subIds = new HashSet<int>();
                                foreach (var el in subDoc.RootElement.GetProperty("Data").EnumerateArray())
                                    subIds.Add(el.GetProperty("Id").GetInt32());

                                fetched = fetched.Where(d => subIds.Contains(d.Id)).ToList();
                            }
                        }
                    }

                    lstResults.Items.Clear();
                    _currentDiseases = fetched;
                    foreach (var item in _currentDiseases)
                        lstResults.Items.Add(item.Name);

                    lblResultCount.Text = $"{_currentDiseases.Count} kết quả gợi ý tham khảo" +
                        (_activeFilters.Count > 0 ? $" · Đang lọc {_activeFilters.Count} điều kiện" : string.Empty);

                    if (_currentDiseases.Count == 0)
                    {
                        rtbDetail.Text = "Không tìm thấy kết quả phù hợp với từ khóa / bộ lọc đang áp dụng.";
                    }

                    string logKeyword = string.Join(", ",
                        new[] { keyword }.Concat(_activeFilters).Where(s => !string.IsNullOrWhiteSpace(s)));
                    if (!string.IsNullOrWhiteSpace(logKeyword))
                    {
                        await _userService.ThemLichSuAsync(logKeyword, null, _currentDiseases.Count);
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

                    rtbDetail.Text = $"=== {name.ToUpper()} (GỢI Ý THAM KHẢO) ===\n\n📌 Mô tả bệnh:\n{desc}\n\n💊 Hướng chăm sóc / tham khảo:\n{treat}";

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
