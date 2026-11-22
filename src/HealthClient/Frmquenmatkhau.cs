using HealthClient.Services;
using HealthShared.Validation;

namespace HealthClient;

public sealed class FrmQuenMatKhau : Form
{
    private readonly TextBox _txtEmail = new();
    private readonly Button _btnGuiOtp = new();

    private readonly TextBox _txtOtp = new();
    private readonly Button _btnXacThucOtp = new();

    private readonly TextBox _txtMatKhauMoi = new();
    private readonly TextBox _txtXacNhanMatKhau = new();
    private readonly Button _btnDatLaiMatKhau = new();

    private readonly Label _lblTrangThai = new();
    private readonly Button _btnDong = new();

    private string? _verifiedEmail;
    private string? _resetToken;

    public bool DaDoiMatKhauThanhCong { get; private set; }

    public FrmQuenMatKhau()
    {
        Text = "Khôi phục mật khẩu";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(20, 16, 20, 18);
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.White;

        KhoiTaoGiaoDien();
        CapNhatTrangThaiBuoc(1);
    }

    private void KhoiTaoGiaoDien()
    {
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            MinimumSize = new Size(500, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var lblTieuDe = new Label
        {
            Text = "Đặt lại mật khẩu qua Email OTP",
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(20, 83, 136),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        // Bước 1: Email
        var lblEmail = TaoNhan("1. Email đã đăng ký:");
        _txtEmail.Dock = DockStyle.Fill;
        _txtEmail.Margin = new Padding(0, 0, 10, 6);
        _txtEmail.PlaceholderText = "ten@mien.com";

        CauHinhNut(_btnGuiOtp, "Gửi OTP");
        _btnGuiOtp.Click += async (_, _) => await GuiOtpAsync();

        // Bước 2: OTP
        var lblOtp = TaoNhan("2. Mã OTP 6 số (hiệu lực 15 phút, tối đa 5 lần sai):");
        _txtOtp.Dock = DockStyle.Fill;
        _txtOtp.Margin = new Padding(0, 0, 10, 6);
        _txtOtp.MaxLength = 6;
        _txtOtp.PlaceholderText = "123456";

        CauHinhNut(_btnXacThucOtp, "Xác thực OTP");
        _btnXacThucOtp.Click += async (_, _) => await XacThucOtpAsync();

        // Bước 3: Mật khẩu mới
        var lblMatKhauMoi = TaoNhan("3. Mật khẩu mới (8-64 ký tự, gồm chữ và số):");
        _txtMatKhauMoi.Dock = DockStyle.Fill;
        _txtMatKhauMoi.Margin = new Padding(0, 0, 0, 6);
        _txtMatKhauMoi.UseSystemPasswordChar = true;

        var lblXacNhan = TaoNhan("Nhập lại mật khẩu mới:");
        _txtXacNhanMatKhau.Dock = DockStyle.Fill;
        _txtXacNhanMatKhau.Margin = new Padding(0, 0, 0, 8);
        _txtXacNhanMatKhau.UseSystemPasswordChar = true;

        _lblTrangThai.AutoSize = true;
        _lblTrangThai.MaximumSize = new Size(490, 0);
        _lblTrangThai.Margin = new Padding(0, 6, 0, 10);
        _lblTrangThai.ForeColor = Color.FromArgb(180, 35, 24);

        var pnlNut = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 0)
        };

        CauHinhNut(_btnDong, "Đóng", minWidth: 96);
        _btnDong.Margin = new Padding(0);
        _btnDong.Click += (_, _) => Close();

        CauHinhNut(_btnDatLaiMatKhau, "Đặt lại mật khẩu", minWidth: 165);
        _btnDatLaiMatKhau.Margin = new Padding(0, 0, 10, 0);
        _btnDatLaiMatKhau.Click += async (_, _) => await DatLaiMatKhauAsync();

        pnlNut.Controls.Add(_btnDong);
        pnlNut.Controls.Add(_btnDatLaiMatKhau);

        int row = 0;
        layout.Controls.Add(lblTieuDe, 0, row);
        layout.SetColumnSpan(lblTieuDe, 2);

        row++;
        layout.Controls.Add(lblEmail, 0, row);
        layout.SetColumnSpan(lblEmail, 2);

        row++;
        layout.Controls.Add(_txtEmail, 0, row);
        layout.Controls.Add(_btnGuiOtp, 1, row);

        row++;
        layout.Controls.Add(lblOtp, 0, row);
        layout.SetColumnSpan(lblOtp, 2);

        row++;
        layout.Controls.Add(_txtOtp, 0, row);
        layout.Controls.Add(_btnXacThucOtp, 1, row);

        row++;
        layout.Controls.Add(lblMatKhauMoi, 0, row);
        layout.SetColumnSpan(lblMatKhauMoi, 2);

        row++;
        layout.Controls.Add(_txtMatKhauMoi, 0, row);
        layout.SetColumnSpan(_txtMatKhauMoi, 2);

        row++;
        layout.Controls.Add(lblXacNhan, 0, row);
        layout.SetColumnSpan(lblXacNhan, 2);

        row++;
        layout.Controls.Add(_txtXacNhanMatKhau, 0, row);
        layout.SetColumnSpan(_txtXacNhanMatKhau, 2);

        row++;
        layout.Controls.Add(_lblTrangThai, 0, row);
        layout.SetColumnSpan(_lblTrangThai, 2);

        row++;
        layout.Controls.Add(pnlNut, 0, row);
        layout.SetColumnSpan(pnlNut, 2);

        CancelButton = _btnDong;
        Controls.Add(layout);
    }

    private static Label TaoNhan(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Margin = new Padding(0, 8, 0, 4)
    };

    private static void CauHinhNut(Button btn, string text, int minWidth = 132)
    {
        btn.Text = text;
        btn.AutoSize = true;
        btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btn.MinimumSize = new Size(minWidth, 34);
        btn.Padding = new Padding(10, 4, 10, 4);
        btn.Margin = new Padding(0, 0, 0, 6);
    }

    private void CapNhatTrangThaiBuoc(int buoc)
    {
        _txtOtp.Enabled = buoc >= 2;
        _btnXacThucOtp.Enabled = buoc >= 2;

        _txtMatKhauMoi.Enabled = buoc >= 3;
        _txtXacNhanMatKhau.Enabled = buoc >= 3;
        _btnDatLaiMatKhau.Enabled = buoc >= 3;
    }

    private void DatThongBao(string noiDung, bool thanhCong = false)
    {
        _lblTrangThai.ForeColor = thanhCong
            ? Color.FromArgb(18, 128, 60)
            : Color.FromArgb(180, 35, 24);
        _lblTrangThai.Text = noiDung;
    }

    private void DatDangXuLy(bool dangXuLy)
    {
        UseWaitCursor = dangXuLy;
        _btnGuiOtp.Enabled = !dangXuLy;
        _btnXacThucOtp.Enabled = !dangXuLy && _verifiedEmail is not null;
        _btnDatLaiMatKhau.Enabled = !dangXuLy && _resetToken is not null;
    }

    private async Task GuiOtpAsync()
    {
        string email = _txtEmail.Text.Trim();
        var loi = InputValidator.KiemTraEmail(email);
        if (loi.Count > 0)
        {
            DatThongBao(string.Join(" ", loi.Values));
            _txtEmail.Focus();
            return;
        }

        _resetToken = null;
        DatDangXuLy(true);
        try
        {
            var kq = await AppServices.Auth.GuiOtpQuenMatKhauAsync(email);
            if (kq.Ma == MaKetQua.ThanhCong)
            {
                _verifiedEmail = email;
                CapNhatTrangThaiBuoc(2);
                DatThongBao(kq.ThongBao ?? "Đã gửi mã OTP nếu email hợp lệ.", thanhCong: true);
                _txtOtp.Focus();
            }
            else
            {
                DatThongBao(kq.ThongBao ?? "Không gửi được mã OTP.");
            }
        }
        finally
        {
            DatDangXuLy(false);
        }
    }

    private async Task XacThucOtpAsync()
    {
        string email = _verifiedEmail ?? _txtEmail.Text.Trim();
        string otp = _txtOtp.Text.Trim();

        var loiOtp = InputValidator.KiemTraOtp(otp);
        if (loiOtp.Count > 0)
        {
            DatThongBao(string.Join(" ", loiOtp.Values));
            _txtOtp.Focus();
            return;
        }

        DatDangXuLy(true);
        try
        {
            var kq = await AppServices.Auth.XacThucOtpQuenMatKhauAsync(email, otp);
            if (kq.Ma == MaKetQua.ThanhCong && !string.IsNullOrWhiteSpace(kq.ResetToken))
            {
                _resetToken = kq.ResetToken;
                CapNhatTrangThaiBuoc(3);
                DatThongBao(kq.ThongBao ?? "Xác thực OTP thành công. Hãy nhập mật khẩu mới.", thanhCong: true);
                _txtMatKhauMoi.Focus();
            }
            else
            {
                DatThongBao(kq.ThongBao ?? "Mã OTP không đúng hoặc đã hết hạn.");
            }
        }
        finally
        {
            DatDangXuLy(false);
        }
    }

    private async Task DatLaiMatKhauAsync()
    {
        if (string.IsNullOrWhiteSpace(_resetToken))
        {
            DatThongBao("Bạn cần xác thực OTP trước.");
            return;
        }

        string matKhauMoi = _txtMatKhauMoi.Text;
        string xacNhan = _txtXacNhanMatKhau.Text;

        var loiPwd = InputValidator.KiemTraDatLaiMatKhau(matKhauMoi);
        if (loiPwd.Count > 0)
        {
            DatThongBao(string.Join(" ", loiPwd.Values));
            _txtMatKhauMoi.Focus();
            return;
        }

        if (matKhauMoi != xacNhan)
        {
            DatThongBao("Hai ô mật khẩu mới không giống nhau.");
            _txtXacNhanMatKhau.Focus();
            return;
        }

        DatDangXuLy(true);
        try
        {
            var kq = await AppServices.Auth.DatLaiMatKhauBangTokenAsync(_resetToken, matKhauMoi);
            if (kq.Ma == MaKetQua.ThanhCong)
            {
                DaDoiMatKhauThanhCong = true;
                MessageBox.Show(
                    "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                DatThongBao(kq.ThongBao ?? "Không đặt lại được mật khẩu.");
            }
        }
        finally
        {
            DatDangXuLy(false);
        }
    }
}