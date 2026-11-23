using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HealthClient.Services;
using HealthShared.Validation;

namespace HealthClient.UIToiGian
{
    // Trang Dang ky Ban 6:
    //  - Cot trai: bo cau "Ho so gon gang...", giu anh hoa mem nen chung + "Suc khoe la tai san quy gia nhat" (khong nghieng)
    //  - Cot phai: 1 COT DOC RONG RAI (440px) — Ho va ten, Ten dang nhap, Email, Mat khau, Do manh, Xac nhan mat khau
    //    de ten dai khong bi khuat va chu loi do 22px khong bi mat net
    //  - Placeholder mat khau ro rang (bo "8+").
    internal sealed class TrangDangKy : TrangXacThuc
    {
        private static readonly Regex RegexTenDangNhap = new(@"^[A-Za-z0-9_]{3,20}$");
        private static readonly Regex RegexEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private readonly FormField _hoTen = new();
        private readonly FormField _ten = new();
        private readonly FormField _email = new();
        private readonly FormField _matKhau = new();
        private readonly ThanhDoManh _doManh = new();
        private readonly FormField _xacNhan = new();
        private readonly OTich _dongY = new();
        private readonly Label _loiChung = new();
        private readonly FlatButton _nut = new();

        public override IButtonControl? NutMacDinh => _nut;
        public override FormField? OBatDau => _hoTen;

        public TrangDangKy(FrmVo shell, int tabTruoc = -1)
            : base(shell, 1, tabTruoc, 440)
        {
            Trai.DatCheDo(
                KieuMinhHoa.DangKy,
                danhSachY: null,
                khauHieuPhu: null);

            int w = Cot.Width;

            Cot.Controls.Add(TaoTieuDe("Tạo hồ sơ sức khỏe của bạn", this.Px(54)));
            Cot.Controls.Add(TaoMoTa("Chỉ vài bước để lưu lịch sử tra cứu và sử dụng thuận tiện hơn.", this.Px(102), 34));

            int y = this.Px(144);
            int gapField = this.Px(4);

            void ThemTruong(FormField f, string nhan, bool batBuoc, LucideIconKind icon, string placeholder, int maxLen, bool mk = false)
            {
                f.ThuGon = true;
                f.Nhan = nhan;
                f.BatBuoc = batBuoc;
                f.IconTrai = icon;
                f.PlaceholderText = placeholder;
                f.MaxLength = maxLen;
                f.HienNutXoa = !mk;
                f.LaMatKhau = mk;
                f.Location = new Point(0, y);
                f.Width = w;
                Cot.Controls.Add(f);
                y = f.Bottom + gapField;
            }

            ThemTruong(_hoTen, "Họ và tên", true, LucideIconKind.User, "Nhập họ và tên đầy đủ", 50);
            GanGoiY(_hoTen, "Họ và tên hợp lệ:",
                "Từ 2 đến 50 ký tự",
                "Chỉ gồm chữ cái tiếng Việt và khoảng trắng");

            ThemTruong(_ten, "Tên đăng nhập", true, LucideIconKind.User, "Ví dụ: nguyenvana_01", 20);
            GanGoiY(_ten, "Tên đăng nhập hợp lệ:",
                "Từ 3 đến 20 ký tự, không trùng người khác",
                "Chữ cái không dấu, chữ số hoặc dấu gạch dưới (_)");

            ThemTruong(_email, "Email", AppTheme.EmailBatBuoc, LucideIconKind.Mail, "ten@mien.com", 100);
            GanGoiY(_email, "Email cá nhân:",
                AppTheme.EmailBatBuoc ? "Bắt buộc nhập theo dạng ten@mien.com" : "Không bắt buộc, theo dạng ten@mien.com",
                "Dùng để xác minh và khôi phục mật khẩu");

            ThemTruong(_matKhau, "Mật khẩu", true, LucideIconKind.Lock, "Tối thiểu 8 ký tự, gồm chữ và số", 64, true);
            GanGoiY(_matKhau, "Quy định mật khẩu:",
                "Độ dài từ 8 đến 64 ký tự",
                "Bắt buộc có ít nhất 1 chữ cái và 1 chữ số",
                "Nên kết hợp chữ hoa và ký tự đặc biệt (@#$)");

            _doManh.Bounds = new Rectangle(0, y, w, this.Px(54));
            Cot.Controls.Add(_doManh);
            _matKhau.TextChanged += (_, _) => _doManh.Dat(_matKhau.Text);
            y = _doManh.Bottom + this.Px(8);

            ThemTruong(_xacNhan, "Xác nhận mật khẩu", true, LucideIconKind.Lock, "Nhập lại mật khẩu", 64, true);
            GanGoiY(_xacNhan, "Xác nhận mật khẩu:",
                "Nhập lại giống hệt mật khẩu đã nhập ở ô phía trên");

            if (AppTheme.YeuCauDongYDieuKhoan)
            {
                _dongY.Font = AppTheme.ChuNho;
                _dongY.ForeColor = AppTheme.ChuPhu;
                _dongY.Text = "Bằng việc ấn tiếp tục, bạn xác nhận đã đọc và đồng ý với chính sách quyền riêng tư của chúng tôi";
                _dongY.LienKet = new[] { "chính sách quyền riêng tư" };
                _dongY.Bounds = new Rectangle(0, y + this.Px(2), w, this.Px(46));
                _dongY.LienKetClick += _ =>
                {
                    using var dlg = new HopThoaiChinhSach();
                    dlg.ShowDialog(FindForm());
                };
                Cot.Controls.Add(_dongY);
                y = _dongY.Bottom + this.Px(6);
            }

            _loiChung.AutoSize = false;
            _loiChung.UseMnemonic = false;
            _loiChung.Font = AppTheme.ChuNhoDam;
            _loiChung.ForeColor = AppTheme.Loi;
            _loiChung.TextAlign = ContentAlignment.MiddleLeft;
            _loiChung.Bounds = new Rectangle(0, y, w, this.Px(24));

            _nut.Text = "Tạo tài khoản";
            _nut.Kieu = KieuNut.Chinh;
            _nut.BanKinhBo = 6;
            _nut.MuiTen = true;
            _nut.Font = AppTheme.ChuNut;
            _nut.Bounds = new Rectangle(0, _loiChung.Bottom + this.Px(6), w, this.Px(46));
            _nut.Click += NutDangKy_Click;

            Cot.Height = _nut.Bottom + this.Px(8);
            Cot.Controls.Add(_loiChung);
            Cot.Controls.Add(_nut);

            CanGiua();
        }

        private void XoaLoi()
        {
            _hoTen.Loi = string.Empty;
            _ten.Loi = string.Empty;
            _email.Loi = string.Empty;
            _matKhau.Loi = string.Empty;
            _xacNhan.Loi = string.Empty;
            _loiChung.Text = string.Empty;
        }

        private FormField? KiemTraDuLieu(
            string hoTen, string ten, string email, string matKhau, string xacNhan)
        {
            Dictionary<string, string> loi;
            if (AppTheme.EmailBatBuoc)
            {
                loi = InputValidator.KiemTraDangKy(hoTen, ten, matKhau, xacNhan, email);
            }
            else
            {
                loi = new Dictionary<string, string>();
                if (hoTen.Length == 0)
                    loi[InputValidator.Truong.HoTen] = "Vui lòng nhập họ và tên.";
                else if (hoTen.Length < 2 || hoTen.Length > 50 || !hoTen.All(c => char.IsLetter(c) || c == ' '))
                    loi[InputValidator.Truong.HoTen] = "Họ tên cần 2-50 ký tự, gồm chữ cái và khoảng trắng.";

                if (ten.Length == 0)
                    loi[InputValidator.Truong.TenDangNhap] = "Vui lòng nhập tên đăng nhập.";
                else if (!RegexTenDangNhap.IsMatch(ten))
                    loi[InputValidator.Truong.TenDangNhap] = "Tên đăng nhập 3-20 ký tự: chữ không dấu, số, dấu _.";

                if (email.Length > 0 && (email.Length > 100 || !RegexEmail.IsMatch(email)))
                    loi[InputValidator.Truong.Email] = "Email phải có dạng ten@mien.com.";

                if (matKhau.Length == 0)
                    loi[InputValidator.Truong.MatKhau] = "Vui lòng nhập mật khẩu.";
                else if (matKhau.Length < 8 || matKhau.Length > 64 || !matKhau.Any(char.IsLetter) || !matKhau.Any(char.IsDigit))
                    loi[InputValidator.Truong.MatKhau] = "Mật khẩu cần 8-64 ký tự, có ít nhất một chữ và một số.";

                if (xacNhan.Length == 0)
                    loi[InputValidator.Truong.XacNhanMatKhau] = "Vui lòng nhập lại mật khẩu.";
                else if (xacNhan != matKhau)
                    loi[InputValidator.Truong.XacNhanMatKhau] = "Hai ô mật khẩu không giống nhau.";
            }

            FormField? dau = null;
            void Gan(string khoa, FormField o)
            {
                if (loi.TryGetValue(khoa, out string? tb))
                {
                    o.Loi = tb;
                    dau ??= o;
                }
            }

            Gan(InputValidator.Truong.HoTen, _hoTen);
            Gan(InputValidator.Truong.TenDangNhap, _ten);
            Gan(InputValidator.Truong.Email, _email);
            Gan(InputValidator.Truong.MatKhau, _matKhau);
            Gan(InputValidator.Truong.XacNhanMatKhau, _xacNhan);
            return dau;
        }

        private async void NutDangKy_Click(object? sender, EventArgs e)
        {
            XoaLoi();

            string hoTen = _hoTen.Text.Trim();
            string ten = _ten.Text.Trim();
            string email = _email.Text.Trim();
            string matKhau = _matKhau.Text;
            string xacNhan = _xacNhan.Text;

            FormField? oLoi = KiemTraDuLieu(hoTen, ten, email, matKhau, xacNhan);
            if (oLoi != null)
            {
                if (oLoi == _xacNhan)
                {
                    string thongBao = _xacNhan.Loi;
                    _matKhau.Clear();
                    _xacNhan.Clear();
                    _xacNhan.Loi = thongBao;
                    oLoi = _matKhau;
                }
                oLoi.Focus();
                return;
            }

            if (AppTheme.YeuCauDongYDieuKhoan && !_dongY.Checked)
            {
                _loiChung.Text = "Vui lòng xác nhận chính sách quyền riêng tư trước khi tạo tài khoản.";
                _dongY.Focus();
                return;
            }

            DatDangXuLy(true);
            try
            {
                KetQuaXacThuc kq = await AppServices.Auth.DangKyAsync(
                    ten, matKhau, hoTen, email.Length == 0 ? null : email);

                switch (kq.Ma)
                {
                    case MaKetQua.ThanhCong:
                        HopThoaiThongBao.HienThanhCong(
                            FindForm(),
                            "Tạo tài khoản thành công",
                            $"Chào mừng {hoTen}! Tài khoản \"{ten}\" đã được tạo và sẵn sàng để đăng nhập.",
                            "Tiếp tục đăng nhập");

                        Shell.HienDangNhap("Tạo tài khoản thành công. Hãy đăng nhập để tiếp tục.", ten);
                        return;

                    case MaKetQua.TenDaTonTai:
                        _ten.Loi = "Tên này đã có người dùng.";
                        _ten.Focus();
                        break;

                    case MaKetQua.LoiKetNoi:
                        _loiChung.Text = "Không kết nối được tới máy chủ. Hãy thử lại.";
                        break;

                    default:
                        _loiChung.Text = kq.ThongBao
                            ?? "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                        break;
                }
            }
            catch (Exception)
            {
                _loiChung.Text = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
            }
            finally
            {
                if (!IsDisposed)
                    DatDangXuLy(false);
            }
        }

        private void DatDangXuLy(bool dangXuLy)
        {
            _nut.DangXuLy = dangXuLy;
            _nut.Enabled = !dangXuLy;
            _nut.Text = dangXuLy ? "Đang tạo tài khoản..." : "Tạo tài khoản";
        }
    }
}
