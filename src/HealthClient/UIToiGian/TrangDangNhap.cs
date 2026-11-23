using System.Drawing;
using System.Windows.Forms;
using HealthClient.Services;

namespace HealthClient.UIToiGian
{
    // Trang Dang nhap Ban 9:
    //  - Tinh toa do dong theo Bottom + gap de KHONG BAO GIO bi che khuat net chu o bat ky DPI nao.
    internal sealed class TrangDangNhap : TrangXacThuc
    {
        private readonly FormField _ten = new();
        private readonly FormField _matKhau = new();
        private readonly OTich _nho = new();
        private readonly FlatButton _quen = new();
        private readonly Label _loiChung = new();
        private readonly FlatButton _nut = new();

        public override IButtonControl? NutMacDinh => _nut;
        public override FormField? OBatDau => _ten.Text.Length == 0 ? _ten : _matKhau;

        public TrangDangNhap(FrmVo shell, string? nhacNho, string? tenGoiY, int tabTruoc = -1)
            : base(shell, 0, tabTruoc, 420)
        {
            Trai.DatCheDo(
                KieuMinhHoa.DangNhap,
                new[]
                {
                    (LucideIconKind.BookOpen, AppTheme.Chinh, AppTheme.ChipXanh, "Thông tin dễ hiểu"),
                    (LucideIconKind.ListChecks, AppTheme.Chinh, AppTheme.ChipXanh, "Nội dung chọn lọc"),
                    (LucideIconKind.Search, AppTheme.Chinh, AppTheme.ChipXanh, "Tra cứu thuận tiện")
                });

            int w = Cot.Width;

            var lblTd = TaoTieuDe("Chào mừng trở lại", this.Px(54));
            var lblMt = TaoMoTa(nhacNho ?? "Đăng nhập để tiếp tục hành trình chăm sóc sức khỏe của bạn.", lblTd.Bottom + this.Px(2), 38);
            Cot.Controls.Add(lblTd);
            Cot.Controls.Add(lblMt);

            _ten.Nhan = "Tên đăng nhập";
            _ten.BatBuoc = true;
            _ten.IconTrai = LucideIconKind.User;
            _ten.HienNutXoa = true;
            _ten.PlaceholderText = "Nhập tên đăng nhập";
            _ten.MaxLength = 20;
            _ten.Location = new Point(0, lblMt.Bottom + this.Px(8));
            _ten.Width = w;
            GanGoiY(_ten, "Tên đăng nhập hợp lệ gồm:",
                "Từ 3 đến 20 ký tự",
                "Chữ cái không dấu, chữ số hoặc dấu gạch dưới (_)",
                "Là tên bạn đã đặt khi đăng ký tài khoản");

            _matKhau.Nhan = "Mật khẩu";
            _matKhau.BatBuoc = true;
            _matKhau.IconTrai = LucideIconKind.Lock;
            _matKhau.LaMatKhau = true;
            _matKhau.PlaceholderText = "Nhập mật khẩu";
            _matKhau.MaxLength = 64;
            _matKhau.Location = new Point(0, _ten.Bottom + this.Px(4));
            _matKhau.Width = w;
            GanGoiY(_matKhau, "Quy tắc mật khẩu:",
                "Độ dài từ 8 đến 64 ký tự",
                "Có ít nhất một chữ cái và một chữ số",
                "Nhấn biểu tượng con mắt để xem hoặc ẩn mật khẩu");

            int yHangNho = _matKhau.Bottom + this.Px(4);
            _nho.Text = "Nhớ mật khẩu";
            _nho.Font = AppTheme.ChuDam;
            _nho.ForeColor = AppTheme.ChuPhu;
            _nho.Bounds = new Rectangle(0, yHangNho, this.Px(180), this.Px(28));

            _quen.Kieu = KieuNut.Chu;
            _quen.CanPhai = true;
            _quen.Text = "Quên mật khẩu?";
            _quen.Font = AppTheme.ChuDam;
            _quen.Bounds = new Rectangle(w - this.Px(181), yHangNho, this.Px(180), this.Px(28));
            _quen.Click += (_, _) => MoQuenMatKhau();

            _loiChung.AutoSize = false;
            _loiChung.UseMnemonic = false;
            _loiChung.Font = AppTheme.ChuNhoDam;
            _loiChung.ForeColor = AppTheme.Loi;
            _loiChung.TextAlign = ContentAlignment.MiddleLeft;
            _loiChung.Bounds = new Rectangle(0, _nho.Bottom + this.Px(4), w, this.Px(24));

            _nut.Text = "Đăng nhập";
            _nut.Kieu = KieuNut.Chinh;
            _nut.BanKinhBo = 6;
            _nut.MuiTen = true;
            _nut.Font = AppTheme.ChuNut;
            _nut.Bounds = new Rectangle(0, _loiChung.Bottom + this.Px(4), w, this.Px(46));
            _nut.Click += NutDangNhap_Click;

            Cot.Controls.AddRange(new Control[]
            {
                _ten, _matKhau, _nho, _quen, _loiChung, _nut
            });
            int yLink = _nut.Bottom + this.Px(12);
            ThemDongLienKet("Chưa có tài khoản?", "Bấm vào đây để đăng ký", yLink, () => Shell.HienDangKy());

            Cot.Height = yLink + this.Px(34);

            string? daNho = CauHinhCucBo.DocTenDaNho();
            if (!string.IsNullOrEmpty(tenGoiY))
            {
                _ten.Text = tenGoiY;
            }
            else if (!string.IsNullOrEmpty(daNho))
            {
                _ten.Text = daNho;
                _nho.Checked = true;

                string? matKhauDaNho = CauHinhCucBo.DocMatKhauDaNho();
                if (!string.IsNullOrEmpty(matKhauDaNho))
                    _matKhau.Text = matKhauDaNho;
            }

            CanGiua();
        }

        private async void NutDangNhap_Click(object? sender, EventArgs e)
        {
            _ten.Loi = string.Empty;
            _matKhau.Loi = string.Empty;
            _loiChung.Text = string.Empty;

            string ten = _ten.Text.Trim();
            string matKhau = _matKhau.Text;

            bool thieu = false;
            if (ten.Length == 0)
            {
                _ten.Loi = "Vui lòng nhập tên đăng nhập.";
                thieu = true;
            }
            if (matKhau.Length == 0)
            {
                _matKhau.Loi = "Vui lòng nhập mật khẩu.";
                thieu = true;
            }
            if (thieu)
            {
                if (ten.Length == 0) _ten.Focus(); else _matKhau.Focus();
                return;
            }

            DatDangXuLy(true);
            try
            {
                KetQuaXacThuc kq = await AppServices.Auth.DangNhapAsync(ten, matKhau);

                switch (kq.Ma)
                {
                    case MaKetQua.ThanhCong:
                        CauHinhCucBo.LuuTenDaNho(_nho.Checked ? ten : null);
                        CauHinhCucBo.LuuMatKhauDaNho(_nho.Checked ? matKhau : null);
                        Shell.MoTrangChu(kq.HoTen ?? ten);
                        return;

                    case MaKetQua.SaiThongTin:
                        _matKhau.Clear();
                        _matKhau.Loi = "Tên đăng nhập hoặc mật khẩu không đúng.";
                        _matKhau.Focus();
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
            _nut.Text = dangXuLy ? "Đang đăng nhập..." : "Đăng nhập";
            _quen.Enabled = !dangXuLy;
        }

        private void MoQuenMatKhau()
        {
            if (!AppTheme.BatHopThoaiQuenMatKhau)
            {
                _loiChung.Text = "Chức năng khôi phục mật khẩu đang được hoàn thiện.";
                return;
            }

            Form? chu = FindForm();
            using var nenToi = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Bounds = chu?.Bounds ?? Screen.FromControl(this).Bounds,
                BackColor = Color.Black,
                Opacity = 0.35
            };
            if (chu != null) nenToi.Show(chu);

            try
            {
                using var dlg = new HopThoaiQuenMatKhau(_ten.Text.Trim());
                if (dlg.ShowDialog(nenToi) == DialogResult.OK && !string.IsNullOrEmpty(dlg.TenDaDatLai))
                {
                    _ten.Text = dlg.TenDaDatLai;
                    _matKhau.Clear();
                    _matKhau.Focus();
                }
            }
            finally
            {
                nenToi.Close();
                chu?.Activate();
            }
        }
    }
}
