using System.Globalization;
using HealthClient.Services;
using HealthShared;

namespace HealthClient;

public sealed class FrmHoSo : Form
{
    private readonly UserProfileHistoryService _service = new();

    private Label lblTaiKhoan = null!;
    private TextBox txtHoTen = null!;
    private TextBox txtEmail = null!;
    private DateTimePicker dtpNgaySinh = null!;
    private ComboBox cboGioiTinh = null!;
    private TextBox txtChieuCao = null!;
    private TextBox txtCanNang = null!;
    private Label lblBmi = null!;
    private TextBox txtDiUng = null!;
    private TextBox txtBenhNen = null!;
    private TextBox txtGhiChu = null!;
    private Label lblTrangThai = null!;
    private Button btnLuu = null!;
    private Button btnDong = null!;

    public string? HoTenMoi { get; private set; }

    public FrmHoSo()
    {
        KhoiTaoGiaoDien();
        Load += async (_, _) => await TaiHoSoAsync();
    }

    private void KhoiTaoGiaoDien()
    {
        Text = "Hồ sơ sức khỏe cá nhân";
        ClientSize = new Size(560, 610);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);

        var lblTieuDe = new Label
        {
            Text = "HỒ SƠ SỨC KHỎE CÁ NHÂN",
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 118, 110),
            Location = new Point(24, 18),
            AutoSize = true
        };

        lblTaiKhoan = new Label
        {
            Text = "Tài khoản: đang tải...",
            ForeColor = Color.DimGray,
            Location = new Point(26, 48),
            AutoSize = true
        };

        int left = 24;
        int fullWidth = 512;

        // Họ tên
        Controls.Add(TaoNhan("Họ và tên (*)", left, 78));
        txtHoTen = new TextBox
        {
            Location = new Point(left, 100),
            Size = new Size(250, 27),
            MaxLength = 100
        };

        // Email
        Controls.Add(TaoNhan("Email", 288, 78));
        txtEmail = new TextBox
        {
            Location = new Point(288, 100),
            Size = new Size(248, 27),
            MaxLength = 254,
            PlaceholderText = "example@email.com"
        };

        // Ngày sinh
        Controls.Add(TaoNhan("Ngày sinh (tích chọn để nhập)", left, 138));
        dtpNgaySinh = new DateTimePicker
        {
            Location = new Point(left, 160),
            Size = new Size(250, 27),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            ShowCheckBox = true,
            Checked = false,
            MaxDate = DateTime.Today
        };

        // Giới tính
        Controls.Add(TaoNhan("Giới tính", 288, 138));
        cboGioiTinh = new ComboBox
        {
            Location = new Point(288, 160),
            Size = new Size(248, 27),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboGioiTinh.Items.AddRange(["-- Chưa chọn --", "Nam", "Nữ", "Khác"]);
        cboGioiTinh.SelectedIndex = 0;

        // Chiều cao - Cân nặng - BMI
        Controls.Add(TaoNhan("Chiều cao (cm)", left, 198));
        txtChieuCao = new TextBox
        {
            Location = new Point(left, 220),
            Size = new Size(155, 27),
            PlaceholderText = "VD: 170"
        };
        txtChieuCao.TextChanged += (_, _) => CapNhatBmi();

        Controls.Add(TaoNhan("Cân nặng (kg)", 196, 198));
        txtCanNang = new TextBox
        {
            Location = new Point(196, 220),
            Size = new Size(155, 27),
            PlaceholderText = "VD: 65.5"
        };
        txtCanNang.TextChanged += (_, _) => CapNhatBmi();

        Controls.Add(TaoNhan("Chỉ số BMI", 368, 198));
        lblBmi = new Label
        {
            Text = "--",
            Location = new Point(368, 223),
            Size = new Size(168, 24),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 64, 175)
        };

        // Dị ứng
        Controls.Add(TaoNhan("Tiền sử dị ứng (thuốc, thực phẩm...)", left, 258));
        txtDiUng = new TextBox
        {
            Location = new Point(left, 280),
            Size = new Size(fullWidth, 58),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            MaxLength = 2000
        };

        // Bệnh nền
        Controls.Add(TaoNhan("Bệnh nền (nếu có)", left, 348));
        txtBenhNen = new TextBox
        {
            Location = new Point(left, 370),
            Size = new Size(fullWidth, 58),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            MaxLength = 2000
        };

        // Ghi chú
        Controls.Add(TaoNhan("Ghi chú sức khỏe khác", left, 438));
        txtGhiChu = new TextBox
        {
            Location = new Point(left, 460),
            Size = new Size(fullWidth, 58),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            MaxLength = 2000
        };

        lblTrangThai = new Label
        {
            Text = string.Empty,
            ForeColor = Color.DimGray,
            Location = new Point(left, 530),
            Size = new Size(fullWidth, 20)
        };

        btnLuu = new Button
        {
            Text = "Lưu hồ sơ",
            Location = new Point(320, 560),
            Size = new Size(110, 34),
            BackColor = Color.FromArgb(15, 118, 110),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLuu.FlatAppearance.BorderSize = 0;
        btnLuu.Click += async (_, _) => await LuuHoSoAsync();

        btnDong = new Button
        {
            Text = "Đóng",
            Location = new Point(440, 560),
            Size = new Size(96, 34),
            UseVisualStyleBackColor = true,
            Cursor = Cursors.Hand
        };
        btnDong.Click += (_, _) => Close();

        Controls.AddRange([
            lblTieuDe, lblTaiKhoan,
            txtHoTen, txtEmail, dtpNgaySinh, cboGioiTinh,
            txtChieuCao, txtCanNang, lblBmi,
            txtDiUng, txtBenhNen, txtGhiChu,
            lblTrangThai, btnLuu, btnDong
        ]);
    }

    private static Label TaoNhan(string text, int x, int y) => new()
    {
        Text = text,
        Location = new Point(x, y),
        AutoSize = true,
        ForeColor = Color.FromArgb(51, 65, 85)
    };

    private async Task TaiHoSoAsync()
    {
        SetDangXuLy(true, "Đang tải hồ sơ từ Server...");
        var ketQua = await _service.LayHoSoAsync();
        SetDangXuLy(false);

        if (!ketQua.ThanhCong || ketQua.DuLieu is null)
        {
            lblTrangThai.Text = ketQua.ThongBao;
            MessageBox.Show(ketQua.ThongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DienDuLieuLenForm(ketQua.DuLieu);
    }

    private void DienDuLieuLenForm(UserProfilePayload p)
    {
        lblTaiKhoan.Text = $"Tài khoản: {p.TenDangNhap} (ID: {p.UserId})";
        txtHoTen.Text = p.HoTen;
        txtEmail.Text = p.Email ?? string.Empty;

        if (p.NgaySinh.HasValue)
        {
            dtpNgaySinh.Checked = true;
            dtpNgaySinh.Value = p.NgaySinh.Value.Date <= dtpNgaySinh.MaxDate
                ? p.NgaySinh.Value.Date
                : dtpNgaySinh.MaxDate;
        }
        else
        {
            dtpNgaySinh.Checked = false;
        }

        cboGioiTinh.SelectedIndex = p.GioiTinh switch
        {
            "Nam" => 1,
            "Nu" => 2,
            "Khac" => 3,
            _ => 0
        };

        txtChieuCao.Text = p.ChieuCaoCm?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
        txtCanNang.Text = p.CanNangKg?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
        txtDiUng.Text = p.DiUng ?? string.Empty;
        txtBenhNen.Text = p.BenhNen ?? string.Empty;
        txtGhiChu.Text = p.GhiChu ?? string.Empty;

        CapNhatBmi();

        lblTrangThai.Text = p.NgayCapNhat.HasValue
            ? $"Cập nhật lần cuối (UTC): {p.NgayCapNhat.Value:dd/MM/yyyy HH:mm}"
            : "Bạn chưa cập nhật thông tin sức khỏe bổ sung.";
    }

    private async Task LuuHoSoAsync()
    {
        string hoTen = txtHoTen.Text.Trim();
        if (string.IsNullOrWhiteSpace(hoTen))
        {
            MessageBox.Show("Vui lòng nhập họ và tên.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtHoTen.Focus();
            return;
        }

        if (!DocSoThapPhan(txtChieuCao.Text, out decimal? chieuCao))
        {
            MessageBox.Show("Chiều cao không hợp lệ (nhập số từ 1 đến 300 cm hoặc để trống).", "Dữ liệu chưa đúng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtChieuCao.Focus();
            return;
        }

        if (!DocSoThapPhan(txtCanNang.Text, out decimal? canNang))
        {
            MessageBox.Show("Cân nặng không hợp lệ (nhập số từ 1 đến 700 kg hoặc để trống).", "Dữ liệu chưa đúng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCanNang.Focus();
            return;
        }

        string? gioiTinh = cboGioiTinh.SelectedIndex switch
        {
            1 => "Nam",
            2 => "Nu",
            3 => "Khac",
            _ => null
        };

        var payload = new UpdateProfilePayload(
            HoTen: hoTen,
            Email: string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
            NgaySinh: dtpNgaySinh.Checked ? dtpNgaySinh.Value.Date : null,
            GioiTinh: gioiTinh,
            ChieuCaoCm: chieuCao,
            CanNangKg: canNang,
            DiUng: string.IsNullOrWhiteSpace(txtDiUng.Text) ? null : txtDiUng.Text.Trim(),
            BenhNen: string.IsNullOrWhiteSpace(txtBenhNen.Text) ? null : txtBenhNen.Text.Trim(),
            GhiChu: string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null : txtGhiChu.Text.Trim());

        SetDangXuLy(true, "Đang lưu hồ sơ...");
        var ketQua = await _service.CapNhatHoSoAsync(payload);
        SetDangXuLy(false);

        if (!ketQua.ThanhCong)
        {
            lblTrangThai.Text = ketQua.ThongBao;
            MessageBox.Show(ketQua.ThongBao, "Cập nhật thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        HoTenMoi = hoTen;
        if (ketQua.DuLieu is not null)
            DienDuLieuLenForm(ketQua.DuLieu);

        MessageBox.Show("Cập nhật hồ sơ sức khỏe thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CapNhatBmi()
    {
        if (DocSoThapPhan(txtChieuCao.Text, out decimal? h) && h is > 0 &&
            DocSoThapPhan(txtCanNang.Text, out decimal? w) && w is > 0)
        {
            decimal m = h.Value / 100m;
            decimal bmi = w.Value / (m * m);
            string phanLoai = bmi switch
            {
                < 18.5m => "Thiếu cân",
                < 25m => "Bình thường",
                < 30m => "Thừa cân",
                _ => "Béo phì"
            };
            lblBmi.Text = $"{bmi:0.0} ({phanLoai})";
        }
        else
        {
            lblBmi.Text = "--";
        }
    }

    private static bool DocSoThapPhan(string raw, out decimal? ketQua)
    {
        ketQua = null;
        if (string.IsNullOrWhiteSpace(raw))
            return true;

        string chuan = raw.Trim().Replace(',', '.');
        if (decimal.TryParse(chuan, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal val))
        {
            ketQua = val;
            return true;
        }
        return false;
    }

    private void SetDangXuLy(bool dangXuLy, string? thongBao = null)
    {
        btnLuu.Enabled = !dangXuLy;
        Cursor = dangXuLy ? Cursors.WaitCursor : Cursors.Default;
        if (thongBao is not null)
            lblTrangThai.Text = thongBao;
    }
}