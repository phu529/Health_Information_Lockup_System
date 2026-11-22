using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using HealthClient.Services;
using HealthClient.UI;

namespace HealthClient
{
    public partial class FrmDangKy : Form
    {
        // Luat kiem tra lay tu bai tap 2.2 / Phu luc B (bang truong du lieu).
        // Khi InputValidator (Shared) cua nhom san sang thi thay phan nay.
        private static readonly Regex RegexTenDangNhap =
            new(@"^[A-Za-z0-9_]{3,20}$");

        private static readonly Regex RegexEmail =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private bool _khoaGoLoi;

        /// <summary>Co gia tri khi dang ky thanh cong, de form Dang nhap dien san.</summary>
        public string? TenVuaDangKy { get; private set; }

        public FrmDangKy()
        {
            InitializeComponent();

            txtHoTen.NoiDungThayDoi += (s, e) => GoLoi(txtHoTen, lblLoiHoTen);
            txtTenDangNhap.NoiDungThayDoi += (s, e) => GoLoi(txtTenDangNhap, lblLoiTenDangNhap);
            txtMatKhau.NoiDungThayDoi += (s, e) => GoLoi(txtMatKhau, lblLoiMatKhau);
            txtXacNhanMatKhau.NoiDungThayDoi += (s, e) => GoLoi(txtXacNhanMatKhau, lblLoiXacNhanMatKhau);
            txtEmail.NoiDungThayDoi += (s, e) => GoLoi(txtEmail, lblLoiEmail);

            // Ghi chu (i) cho tung o: noi dung khop dung luat kiem tra trong KiemTraDuLieu ben duoi
            GanGhiChu(txtHoTen, "Họ và tên nên là:",
                "Từ 2 đến 50 ký tự",
                "Chỉ gồm chữ cái (có dấu tiếng Việt) và khoảng trắng",
                "Ví dụ: Nguyễn Văn A");

            GanGhiChu(txtTenDangNhap, "Tên đăng nhập có thể là:",
                "Từ 3 đến 20 ký tự",
                "Chữ không dấu, chữ số hoặc dấu gạch dưới (_)",
                "Chưa có ai dùng tên này");

            GanGhiChu(txtMatKhau, "Mật khẩu cần có:",
                "Từ 8 đến 64 ký tự",
                "Ít nhất một chữ cái và một chữ số",
                "Nên khác mật khẩu bạn dùng ở nơi khác");

            GanGhiChu(txtXacNhanMatKhau, "Nhập lại mật khẩu:",
                "Gõ lại đúng mật khẩu ở ô phía trên",
                "Giống từng ký tự, phân biệt chữ hoa và chữ thường");

            GanGhiChu(txtEmail, "Email (không bắt buộc):",
                "Có thể để trống",
                "Nếu nhập, cần đúng dạng ten@mien.com",
                "Tối đa 100 ký tự");

            foreach (OnhapLieu o in new[] { txtHoTen, txtTenDangNhap, txtMatKhau, txtXacNhanMatKhau, txtEmail })
                o.NoiDungThayDoi += (s, e) => AnTheGhiChu();

            pnlCuon.Click += (s, e) => AnTheGhiChu();

            TaoDoManhMatKhau();
            TaoNutQuayLai();
        }

        // ----- v34: thanh do manh mat khau + 5 tieu chi (y tuong tu video "Password UI upgrade") -----
        private DoManhMatKhau? _doManh;

        private void TaoDoManhMatKhau()
        {
            // Toa do duoi day la toa do luc THIET KE (form chua duoc co gian theo DPI khi ham nay chay trong constructor).
            // Bo the "CHI MAT MOT PHUT" de form thoang hon: day cac o len 34 diem anh.
            // v40: moi hang so ben duoi la toa do luc THIET KE (o mat khau cao 44). Nhan voi ti le THUC (cao hien tai / 44)
            // de dung ca khi form da duoc co gian theo DPI (150%) lan chua co gian.
            float tl = txtMatKhau.Height / 44f;
            int dayLen = (int)Math.Round(34 * tl);
            huyHieu.Visible = false;

            foreach (Control c in pnlCuon.Controls)
            {
                if (!ReferenceEquals(c, huyHieu))
                    c.Top -= dayLen;
            }

            // Chen thanh do manh ngay duoi o mat khau, day cac thanh phan phia sau (thong bao loi, xac nhan, email) xuong.
            int caoThanh = (int)Math.Round(54 * tl);
            int chen = caoThanh + (int)Math.Round(8 * tl);

            // v35: chot nguong TRUOC vong lap. Ban v34 so sanh voi lblLoiMatKhau.Top ngay trong vong lap, trong khi
            // chinh nhan do bi doi xuong, nen "Nhap lai mat khau" va o ben duoi khong duoc doi va de len thanh do manh.
            int nguong = lblLoiMatKhau.Top;

            foreach (Control c in pnlCuon.Controls)
            {
                if (c.Top >= nguong)
                    c.Top += chen;
            }

            var doManh = new DoManhMatKhau
            {
                Name = "doManhMatKhau",
                Size = new Size(txtMatKhau.Width, caoThanh),
                Location = new Point(txtMatKhau.Left, txtMatKhau.Bottom + (int)Math.Round(6 * tl))
            };

            pnlCuon.Controls.Add(doManh);
            doManh.BringToFront();
            _doManh = doManh;

            // Vien o mat khau doi mau theo do manh (do - cam - xanh), giong video; chua nhap thi ve mau thuong.
            txtMatKhau.NoiDungThayDoi += (s, e) => txtMatKhau.MauVienPhu = doManh.CapNhat(txtMatKhau.Text);
        }

        // ----- v23: nut "Quay lai" ve man hinh chao (form Dang nhap xu ly viec quay ve) -----
        private NutQuayLai? _nutQuayLai;

        /// <summary>Co gia tri (toa do man hinh) khi nguoi dung bam "Quay lai", de form Dang nhap quay ve man hinh chao.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point? DiemQuayVeBia { get; private set; }

        private void TaoNutQuayLai()
        {
            var nut = new NutQuayLai
            {
                Name = "nutQuayLai"
            };

            nut.Bam += (s, diemManHinh) =>
            {
                if (_dangChuyen)
                    return;

                _dangChuyen = true;
                DiemQuayVeBia = diemManHinh;
                Close();
            };

            pnlThuongHieu.Controls.Add(nut);
            nut.GanVaoGoc(pnlThuongHieu);
            nut.BringToFront();
            _nutQuayLai = nut;
        }

        // ----- The ghi chu (i): tao khi can, hien duoi o (hoac tren o neu het cho) -----
        private TheGhiChu? _theGhiChu;
        private OnhapLieu? _oDangMo;

        private int Px(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        private void GanGhiChu(OnhapLieu o, string tieuDe, params string[] dong)
        {
            o.GhiChu = "Gợi ý";
            o.NutGhiChuBam += (s, e) => HienGhiChu(o, tieuDe, dong);
        }

        private void HienGhiChu(OnhapLieu o, string tieuDe, string[] dong)
        {
            if (_theGhiChu == null)
            {
                var the = new TheGhiChu();
                the.Click += (s, ev) => the.Visible = false;
                pnlCuon.Controls.Add(the);
                _theGhiChu = the;
            }

            if (_theGhiChu.Visible && ReferenceEquals(_oDangMo, o))
            {
                _theGhiChu.Visible = false;
                return;
            }

            _theGhiChu.DatNoiDung(tieuDe, dong);
            int cao = _theGhiChu.TinhChieuCao();
            bool xuongDuoi = o.Bottom + Px(2) + cao <= pnlCuon.ClientSize.Height;

            _theGhiChu.MuiTenOPhiaDuoi = !xuongDuoi;
            _theGhiChu.Size = new Size(o.Width, cao);
            _theGhiChu.Location = new Point(o.Left, xuongDuoi ? o.Bottom + Px(2) : o.Top - cao - Px(2));
            _theGhiChu.MuiTenX = o.TamNutGhiChuX();
            _theGhiChu.BringToFront();
            _theGhiChu.Visible = true;
            _oDangMo = o;
        }

        private void AnTheGhiChu()
        {
            if (_theGhiChu != null && _theGhiChu.Visible)
                _theGhiChu.Visible = false;
        }

        // Do form Dang nhap truyen sang: mo dung cho cua so cu va anh chup form Dang nhap de lam hieu ung chuyen trang.
        // Chi dung luc chay chuong trinh, khong thuoc ve giao dien thiet ke (Hidden).
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point? ViTriMo { get; set; }

        /// <summary>Anh chup form Dang nhap luc bam "Dang ky". Form Dang nhap so huu va tu giai phong anh nay.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap? AnhFormDangNhap { get; set; }

        private bool _dangChuyen;
        private LopChuyenTrang? _lopTrang;
        private Bitmap? _anhTam;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ManHinh.VuaVoiManHinh(this, ViTriMo);

            // Co anh form Dang nhap thi giu form trong suot cho den khi lop chuyen trang san sang (khong nhay khung).
            if (AnhFormDangNhap != null && HoatAnh.DuocPhep)
                Opacity = 0;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ManHinh.VuaVoiManHinh(this, ViTriMo);
            txtHoTen.DatConTro();
            ChayChuyenVao();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // Go lop chuyen trang truoc khi giai phong anh de khong con ve bang anh da huy.
            if (_lopTrang != null)
                GoLopTrang(_lopTrang);

            _anhTam?.Dispose();
            _anhTam = null;
        }

        // ----- v25: chuyen trang Dang nhap <-> Dang ky -----

        // Ve cac control cua form (tru lop chuyen trang) len mot anh, tu duoi len tren theo thu tu chong lop.
        private Bitmap? ChupNoiDungForm()
        {
            Size kt = ClientSize;

            if (kt.Width < 100 || kt.Height < 100)
                return null;

            var anh = new Bitmap(kt.Width, kt.Height, PixelFormat.Format32bppPArgb);

            try
            {
                using (Graphics g = Graphics.FromImage(anh))
                    g.Clear(BackColor);

                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    Control c = Controls[i];

                    if (ReferenceEquals(c, _lopTrang) || !c.Visible || c.Width <= 0 || c.Height <= 0)
                        continue;

                    c.DrawToBitmap(anh, c.Bounds);
                }

                return anh;
            }
            catch (Exception)
            {
                anh.Dispose();
                return null;
            }
        }

        private LopChuyenTrang? TaoLopChuyenTrang(Bitmap tu, Bitmap den, int huong)
        {
            try
            {
                Color nen = pnlNoiDung.BackColor.A == 255 ? pnlNoiDung.BackColor : Theme.NenTrang;

                var lop = new LopChuyenTrang(tu, den, pnlNoiDung.Bounds, nen, huong)
                {
                    Name = "lopChuyenTrang",
                    Bounds = ClientRectangle
                };

                Controls.Add(lop);
                lop.BringToFront();
                lop.CreateControl();
                lop.Refresh();
                _lopTrang = lop;
                return lop;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void GoLopTrang(LopChuyenTrang lop)
        {
            if (ReferenceEquals(_lopTrang, lop))
                _lopTrang = null;

            Controls.Remove(lop);
            lop.Dispose();
        }

        // Vao: lop phu kin hien dung anh form Dang nhap (khung dau giong het man hinh cu), roi noi dung Dang ky truot vao.
        private void ChayChuyenVao()
        {
            Bitmap? tu = AnhFormDangNhap;

            if (tu == null || !HoatAnh.DuocPhep)
            {
                Opacity = 1;
                return;
            }

            Bitmap? den = ChupNoiDungForm();
            LopChuyenTrang? lop = den == null ? null : TaoLopChuyenTrang(tu, den, 1);

            if (den == null || lop == null)
            {
                den?.Dispose();
                Opacity = 1;
                return;
            }

            LopChuyenTrang lopCC = lop;

            _anhTam = den;
            _dangChuyen = true;
            Opacity = 1;

            HoatAnh.Chay(
                this, 780,
                t => lopCC.DatTienDo(t),
                () =>
                {
                    GoLopTrang(lopCC);
                    _anhTam?.Dispose();
                    _anhTam = null;
                    _dangChuyen = false;
                    txtHoTen.DatConTro();
                },
                HoatAnh.VaoRaMem);
        }

        // Ra: noi dung Dang ky truot ve, noi dung Dang nhap hien lai; xong thi dong form (form Dang nhap nam ngay duoi).
        private void ChayChuyenRa()
        {
            // v30: dung co rieng cho luc thoat; neu hieu ung "vao" con dang chay thi dung no lai chu khong chan nut Huy.
            if (_dangThoat)
                return;

            _dangThoat = true;
            _dangChuyen = false;

            Bitmap? den = AnhFormDangNhap;
            string lyDo = den == null ? "khong co anh form dang nhap" : !HoatAnh.DuocPhep ? "Windows tat hieu ung" : "";

            if (den == null || !HoatAnh.DuocPhep)
            {
                GhiNhatKyChuyenTrang("thoat: " + lyDo);
                MoCuaSoMo();
                return;
            }

            AnTheGhiChu();

            Bitmap? tu = ChupNoiDungForm();
            LopChuyenTrang? lop = tu == null ? null : TaoLopChuyenTrang(tu, den, -1);

            if (tu == null || lop == null)
            {
                GhiNhatKyChuyenTrang("thoat: khong tao duoc lop chuyen trang");
                tu?.Dispose();
                MoCuaSoMo();
                return;
            }

            GhiNhatKyChuyenTrang("thoat: chay hieu ung");

            LopChuyenTrang lopCC = lop;

            _anhTam = tu;
            _dangChuyen = true;

            HoatAnh.Chay(
                this, 700,
                t => lopCC.DatTienDo(t),
                () => Close(),
                HoatAnh.VaoRaMem);
        }

        private bool _dangThoat;

        // Du phong: khong chay duoc hieu ung "quet" thi mo dan form de van co chuyen canh, khong dong dot ngot.
        private void MoCuaSoMo()
        {
            if (!HoatAnh.DuocPhep)
            {
                Close();
                return;
            }

            HoatAnh.Chay(this, 260, t => Opacity = 1.0 - t, () => Close());
        }

        // Ghi mot dong chan doan vao %LocalAppData%\HealthClient\chuyen-trang.log de biet vi sao hieu ung khong chay.
        private static void GhiNhatKyChuyenTrang(string noiDung)
        {
            try
            {
                string thuMuc = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HealthClient");
                Directory.CreateDirectory(thuMuc);
                File.AppendAllText(
                    Path.Combine(thuMuc, "chuyen-trang.log"),
                    DateTime.Now.ToString("HH:mm:ss") + " " + noiDung + Environment.NewLine);
            }
            catch (Exception)
            {
                // Chan doan khong duoc lam hong chuc nang chinh.
            }
        }

        // ----- Trang thai man hinh -----

        private void GoLoi(OnhapLieu o, Label nhan)
        {
            if (_khoaGoLoi)
                return;

            nhan.Text = string.Empty;
            o.CoLoi = false;
        }

        private void XoaONhap(OnhapLieu o)
        {
            _khoaGoLoi = true;
            o.Clear();
            _khoaGoLoi = false;
        }

        private void XoaLoi()
        {
            lblLoiHoTen.Text = string.Empty;
            lblLoiTenDangNhap.Text = string.Empty;
            lblLoiMatKhau.Text = string.Empty;
            lblLoiXacNhanMatKhau.Text = string.Empty;
            lblLoiEmail.Text = string.Empty;
            lblLoiChung.Text = string.Empty;

            txtHoTen.CoLoi = false;
            txtTenDangNhap.CoLoi = false;
            txtMatKhau.CoLoi = false;
            txtXacNhanMatKhau.CoLoi = false;
            txtEmail.CoLoi = false;
        }

        private void DatDangXuLy(bool dangXuLy)
        {
            btnDangKy.Enabled = !dangXuLy;
            btnDangKy.Text = dangXuLy ? "Đang gửi..." : "Đăng ký";
            thanhTab.Enabled = !dangXuLy;

            if (_nutQuayLai != null)
                _nutQuayLai.Enabled = !dangXuLy;

            UseWaitCursor = dangXuLy;
        }

        // ----- Kiem tra du lieu: tra ve o dau tien (tu tren xuong) bi loi, null neu hop le -----

        private OnhapLieu? KiemTraDuLieu(
            string hoTen, string ten, string matKhau, string xacNhan, string email)
        {
            OnhapLieu? loiDauTien = null;

            void DatLoi(Label nhan, OnhapLieu o, string noiDung)
            {
                nhan.Text = noiDung;
                o.CoLoi = true;
                loiDauTien ??= o;
            }

            if (hoTen.Length == 0)
                DatLoi(lblLoiHoTen, txtHoTen,
                    "Vui lòng nhập họ và tên.");
            else if (hoTen.Length < 2 || hoTen.Length > 50 ||
                     !hoTen.All(c => char.IsLetter(c) || c == ' '))
                DatLoi(lblLoiHoTen, txtHoTen,
                    "Họ tên cần 2-50 ký tự, gồm chữ cái và khoảng trắng.");

            if (ten.Length == 0)
                DatLoi(lblLoiTenDangNhap, txtTenDangNhap,
                    "Vui lòng nhập tên đăng nhập.");
            else if (!RegexTenDangNhap.IsMatch(ten))
                DatLoi(lblLoiTenDangNhap, txtTenDangNhap,
                    "Tên đăng nhập 3-20 ký tự: chữ không dấu, số, dấu _.");

            if (matKhau.Length == 0)
                DatLoi(lblLoiMatKhau, txtMatKhau,
                    "Vui lòng nhập mật khẩu.");
            else if (matKhau.Length < 8 || matKhau.Length > 64 ||
                     !matKhau.Any(char.IsLetter) ||
                     !matKhau.Any(char.IsDigit))
                DatLoi(lblLoiMatKhau, txtMatKhau,
                    "Mật khẩu cần 8-64 ký tự, có ít nhất một chữ và một số.");

            if (xacNhan.Length == 0)
                DatLoi(lblLoiXacNhanMatKhau, txtXacNhanMatKhau,
                    "Vui lòng nhập lại mật khẩu.");
            else if (xacNhan != matKhau)
                DatLoi(lblLoiXacNhanMatKhau, txtXacNhanMatKhau,
                    "Hai ô mật khẩu không giống nhau.");

            // Email khong bat buoc.
            if (email.Length > 0 &&
                (email.Length > 100 || !RegexEmail.IsMatch(email)))
                DatLoi(lblLoiEmail, txtEmail,
                    "Email phải có dạng ten@mien.com.");

            return loiDauTien;
        }

        // ----- Su kien -----

        private async void btnDangKy_Click(object? sender, EventArgs e)
        {
            if (_dangChuyen)
                return;

            XoaLoi();

            string hoTen = txtHoTen.Text.Trim();
            string ten = txtTenDangNhap.Text.Trim();
            string matKhau = txtMatKhau.Text;
            string xacNhan = txtXacNhanMatKhau.Text;
            string email = txtEmail.Text.Trim();

            OnhapLieu? oLoi = KiemTraDuLieu(hoTen, ten, matKhau, xacNhan, email);

            if (oLoi != null)
            {
                // Hai o mat khau khong khop: xoa ca hai (giu nguyen cac o con lai).
                if (oLoi == txtXacNhanMatKhau)
                {
                    XoaONhap(txtMatKhau);
                    XoaONhap(txtXacNhanMatKhau);
                    oLoi = txtMatKhau;
                }

                oLoi.DatConTro();
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
                        TenVuaDangKy = ten;

                        MessageBox.Show(
                            "Đăng ký tài khoản thành công!",
                            "Thông báo",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        // v29: tro ve form Dang nhap bang hieu ung chuyen trang thay vi dong dot ngot.
                        ChayChuyenRa();
                        break;

                    case MaKetQua.TenDaTonTai:
                        lblLoiTenDangNhap.Text = "Tên này đã có người dùng.";
                        txtTenDangNhap.CoLoi = true;
                        txtTenDangNhap.DatConTro();
                        break;

                    case MaKetQua.LoiKetNoi:
                        lblLoiChung.Text = "Không kết nối được tới máy chủ. Hãy thử lại.";
                        break;

                    default:
                        lblLoiChung.Text = kq.ThongBao
                            ?? "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                        break;
                }
            }
            catch (Exception)
            {
                lblLoiChung.Text = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
            }
            finally
            {
                // Dang chay hieu ung quay ve Dang nhap thi giu nut khoa den khi form dong (tranh bam hai lan).
                if (!_dangChuyen)
                    DatDangXuLy(false);
            }
        }

        // Tab chi con mot muc "Dang ky" nen khong con ai bam vao day; giu ham de Designer van dang ky duoc su kien.
        private void thanhTab_TabBam(object? sender, int chiSo)
        {
            if (chiSo == 0)
                ChayChuyenRa();
        }

        private void btnHuy_Click(object? sender, EventArgs e)
        {
            ChayChuyenRa();
        }
    }
}
