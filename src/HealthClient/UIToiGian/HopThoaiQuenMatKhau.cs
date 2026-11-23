using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HealthClient.Services;
using HealthShared.Validation;

namespace HealthClient.UIToiGian
{
    internal interface IKhoiPhucMatKhau
    {
        Task<(bool ThanhCong, string ThongBao, string? MaDemo)> GuiMaKhoiPhucAsync(
            string tenDangNhap, string email, CancellationToken token = default);

        Task<(bool ThanhCong, string ThongBao)> XacMinhMaAsync(
            string tenDangNhap, string email, string maXacNhan, CancellationToken token = default);
    }

    internal sealed class KhoiPhucMatKhauDemo : IKhoiPhucMatKhau
    {
        public const int ThoiHanGiay = 60;

        private string? _ten;
        private string? _email;
        private string? _ma;
        private DateTime _hetHanUtc;

        public Task<(bool ThanhCong, string ThongBao, string? MaDemo)> GuiMaKhoiPhucAsync(
            string tenDangNhap, string email, CancellationToken token = default)
        {
            _ten = tenDangNhap.Trim();
            _email = email.Trim();
            _ma = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
            _hetHanUtc = DateTime.UtcNow.AddSeconds(ThoiHanGiay);

            string? maTraVe = AppTheme.CheDoDemoMail ? _ma : null;
            return Task.FromResult((true, "Đã gửi mã khôi phục gồm 6 chữ số tới email của bạn.", maTraVe));
        }

        public Task<(bool ThanhCong, string ThongBao)> XacMinhMaAsync(
            string tenDangNhap, string email, string maXacNhan, CancellationToken token = default)
        {
            if (_ma == null ||
                !string.Equals(_ten, tenDangNhap.Trim(), StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(_email, email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult((false, "Mã OTP không hợp lệ. Vui lòng gửi lại mã mới."));
            }

            if (DateTime.UtcNow > _hetHanUtc)
                return Task.FromResult((false, "Mã OTP đã hết hạn. Vui lòng bấm gửi lại mã mới."));

            if (!string.Equals(_ma, maXacNhan.Trim(), StringComparison.Ordinal))
                return Task.FromResult((false, "Mã OTP không hợp lệ. Vui lòng kiểm tra lại."));

            return Task.FromResult((true, "Xác minh thành công."));
        }
    }

    // Hop thoai Quen mat khau Ban 9:
    //  - Tang ClientSize (504x624) va dem du chieu cao cho moi label/FormField/ThanhBuoc/OTP -> KHONG cat bat ky net chu nao.
    internal sealed class HopThoaiQuenMatKhau : Form
    {
        private static readonly Regex RegexEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private readonly IKhoiPhucMatKhau _mail = new KhoiPhucMatKhauDemo();
        private readonly Label _lblTieuDe = new();
        private readonly Label _lblMoTa = new();
        private readonly ThanhBuoc _thanhBuoc = new();
        private readonly Panel _vungBuoc = new();
        private readonly TheGoiYNoi _goiYNoi = new();

        private readonly Stopwatch _swMo = new();
        private readonly System.Windows.Forms.Timer _nhipMo = new() { Interval = 16 };
        private int _dichTop;

        private readonly Panel _p1 = new();
        private readonly FormField _fTen = new();
        private readonly FormField _fEmail = new();
        private readonly Label _loi1 = new();
        private readonly FlatButton _btnDong1 = new();
        private readonly FlatButton _btnGuiMa = new();

        private readonly Panel _p2 = new();
        private readonly Label _lblEmailNhan = new();
        private readonly Panel _hopDemo = new();
        private readonly Label _lblMaDemo = new();
        private readonly FlatButton _btnDienMa = new();
        private readonly O6SoOtp _otp = new();
        private readonly Label _lblLoiOtp = new();
        private readonly FlatButton _btnGuiLai = new();
        private readonly Label _loi2 = new();
        private readonly FlatButton _btnQuayLai2 = new();
        private readonly FlatButton _btnXacMinh = new();

        private readonly Panel _p3 = new();
        private readonly FormField _fMkMoi = new();
        private readonly ThanhDoManh _doManh = new();
        private readonly FormField _fXacNhan = new();
        private readonly Label _loi3 = new();
        private readonly FlatButton _btnQuayLai3 = new();
        private readonly FlatButton _btnDatLai = new();

        private readonly Panel _p4 = new();
        private readonly FlatButton _btnDangNhapNgay = new();

        private readonly System.Windows.Forms.Timer _demNguoc = new() { Interval = 1000 };
        private int _giayConLai;
        private int _buocHienTai = 1;
        private string _tenDaNhap = string.Empty;
        private string _emailDaNhap = string.Empty;
        private string? _maDemo;
        private string? _resetToken;

        public string? TenDaDatLai { get; private set; }

        public HopThoaiQuenMatKhau(string tenGoiY)
        {
            Text = "Khôi phục mật khẩu";
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.NenTrang;
            Font = AppTheme.ChuThuong;
            ClientSize = new Size(this.Px(504), this.Px(624));
            Opacity = SystemInformation.UIEffectsEnabled ? 0.0 : 1.0;

            int pad = this.Px(26);
            int w = ClientSize.Width - pad * 2;

            _lblTieuDe.AutoSize = false;
            _lblTieuDe.UseMnemonic = false;
            _lblTieuDe.Font = AppTheme.TieuDe;
            _lblTieuDe.ForeColor = AppTheme.TieuDeMau;
            _lblTieuDe.Text = "Quên mật khẩu";
            _lblTieuDe.TextAlign = ContentAlignment.MiddleLeft;
            _lblTieuDe.Bounds = new Rectangle(pad, this.Px(14), w, this.Px(48));

            _lblMoTa.AutoSize = false;
            _lblMoTa.UseMnemonic = false;
            _lblMoTa.Font = AppTheme.ChuThuong;
            _lblMoTa.ForeColor = AppTheme.ChuPhu;
            _lblMoTa.TextAlign = ContentAlignment.MiddleLeft;
            _lblMoTa.Bounds = new Rectangle(pad, this.Px(64), w, this.Px(28));

            _thanhBuoc.Bounds = new Rectangle(pad, this.Px(98), w, this.Px(54));

            _vungBuoc.Bounds = new Rectangle(pad, this.Px(158), w, this.Px(450));
            _vungBuoc.BackColor = AppTheme.NenTrang;
            _vungBuoc.MouseDown += (_, _) => _goiYNoi.An();

            Controls.AddRange(new Control[] { _lblTieuDe, _lblMoTa, _thanhBuoc, _vungBuoc, _goiYNoi });
            _goiYNoi.BringToFront();

            TaoBuoc1(w, tenGoiY);
            TaoBuoc2(w);
            TaoBuoc3(w);
            TaoBuoc4(w);

            _vungBuoc.Controls.AddRange(new Control[] { _p1, _p2, _p3, _p4 });

            _demNguoc.Tick += (_, _) =>
            {
                if (!Visible)
                {
                    _demNguoc.Stop();
                    return;
                }
                if (_giayConLai > 0) _giayConLai--;
                CapNhatNutGuiLai();
                if (_giayConLai <= 0)
                {
                    _demNguoc.Stop();
                }
            };

            _nhipMo.Tick += (_, _) =>
            {
                float t = Math.Clamp(_swMo.ElapsedMilliseconds / 160f, 0f, 1f);
                float k = AppTheme.EaseOutCubic(t);
                Opacity = k;
                Top = _dichTop + (int)MathF.Round(this.Px(10) * (1f - k));
                if (t >= 1f)
                {
                    Opacity = 1.0;
                    Top = _dichTop;
                    _nhipMo.Stop();
                }
            };

            ChuyenBuoc(1, hieuUng: false);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (SystemInformation.UIEffectsEnabled)
            {
                _dichTop = Top;
                Top = _dichTop + this.Px(10);
                _swMo.Restart();
                _nhipMo.Start();
            }
            else
            {
                Opacity = 1.0;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                _demNguoc.Stop();
                _nhipMo.Stop();
            }
            else if (_giayConLai > 0)
            {
                _demNguoc.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _demNguoc.Stop();
                _demNguoc.Dispose();
                _nhipMo.Stop();
                _nhipMo.Dispose();
            }
            base.Dispose(disposing);
        }

        private void GanGoiYDialog(FormField f, Panel container, string tieuDe, params string[] dong)
        {
            f.TieuDeGoiY = tieuDe;
            f.NoiDungGoiY = dong;

            Rectangle VungIcon()
            {
                Rectangle ic = f.LayVungIconGoiY();
                return new Rectangle(_vungBuoc.Left + f.Left + ic.Left, _vungBuoc.Top + f.Top + ic.Top, ic.Width, ic.Height);
            }

            f.GoiYHoverChanged += (_, dangHover) =>
            {
                if (dangHover)
                {
                    if (!_goiYNoi.DangGhim)
                        _goiYNoi.HienTai(this, VungIcon(), tieuDe, dong, ghim: false);
                }
                else
                {
                    _goiYNoi.HenAnNeuKhongGhim();
                }
            };

            f.GoiYClicked += (_, _) =>
            {
                if (_goiYNoi.Visible && _goiYNoi.DangGhim) _goiYNoi.An();
                else _goiYNoi.HienTai(this, VungIcon(), tieuDe, dong, ghim: true);
            };

            container.MouseDown += (_, _) => _goiYNoi.An();
        }

        private void TaoBuoc1(int w, string tenGoiY)
        {
            _p1.Dock = DockStyle.Fill;
            _p1.BackColor = AppTheme.NenTrang;

            _fTen.ThuGon = true;
            _fTen.Nhan = "Tên đăng nhập";
            _fTen.BatBuoc = true;
            _fTen.IconTrai = LucideIconKind.User;
            _fTen.HienNutXoa = true;
            _fTen.PlaceholderText = "Nhập tên đăng nhập (3–20 ký tự)";
            _fTen.MaxLength = 20;
            _fTen.Location = new Point(0, this.Px(4));
            _fTen.Width = w;
            // Bat buoc moi nguoi dung deu tu nhap lai Ten dang nhap & Email (khong dien san du vua dang ky)
            _fTen.Text = string.Empty;
            _fEmail.Text = string.Empty;
            GanGoiYDialog(_fTen, _p1, "Tên đăng nhập cần khôi phục:",
                "Từ 3 đến 20 ký tự không dấu, số hoặc dấu _",
                "Khớp với tài khoản đã đăng ký trên hệ thống");

            _fEmail.ThuGon = true;
            _fEmail.Nhan = "Email đã đăng ký";
            _fEmail.BatBuoc = true;
            _fEmail.IconTrai = LucideIconKind.Mail;
            _fEmail.HienNutXoa = true;
            _fEmail.PlaceholderText = "ten@mien.com";
            _fEmail.MaxLength = 100;
            _fEmail.Location = new Point(0, _fTen.Bottom + this.Px(6));
            _fEmail.Width = w;
            GanGoiYDialog(_fEmail, _p1, "Email nhận mã đặt lại:",
                "Nhập đúng địa chỉ email đã đăng ký",
                "Định dạng chuẩn ten@mien.com");

            var hopHuongDan = TaoHopThongBao(
                new Rectangle(0, _fEmail.Bottom + this.Px(10), w, this.Px(76)),
                AppTheme.Nen, AppTheme.Vien, AppTheme.ChuPhu,
                "Hệ thống sẽ gửi mã xác nhận gồm 6 chữ số đến email đã đăng ký (hiệu lực 15 phút) để xác minh trước khi đặt lại mật khẩu.");

            _loi1.AutoSize = false;
            _loi1.UseMnemonic = false;
            _loi1.Font = AppTheme.ChuNhoDam;
            _loi1.ForeColor = AppTheme.Loi;
            _loi1.Bounds = new Rectangle(0, hopHuongDan.Bottom + this.Px(10), w, this.Px(36));

            int yNut = this.Px(388);
            _btnDong1.Kieu = KieuNut.Vien;
            _btnDong1.BanKinhBo = 6;
            _btnDong1.Text = "Đóng";
            _btnDong1.Bounds = new Rectangle(0, yNut, this.Px(108), this.Px(44));
            _btnDong1.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            _btnGuiMa.Kieu = KieuNut.Chinh;
            _btnGuiMa.BanKinhBo = 6;
            _btnGuiMa.LucideIcon = LucideIconKind.Send;
            _btnGuiMa.MuiTen = false;
            _btnGuiMa.Text = "Gửi email đặt lại mật khẩu";
            _btnGuiMa.Bounds = new Rectangle(this.Px(118), yNut, w - this.Px(118), this.Px(44));
            _btnGuiMa.Click += BtnGuiMa_Click;

            _p1.Controls.AddRange(new Control[] { _fTen, _fEmail, hopHuongDan, _loi1, _btnDong1, _btnGuiMa });
        }

        private void TaoBuoc2(int w)
        {
            _p2.Dock = DockStyle.Fill;
            _p2.BackColor = AppTheme.NenTrang;

            var hopMail = new Panel
            {
                Bounds = new Rectangle(0, this.Px(2), w, this.Px(108)),
                BackColor = AppTheme.Nen
            };
            hopMail.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, hopMail.Width - 1.5f, hopMail.Height - 1.5f);
                using var p = AppTheme.DuongBoGoc(r, this.Px(8));
                using var nen = new SolidBrush(AppTheme.Nen);
                using var vien = new Pen(AppTheme.Vien, 1f);
                e.Graphics.FillPath(nen, p);
                e.Graphics.DrawPath(vien, p);

                int dTron = this.Px(40);
                var rTron = new RectangleF(this.Px(14), (hopMail.Height - dTron) / 2f, dTron, dTron);
                using var bgTron = new SolidBrush(AppTheme.NenTrang);
                using var vienTron = new Pen(AppTheme.VienO, 1f);
                e.Graphics.FillEllipse(bgTron, rTron);
                e.Graphics.DrawEllipse(vienTron, rTron);

                int sz = this.Px(19);
                LucideIcons.Draw(e.Graphics, LucideIconKind.Mail,
                    new RectangleF(rTron.X + (dTron - sz) / 2f, rTron.Y + (dTron - sz) / 2f, sz, sz),
                    AppTheme.Chinh, 1.9f);
            };
            hopMail.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuNhoDam,
                ForeColor = AppTheme.Chinh,
                BackColor = AppTheme.Nen,
                Text = "Email cần xác minh",
                Bounds = new Rectangle(this.Px(66), this.Px(10), w - this.Px(78), this.Px(22))
            });
            _lblEmailNhan.AutoSize = false;
            _lblEmailNhan.UseMnemonic = false;
            _lblEmailNhan.Font = AppTheme.ChuDam;
            _lblEmailNhan.ForeColor = AppTheme.TieuDeMau;
            _lblEmailNhan.BackColor = AppTheme.Nen;
            _lblEmailNhan.Bounds = new Rectangle(this.Px(66), this.Px(33), w - this.Px(78), this.Px(26));
            hopMail.Controls.Add(_lblEmailNhan);
            hopMail.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuNho,
                ForeColor = AppTheme.ChuPhu,
                BackColor = AppTheme.Nen,
                Text = "Kiểm tra hộp thư đến hoặc thư rác để lấy mã 6 số (hiệu lực 15 phút).",
                Bounds = new Rectangle(this.Px(66), this.Px(60), w - this.Px(78), this.Px(42))
            });

            _hopDemo.Bounds = new Rectangle(0, hopMail.Bottom + this.Px(10), w, this.Px(48));
            _hopDemo.BackColor = AppTheme.DemoNen;
            _hopDemo.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, _hopDemo.Width - 1.5f, _hopDemo.Height - 1.5f);
                using var p = AppTheme.DuongBoGoc(r, this.Px(8));
                using var nen = new SolidBrush(AppTheme.DemoNen);
                using var vien = new Pen(AppTheme.DemoVien, 1f);
                e.Graphics.FillPath(nen, p);
                e.Graphics.DrawPath(vien, p);
            };
            _lblMaDemo.AutoSize = false;
            _lblMaDemo.UseMnemonic = false;
            _lblMaDemo.Font = AppTheme.ChuNhoDam;
            _lblMaDemo.ForeColor = AppTheme.DemoChu;
            _lblMaDemo.BackColor = AppTheme.DemoNen;
            _lblMaDemo.TextAlign = ContentAlignment.MiddleLeft;
            _lblMaDemo.Bounds = new Rectangle(this.Px(12), this.Px(4), w - this.Px(106), this.Px(40));

            _btnDienMa.Kieu = KieuNut.Nhat;
            _btnDienMa.BanKinhBo = 6;
            _btnDienMa.Text = "Điền mã";
            _btnDienMa.Font = AppTheme.ChuNhoDam;
            _btnDienMa.Bounds = new Rectangle(w - this.Px(88), this.Px(9), this.Px(78), this.Px(30));
            _btnDienMa.Click += (_, _) =>
            {
                if (!string.IsNullOrEmpty(_maDemo))
                {
                    _otp.DatMa(_maDemo);
                    if (_giayConLai > 0) _lblLoiOtp.Text = string.Empty;
                }
            };
            _hopDemo.Controls.AddRange(new Control[] { _lblMaDemo, _btnDienMa });

            var lblOtp = new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuNhan,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Mã xác nhận (6 chữ số)",
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(0, _hopDemo.Bottom + this.Px(12), w - this.Px(110), this.Px(28))
            };

            var btnDanMa = new FlatButton
            {
                Kieu = KieuNut.Vien,
                BanKinhBo = 4,
                Text = "Dán mã",
                Font = AppTheme.ChuNhoDam,
                Bounds = new Rectangle(w - this.Px(96), _hopDemo.Bottom + this.Px(12), this.Px(96), this.Px(28))
            };
            btnDanMa.Click += (_, _) =>
            {
                try
                {
                    if (!Clipboard.ContainsText()) return;
                    string clip = new string(Clipboard.GetText().Where(char.IsDigit).Take(6).ToArray());
                    if (clip.Length == 0)
                    {
                        _lblLoiOtp.Text = "Không tìm thấy mã số trong bộ nhớ tạm.";
                        return;
                    }
                    _otp.DatMa(clip);
                    _lblLoiOtp.Text = string.Empty;
                }
                catch { }
            };

            _otp.Bounds = new Rectangle(0, lblOtp.Bottom + this.Px(6), w, this.Px(54));
            _otp.GiaTriThayDoi += (_, _) =>
            {
                if (_giayConLai > 0) _lblLoiOtp.Text = string.Empty;
            };

            _lblLoiOtp.AutoSize = false;
            _lblLoiOtp.UseMnemonic = false;
            _lblLoiOtp.Font = AppTheme.ChuNhoDam;
            _lblLoiOtp.ForeColor = AppTheme.Loi;
            _lblLoiOtp.TextAlign = ContentAlignment.MiddleLeft;
            _lblLoiOtp.Bounds = new Rectangle(0, _otp.Bottom + this.Px(8), w, this.Px(28));

            _btnGuiLai.Kieu = KieuNut.Chu;
            _btnGuiLai.CanTrai = true;
            _btnGuiLai.LucideIcon = LucideIconKind.Clock;
            _btnGuiLai.Font = AppTheme.ChuNhoDam;
            _btnGuiLai.Bounds = new Rectangle(0, _lblLoiOtp.Bottom + this.Px(4), this.Px(260), this.Px(30));
            _btnGuiLai.Click += BtnGuiLai_Click;

            _loi2.AutoSize = false;
            _loi2.UseMnemonic = false;
            _loi2.Font = AppTheme.ChuNhoDam;
            _loi2.ForeColor = AppTheme.Loi;
            _loi2.Bounds = new Rectangle(0, _btnGuiLai.Bottom + this.Px(4), w, this.Px(26));

            int yNut = this.Px(388);
            _btnQuayLai2.Kieu = KieuNut.Vien;
            _btnQuayLai2.BanKinhBo = 6;
            _btnQuayLai2.Text = "Dùng email khác";
            _btnQuayLai2.Bounds = new Rectangle(0, yNut, this.Px(150), this.Px(44));
            _btnQuayLai2.Click += (_, _) => ChuyenBuoc(1);

            _btnXacMinh.Kieu = KieuNut.Chinh;
            _btnXacMinh.BanKinhBo = 6;
            _btnXacMinh.MuiTen = true;
            _btnXacMinh.Text = "Xác nhận mã";
            _btnXacMinh.Bounds = new Rectangle(this.Px(160), yNut, w - this.Px(160), this.Px(44));
            _btnXacMinh.Click += BtnXacMinh_Click;

            _p2.Controls.AddRange(new Control[]
            {
                hopMail, _hopDemo, lblOtp, btnDanMa, _otp, _lblLoiOtp, _btnGuiLai, _loi2, _btnQuayLai2, _btnXacMinh
            });
        }

        private void TaoBuoc3(int w)
        {
            _p3.Dock = DockStyle.Fill;
            _p3.BackColor = AppTheme.NenTrang;

            _fMkMoi.ThuGon = true;
            _fMkMoi.Nhan = "Mật khẩu mới";
            _fMkMoi.BatBuoc = true;
            _fMkMoi.IconTrai = LucideIconKind.Lock;
            _fMkMoi.LaMatKhau = true;
            _fMkMoi.PlaceholderText = "Tối thiểu 8 ký tự, gồm chữ và số";
            _fMkMoi.MaxLength = 64;
            _fMkMoi.Location = new Point(0, this.Px(4));
            _fMkMoi.Width = w;
            _fMkMoi.TextChanged += (_, _) => _doManh.Dat(_fMkMoi.Text);
            GanGoiYDialog(_fMkMoi, _p3, "Quy tắc mật khẩu mới:",
                "Từ 8 đến 64 ký tự",
                "Có ít nhất một chữ cái và một chữ số",
                "Nên thêm chữ hoa và ký tự đặc biệt (@#$)");

            _doManh.Bounds = new Rectangle(0, _fMkMoi.Bottom + this.Px(4), w, this.Px(58));

            _fXacNhan.ThuGon = true;
            _fXacNhan.Nhan = "Xác nhận mật khẩu mới";
            _fXacNhan.BatBuoc = true;
            _fXacNhan.IconTrai = LucideIconKind.Lock;
            _fXacNhan.LaMatKhau = true;
            _fXacNhan.PlaceholderText = "Nhập lại mật khẩu mới";
            _fXacNhan.MaxLength = 64;
            _fXacNhan.Location = new Point(0, _doManh.Bottom + this.Px(10));
            _fXacNhan.Width = w;
            GanGoiYDialog(_fXacNhan, _p3, "Xác nhận mật khẩu:",
                "Nhập lại giống hệt mật khẩu mới ở trên");

            _loi3.AutoSize = false;
            _loi3.UseMnemonic = false;
            _loi3.Font = AppTheme.ChuNhoDam;
            _loi3.ForeColor = AppTheme.Loi;
            _loi3.TextAlign = ContentAlignment.TopLeft;
            _loi3.Bounds = new Rectangle(0, _fXacNhan.Bottom + this.Px(6), w, this.Px(56));

            int yNut = this.Px(388);
            _btnQuayLai3.Kieu = KieuNut.Vien;
            _btnQuayLai3.BanKinhBo = 6;
            _btnQuayLai3.Text = "Quay lại";
            _btnQuayLai3.Bounds = new Rectangle(0, yNut, this.Px(116), this.Px(44));
            _btnQuayLai3.Click += (_, _) => ChuyenBuoc(2);

            _btnDatLai.Kieu = KieuNut.Chinh;
            _btnDatLai.BanKinhBo = 6;
            _btnDatLai.MuiTen = true;
            _btnDatLai.Text = "Đặt lại mật khẩu";
            _btnDatLai.Bounds = new Rectangle(this.Px(126), yNut, w - this.Px(126), this.Px(44));
            _btnDatLai.Click += BtnDatLai_Click;

            _p3.Controls.AddRange(new Control[]
            {
                _fMkMoi, _doManh, _fXacNhan, _loi3, _btnQuayLai3, _btnDatLai
            });
        }

        private void TaoBuoc4(int w)
        {
            _p4.Dock = DockStyle.Fill;
            _p4.BackColor = AppTheme.NenTrang;

            var bieuTuong = new Panel
            {
                Bounds = new Rectangle((w - this.Px(78)) / 2, this.Px(44), this.Px(78), this.Px(78)),
                BackColor = AppTheme.NenTrang
            };
            bieuTuong.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(2, 2, bieuTuong.Width - 5, bieuTuong.Height - 5);
                using var bg = new SolidBrush(AppTheme.ChipMint);
                using var pen = new Pen(AppTheme.ThanhCong, 2f);
                e.Graphics.FillEllipse(bg, r);
                e.Graphics.DrawEllipse(pen, r);

                int sz = this.Px(38);
                LucideIcons.Draw(e.Graphics, LucideIconKind.CircleCheck,
                    new RectangleF((bieuTuong.Width - sz) / 2f, (bieuTuong.Height - sz) / 2f, sz, sz),
                    AppTheme.ThanhCong, 2.1f);
            };

            var td = new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.TieuDe,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Đặt lại mật khẩu thành công!",
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(0, this.Px(140), w, this.Px(50))
            };

            var moTa = new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuLon,
                ForeColor = AppTheme.ChuPhu,
                Text = "Mật khẩu tài khoản của bạn đã được cập nhật.\nNhấn \"Đăng nhập ngay\" để tiếp tục sử dụng hệ thống.",
                TextAlign = ContentAlignment.TopCenter,
                Bounds = new Rectangle(this.Px(12), this.Px(194), w - this.Px(24), this.Px(60))
            };

            _btnDangNhapNgay.Kieu = KieuNut.Chinh;
            _btnDangNhapNgay.BanKinhBo = 6;
            _btnDangNhapNgay.MuiTen = true;
            _btnDangNhapNgay.Text = "Đăng nhập ngay";
            _btnDangNhapNgay.Bounds = new Rectangle(this.Px(40), this.Px(288), w - this.Px(80), this.Px(46));
            _btnDangNhapNgay.Click += (_, _) =>
            {
                TenDaDatLai = _tenDaNhap;
                DialogResult = DialogResult.OK;
                Close();
            };

            _p4.Controls.AddRange(new Control[] { bieuTuong, td, moTa, _btnDangNhapNgay });
        }

        private Panel TaoHopThongBao(Rectangle bounds, Color nen, Color vien, Color mauChu, string noiDung)
        {
            var p = new Panel { Bounds = bounds, BackColor = nen };
            p.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, p.Width - 1.5f, p.Height - 1.5f);
                using var path = AppTheme.DuongBoGoc(r, this.Px(8));
                using var b = new SolidBrush(nen);
                using var pen = new Pen(vien, 1f);
                e.Graphics.FillPath(b, path);
                e.Graphics.DrawPath(pen, path);
            };
            p.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuNho,
                ForeColor = mauChu,
                BackColor = nen,
                Text = noiDung,
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(this.Px(14), this.Px(8), bounds.Width - this.Px(28), bounds.Height - this.Px(16))
            });
            return p;
        }

        private void ChuyenBuoc(int buoc, bool hieuUng = true)
        {
            _goiYNoi.An();
            int cu = _buocHienTai;
            _buocHienTai = Math.Clamp(buoc, 1, 4);
            _thanhBuoc.Buoc = Math.Min(_buocHienTai, 3);
            _thanhBuoc.HoanTatHet = _buocHienTai == 4;

            _lblTieuDe.Text = _buocHienTai switch
            {
                2 => "Vui lòng xác minh email",
                3 => "Đặt mật khẩu mới",
                4 => "Khôi phục hoàn tất",
                _ => "Quên mật khẩu"
            };

            _lblMoTa.Text = _buocHienTai switch
            {
                1 => "Bước 1/3: Nhập tài khoản và email để nhận mã khôi phục.",
                2 => "Bước 2/3: Nhập mã xác nhận 6 chữ số đã gửi về email.",
                3 => "Bước 3/3: Tạo mật khẩu mới an toàn cho tài khoản của bạn.",
                _ => "Tài khoản đã sẵn sàng đăng nhập lại."
            };

            void ApDung()
            {
                _p1.Visible = _buocHienTai == 1;
                _p2.Visible = _buocHienTai == 2;
                _p3.Visible = _buocHienTai == 3;
                _p4.Visible = _buocHienTai == 4;
            }

            if (hieuUng && IsHandleCreated && Visible && _buocHienTai != cu)
                HieuUngChuyen.Chay(_vungBuoc, ApDung, _buocHienTai > cu ? 1 : -1);
            else
                ApDung();

            AcceptButton = _buocHienTai switch
            {
                1 => _btnGuiMa,
                2 => _btnXacMinh,
                3 => _btnDatLai,
                _ => _btnDangNhapNgay
            };
            CancelButton = _buocHienTai switch
            {
                1 => _btnDong1,
                2 => _btnQuayLai2,
                3 => _btnQuayLai3,
                _ => _btnDangNhapNgay
            };

            if (_buocHienTai == 1) _fTen.Focus();
            else if (_buocHienTai == 2) _otp.Focus();
            else if (_buocHienTai == 3) _fMkMoi.Focus();
            else _btnDangNhapNgay.Focus();
        }

        private void BatDemNguocGuiLai()
        {
            _giayConLai = KhoiPhucMatKhauDemo.ThoiHanGiay;
            CapNhatNutGuiLai();
            if (Visible) _demNguoc.Start();
        }

        private void CapNhatNutGuiLai()
        {
            if (_giayConLai > 0)
            {
                _btnGuiLai.Enabled = false;
                _btnGuiLai.Text = $"Gửi lại email sau {_giayConLai} giây";
            }
            else
            {
                _btnGuiLai.Enabled = true;
                _btnGuiLai.Text = "Gửi lại email xác nhận";
            }
        }

        private async void BtnGuiMa_Click(object? sender, EventArgs e)
        {
            _fTen.Loi = string.Empty;
            _fEmail.Loi = string.Empty;
            _loi1.Text = string.Empty;

            string ten = _fTen.Text.Trim();
            string email = _fEmail.Text.Trim();

            FormField? dau = null;
            if (ten.Length == 0)
            {
                _fTen.Loi = "Vui lòng nhập tên đăng nhập.";
                dau ??= _fTen;
            }
            if (email.Length == 0)
            {
                _fEmail.Loi = "Vui lòng nhập email đã đăng ký.";
                dau ??= _fEmail;
            }
            else if (email.Length > 100 || !RegexEmail.IsMatch(email))
            {
                _fEmail.Loi = "Email phải có dạng ten@mien.com.";
                dau ??= _fEmail;
            }
            if (dau != null) { dau.Focus(); return; }

            _btnGuiMa.Enabled = false;
            _btnDong1.Enabled = false;
            _btnGuiMa.DangXuLy = true;
            _btnGuiMa.Text = "Đang gửi email...";

            try
            {
                KetQuaXacThuc kq = await AppServices.Auth.GuiOtpQuenMatKhauAsync(email, ten);
                if (kq.Ma == MaKetQua.LoiKetNoi)
                {
                    _loi1.Text = "Không kết nối được tới máy chủ. Vui lòng thử lại.";
                    return;
                }
                if (kq.Ma != MaKetQua.ThanhCong)
                {
                    _loi1.Text = kq.ThongBao ?? "Không thể gửi yêu cầu khôi phục.";
                    return;
                }

                _tenDaNhap = ten;
                _emailDaNhap = email;
                _resetToken = null;
                _maDemo = null;
                _lblEmailNhan.Text = _emailDaNhap;

                _hopDemo.Visible = false;
                _otp.Xoa();
                _lblLoiOtp.Text = string.Empty;
                _loi2.Text = string.Empty;

                BatDemNguocGuiLai();
                ChuyenBuoc(2);
            }
            catch (Exception)
            {
                _loi1.Text = "Không thể gửi yêu cầu khôi phục. Vui lòng thử lại.";
            }
            finally
            {
                if (!IsDisposed)
                {
                    _btnGuiMa.DangXuLy = false;
                    _btnGuiMa.Enabled = true;
                    _btnDong1.Enabled = true;
                    _btnGuiMa.Text = "Gửi email đặt lại mật khẩu";
                }
            }
        }

        private async void BtnGuiLai_Click(object? sender, EventArgs e)
        {
            if (_giayConLai > 0) return;
            _loi2.Text = string.Empty;
            _lblLoiOtp.Text = string.Empty;
            KetQuaXacThuc kq = await AppServices.Auth.GuiOtpQuenMatKhauAsync(_emailDaNhap, _tenDaNhap);
            if (kq.Ma == MaKetQua.ThanhCong)
            {
                _resetToken = null;
                _otp.Xoa();
                BatDemNguocGuiLai();
                _otp.Focus();
            }
            else
            {
                _loi2.Text = kq.ThongBao ?? "Không thể gửi lại mã xác nhận.";
            }
        }

        private async void BtnXacMinh_Click(object? sender, EventArgs e)
        {
            _lblLoiOtp.Text = string.Empty;
            _loi2.Text = string.Empty;

            string ma = _otp.Ma.Trim();
            if (ma.Length != 6 || !ma.All(char.IsDigit))
            {
                _lblLoiOtp.Text = "Mã OTP không hợp lệ. Vui lòng nhập đủ 6 chữ số.";
                _otp.Focus();
                return;
            }

            _btnXacMinh.Enabled = false;
            _btnQuayLai2.Enabled = false;
            _btnXacMinh.DangXuLy = true;
            _btnXacMinh.Text = "Đang xác minh...";
            try
            {
                KetQuaXacThuc kq = await AppServices.Auth.XacThucOtpQuenMatKhauAsync(_emailDaNhap, ma);
                if (kq.Ma == MaKetQua.LoiKetNoi)
                {
                    _loi2.Text = "Không kết nối được tới máy chủ. Vui lòng thử lại.";
                    return;
                }
                if (kq.Ma != MaKetQua.ThanhCong || string.IsNullOrWhiteSpace(kq.ResetToken))
                {
                    _lblLoiOtp.Text = kq.ThongBao ?? "Mã OTP không đúng hoặc đã hết hạn.";
                    _otp.Focus();
                    return;
                }

                _resetToken = kq.ResetToken;
                _fMkMoi.Clear();
                _fXacNhan.Clear();
                _doManh.Dat(string.Empty);
                _loi3.Text = string.Empty;
                ChuyenBuoc(3);
            }
            catch (Exception)
            {
                _loi2.Text = "Không thể xác minh mã lúc này. Vui lòng thử lại.";
            }
            finally
            {
                if (!IsDisposed)
                {
                    _btnXacMinh.DangXuLy = false;
                    _btnXacMinh.Enabled = true;
                    _btnQuayLai2.Enabled = true;
                    _btnXacMinh.Text = "Xác nhận mã";
                }
            }
        }

        private async void BtnDatLai_Click(object? sender, EventArgs e)
        {
            _fMkMoi.Loi = string.Empty;
            _fXacNhan.Loi = string.Empty;
            _loi3.Text = string.Empty;

            string mkMoi = _fMkMoi.Text;
            string xacNhan = _fXacNhan.Text;

            var loi = InputValidator.KiemTraQuenMatKhau(_tenDaNhap, _emailDaNhap, mkMoi);
            if (xacNhan.Length == 0)
                loi["XacNhan"] = "Vui lòng nhập lại mật khẩu mới.";
            else if (xacNhan != mkMoi)
                loi["XacNhan"] = "Hai ô mật khẩu không giống nhau.";

            FormField? dau = null;
            if (loi.TryGetValue(InputValidator.Truong.MatKhau, out string? lMk)) { _fMkMoi.Loi = lMk; dau ??= _fMkMoi; }
            if (loi.TryGetValue("XacNhan", out string? lXn)) { _fXacNhan.Loi = lXn; dau ??= _fXacNhan; }
            if (dau != null) { dau.Focus(); return; }

            _btnDatLai.Enabled = false;
            _btnQuayLai3.Enabled = false;
            _btnDatLai.DangXuLy = true;
            _btnDatLai.Text = "Đang cập nhật...";

            try
            {
                if (string.IsNullOrWhiteSpace(_resetToken))
                {
                    _loi3.Text = "Phiên đặt lại mật khẩu đã hết hạn. Vui lòng quay lại Bước 1 xin mã mới.";
                    return;
                }
                KetQuaXacThuc kq = await AppServices.Auth.DatLaiMatKhauBangTokenAsync(_resetToken, mkMoi);
                switch (kq.Ma)
                {
                    case MaKetQua.ThanhCong:
                        _resetToken = null;
                        _demNguoc.Stop();
                        ChuyenBuoc(4);
                        return;

                    case MaKetQua.LoiKetNoi:
                        _loi3.Text = "Không kết nối được tới máy chủ. Hãy thử lại.";
                        break;

                    default:
                        _loi3.Text = kq.ThongBao ?? "Tên đăng nhập hoặc email không khớp với tài khoản.";
                        break;
                }
            }
            catch (Exception)
            {
                _loi3.Text = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
            }
            finally
            {
                if (!IsDisposed)
                {
                    _btnDatLai.DangXuLy = false;
                    _btnDatLai.Enabled = true;
                    _btnQuayLai3.Enabled = true;
                    _btnDatLai.Text = "Đặt lại mật khẩu";
                }
            }
        }

        private sealed class O6SoOtp : UserControl
        {
            private static readonly Font FontSo = new("Segoe UI", 16F, FontStyle.Bold);
            private readonly TextBox[] _o = new TextBox[6];
            private bool _dangGan;

            public event EventHandler? GiaTriThayDoi;

            public string Ma => string.Concat(_o.Select(tb => tb.Text));

            public O6SoOtp()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = AppTheme.NenTrang;

                for (int i = 0; i < 6; i++)
                {
                    int idx = i;
                    var tb = new TextBox
                    {
                        BorderStyle = BorderStyle.None,
                        MaxLength = 6,
                        TextAlign = HorizontalAlignment.Center,
                        Font = FontSo,
                        ForeColor = AppTheme.TieuDeMau,
                        BackColor = AppTheme.NenTrang
                    };
                    tb.Enter += (_, _) => Invalidate();
                    tb.Leave += (_, _) => Invalidate();
                    tb.KeyPress += (_, ke) =>
                    {
                        if (!char.IsControl(ke.KeyChar) && !char.IsDigit(ke.KeyChar))
                            ke.Handled = true;
                    };
                    tb.KeyDown += (_, ke) =>
                    {
                        if (ke.KeyCode == Keys.Back && tb.TextLength == 0 && idx > 0)
                        {
                            _o[idx - 1].Clear();
                            _o[idx - 1].Focus();
                            ke.Handled = true;
                        }
                        else if (ke.KeyCode == Keys.Left && idx > 0)
                        {
                            _o[idx - 1].Focus();
                            ke.Handled = true;
                        }
                        else if (ke.KeyCode == Keys.Right && idx < 5)
                        {
                            _o[idx + 1].Focus();
                            ke.Handled = true;
                        }
                    };
                    tb.TextChanged += (_, _) => XuLyThayDoi(idx);
                    _o[i] = tb;
                    Controls.Add(tb);
                }

                BoTriO();
            }

            public void Xoa() => DatMa(string.Empty);

            public void DatMa(string ma)
            {
                _dangGan = true;
                try
                {
                    string digits = new(ma.Where(char.IsDigit).Take(6).ToArray());
                    for (int i = 0; i < 6; i++)
                        _o[i].Text = i < digits.Length ? digits[i].ToString() : string.Empty;
                }
                finally
                {
                    _dangGan = false;
                }
                Invalidate();
                GiaTriThayDoi?.Invoke(this, EventArgs.Empty);
            }

            public new bool Focus()
            {
                int k = Array.FindIndex(_o, tb => tb.TextLength == 0);
                return _o[k >= 0 ? k : 5].Focus();
            }

            private void XuLyThayDoi(int idx)
            {
                if (_dangGan) return;
                _dangGan = true;
                try
                {
                    string raw = _o[idx].Text;
                    string digits = new(raw.Where(char.IsDigit).ToArray());
                    if (digits.Length > 1)
                    {
                        for (int k = 0; k < digits.Length && idx + k < 6; k++)
                            _o[idx + k].Text = digits[k].ToString();
                        int next = Math.Min(5, idx + digits.Length);
                        _o[next].Focus();
                    }
                    else
                    {
                        if (raw != digits) _o[idx].Text = digits;
                        if (digits.Length == 1 && idx < 5)
                            _o[idx + 1].Focus();
                    }
                }
                finally
                {
                    _dangGan = false;
                }
                Invalidate();
                GiaTriThayDoi?.Invoke(this, EventArgs.Empty);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                BoTriO();
            }

            private void BoTriO()
            {
                int gap = this.Px(10);
                int wO = Math.Max(this.Px(34), (Width - gap * 5) / 6);
                int xStart = Math.Max(0, (Width - (wO * 6 + gap * 5)) / 2);
                for (int i = 0; i < 6; i++)
                {
                    int x = xStart + i * (wO + gap);
                    _o[i].SetBounds(x + this.Px(4), Math.Max(this.Px(4), (Height - _o[i].Height) / 2), Math.Max(10, wO - this.Px(8)), _o[i].Height);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Parent?.BackColor ?? AppTheme.NenTrang);

                int gap = this.Px(10);
                int wO = Math.Max(this.Px(34), (Width - gap * 5) / 6);
                int xStart = Math.Max(0, (Width - (wO * 6 + gap * 5)) / 2);

                for (int i = 0; i < 6; i++)
                {
                    int x = xStart + i * (wO + gap);
                    var r = new RectangleF(x + 1f, 2f, wO - 2f, Height - 4f);
                    bool focus = _o[i].Focused;
                    bool coSo = _o[i].TextLength > 0;
                    Color vien = focus ? AppTheme.ChinhSang : coSo ? AppTheme.Chinh : AppTheme.VienO;

                    using var p = AppTheme.DuongBoGoc(r, this.Px(6));
                    using var bg = new SolidBrush(AppTheme.NenTrang);
                    using var pen = new Pen(vien, focus ? 2f : 1.35f);
                    g.FillPath(bg, p);
                    if (focus)
                    {
                        using var halo = new Pen(AppTheme.VienOCoFocus, 3f);
                        g.DrawPath(halo, p);
                    }
                    g.DrawPath(pen, p);
                }
            }
        }

        private sealed class ThanhBuoc : Control
        {
            private static readonly string[] Ten = { "Gửi email", "Xác nhận mã", "Mật khẩu mới" };
            private int _buoc = 1;
            private bool _hoanTatHet;

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int Buoc
            {
                get => _buoc;
                set { _buoc = Math.Clamp(value, 1, 3); Invalidate(); }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool HoanTatHet
            {
                get => _hoanTatHet;
                set { _hoanTatHet = value; Invalidate(); }
            }

            public ThanhBuoc()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = AppTheme.NenTrang;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(AppTheme.NenTrang);

                float wCot = Width / 3f;
                int d = this.Px(24);
                int yTron = this.Px(2);

                for (int i = 0; i < 2; i++)
                {
                    float x1 = (i + 0.5f) * wCot + d / 2f + this.Px(4);
                    float x2 = (i + 1.5f) * wCot - d / 2f - this.Px(4);
                    bool xong = _hoanTatHet || (i + 1) < _buoc;
                    using var pen = new Pen(xong ? AppTheme.ThanhCong : AppTheme.Vien, 2f);
                    g.DrawLine(pen, x1, yTron + d / 2f, x2, yTron + d / 2f);
                }

                for (int i = 0; i < 3; i++)
                {
                    int so = i + 1;
                    float cx = (i + 0.5f) * wCot;
                    var rTron = new RectangleF(cx - d / 2f, yTron, d, d);
                    bool xong = _hoanTatHet || so < _buoc;
                    bool hienTai = !_hoanTatHet && so == _buoc;

                    Color nen = xong ? AppTheme.ThanhCong : hienTai ? AppTheme.Chinh : AppTheme.Nen;
                    Color vien = xong ? AppTheme.ThanhCong : hienTai ? AppTheme.Chinh : AppTheme.VienDam;

                    using (var b = new SolidBrush(nen))
                        g.FillEllipse(b, rTron);
                    using (var pen = new Pen(vien, 1.5f))
                        g.DrawEllipse(pen, rTron);

                    if (xong)
                    {
                        using var tick = new Pen(Color.White, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawLines(tick, new[]
                        {
                            new PointF(rTron.X + d * 0.28f, rTron.Y + d * 0.52f),
                            new PointF(rTron.X + d * 0.45f, rTron.Y + d * 0.70f),
                            new PointF(rTron.X + d * 0.74f, rTron.Y + d * 0.32f)
                        });
                    }
                    else
                    {
                        TextRenderer.DrawText(g, so.ToString(), AppTheme.ChuNhoDam, Rectangle.Round(rTron),
                            hienTai ? Color.White : AppTheme.ChuPhu,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    }

                    TextRenderer.DrawText(g, Ten[i], hienTai ? AppTheme.ChuNhoDam : AppTheme.ChuNho,
                        new Rectangle((int)(i * wCot), yTron + d + this.Px(4), (int)wCot, this.Px(22)),
                        hienTai ? AppTheme.TieuDeMau : xong ? AppTheme.ThanhCong : AppTheme.ChuPhu,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                }
            }
        }
    }
}
