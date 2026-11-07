using System.Drawing.Imaging;
using HealthClient.Services;
using HealthClient.UI;

namespace HealthClient
{
    public partial class FrmDangNhap : Form
    {
        // Khi true thi viec xoa chu trong o nhap khong tu go nhan loi.
        private bool _khoaGoLoi;

        public FrmDangNhap()
        {
            InitializeComponent();

            // v30: bieu tuong tren hai nut dang nhap nhanh (chi la hinh, khong doi chuc nang).
            btnGoogle.KyHieu = "G";
            btnVssId.KyHieu = "V";

            // v20: giam nhap nhay khi bia chao truot len.
            DoubleBuffered = true;
            TaoLopBia();
            TaoNutQuayLai();

            // Nguoi dung bat dau go lai thi go dau loi cua o do.
            txtTenDangNhap.NoiDungThayDoi += (s, e) => GoLoi(txtTenDangNhap, lblLoiTenDangNhap);
            txtMatKhau.NoiDungThayDoi += (s, e) => GoLoi(txtMatKhau, lblLoiMatKhau);

            // Go chu, doi sang o mat khau hoac bam vao cho trong thi the ghi chu tu dong dong lai.
            txtTenDangNhap.NoiDungThayDoi += (s, e) => AnTheGhiChu();
            txtMatKhau.NoiDungThayDoi += (s, e) => AnTheGhiChu();
            pnlCuon.Click += (s, e) => AnTheGhiChu();

            // v25: man hinh thap thi tu thu gon de hang "Nho mat khau / Quen mat khau?" khong bi cat.
            pnlCuon.SizeChanged += (s, e) => CapNhatGon();

            // "Nho mat khau" (v24): nho ten dang nhap + mat khau. Mat khau duoc ma hoa bang Windows DPAPI
            // (chi dung duoc boi dung tai khoan Windows tren dung may nay), khong luu dang chu thuong.
            string? daNho = CauHinhCucBo.DocTenDaNho();

            if (!string.IsNullOrEmpty(daNho))
            {
                txtTenDangNhap.Text = daNho;
                chkNhoTaiKhoan.Checked = true;

                string? matKhauDaNho = CauHinhCucBo.DocMatKhauDaNho();

                if (!string.IsNullOrEmpty(matKhauDaNho))
                    txtMatKhau.Text = matKhauDaNho;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ManHinh.VuaVoiManHinh(this);
            DatLaiKhungBia();
            CapNhatGon();
        }

        // v25: che do gon. Khi vung chua cac o nhap qua thap (cua so bi thu theo man hinh), an nhan "CHAO MUNG"
        // va don toan bo noi dung len, de hang "Nho mat khau / Quen mat khau?" hien du, khong phai cuon.
        private bool _gon;

        private void CapNhatGon()
        {
            if (pnlCuon.Height <= 0)
                return;

            int dich = huyHieu.Height + (int)Math.Round(6 * DeviceDpi / 96.0);
            int daoCuon = pnlCuon.AutoScrollPosition.Y;
            int daySauCung = chkNhoTaiKhoan.Bottom - daoCuon + (int)Math.Round(4 * DeviceDpi / 96.0);

            bool canGon = !_gon && daySauCung > pnlCuon.Height;
            bool canMo = _gon && daySauCung + dich <= pnlCuon.Height;

            if (!canGon && !canMo)
                return;

            int huong = canGon ? -1 : 1;

            pnlCuon.SuspendLayout();

            foreach (Control c in pnlCuon.Controls)
            {
                if (!ReferenceEquals(c, huyHieu))
                    c.Top += huong * dich;
            }

            huyHieu.Visible = !canGon;
            _gon = canGon;
            pnlCuon.ResumeLayout(true);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ManHinh.VuaVoiManHinh(this);
            DatLaiKhungBia();

            // Bia chao con che thi chua dua con tro vao o nhap; mo bia xong moi dat (xem MoLopBia).
            if (_bia == null)
                DatConTroBanDau();
        }

        private void DatConTroBanDau()
        {
            if (txtTenDangNhap.Text.Length > 0)
                txtMatKhau.DatConTro();
            else
                txtTenDangNhap.DatConTro();
        }

        // ----- v20: bia chao truoc form Dang nhap / Dang ky -----

        private LopBiaChao? _bia;
        private bool _dangMoBia;

        private LopBiaChao TaoDoiTuongBia()
        {
            var bia = new LopBiaChao
            {
                Name = "lopBiaChao",
                Bounds = ClientRectangle
            };

            bia.YeuCauMo += (s, diem) => MoLopBia(diem);
            return bia;
        }

        private void TaoLopBia()
        {
            _bia = TaoDoiTuongBia();
            Controls.Add(_bia);
            _bia.BringToFront();

            // Bia luon phu kin cua so (tru luc dang truot), ke ca khi cua so doi kich thuoc / DPI.
            Resize += (s, e) => DatLaiKhungBia();
        }

        private void DatLaiKhungBia()
        {
            if (_bia != null && !_dangMoBia)
                _bia.Bounds = ClientRectangle;
        }

        // ----- v23: nut "Quay lai" ve man hinh chao -----

        private NutQuayLai? _nutQuayLai;

        // Nut nam o goc tren trai bang thuong hieu (con cua pnlThuongHieu de nen kinh trong suot lay dung hoa tiet).
        // Toa do / kich thuoc o don vi thiet ke 96 dpi; form tu phong theo DPI cung cac control khac.
        private void TaoNutQuayLai()
        {
            var nut = new NutQuayLai
            {
                Name = "nutQuayLai"
            };

            nut.Bam += (s, diemManHinh) => QuayVeBia(PointToClient(diemManHinh));
            pnlThuongHieu.Controls.Add(nut);
            nut.GanVaoGoc(pnlThuongHieu);
            nut.BringToFront();
            _nutQuayLai = nut;
        }

        // Quay lai man hinh chao: lam nguoc hieu ung mo. Vong tron form thu nho dan ve diem bam, bia chao lo lai.
        // Xoa mat khau va thong bao loi dang hien; giu ten dang nhap. Khong dung toi logic dang nhap.
        private void QuayVeBia(Point tam)
        {
            if (_bia != null || _dangMoBia || UseWaitCursor)
                return;

            AnTheGhiChu();
            XoaLoi();
            NapLaiThongTinDaNho();

            _dangMoBia = true;

            LopBiaChao bia = TaoDoiTuongBia();
            Controls.Add(bia);
            bia.BringToFront();
            bia.CreateControl();
            _bia = bia;

            // Khung dau tien phai la form nguyen ven (tien do 1), khong duoc nhay qua bia.
            LopChuyenCanh? lop = HoatAnh.DuocPhep ? TaoLopChuyenCanh(bia, tam, 1f) : null;

            if (lop == null)
            {
                // Khong co hieu ung (Windows tat, hoac chup anh loi): bia hien ngay.
                _dangMoBia = false;
                return;
            }

            LopChuyenCanh lopCC = lop;

            // Lop chuyen canh da che kin, tam an bia that de no khong ve ngam trong luc chay.
            bia.Visible = false;

            HoatAnh.Chay(
                this, 620,
                p => lopCC.DatTienDo(1f - p),
                () => KetThucQuayVe(bia, lopCC));
        }

        private void KetThucQuayVe(LopBiaChao bia, LopChuyenCanh lop)
        {
            // Ve bia that xong roi moi go lop chuyen canh: khung cuoi hai ben giong nhau nen khong nhay.
            bia.Visible = true;
            bia.BringToFront();
            bia.Refresh();

            Controls.Remove(lop);
            lop.Dispose();
            _dangMoBia = false;
        }

        // v22: bam chuot TRAI tai diem "tam" thi mot vong tron no dan tu diem do, lo form Dang nhap (0,7 giay, vao nhanh ra em).
        // Neu khong chup duoc anh (hiem), hoac Windows tat hieu ung: truot bia len nhu v20 / lo ngay.
        private void MoLopBia(Point tam)
        {
            LopBiaChao? bia = _bia;

            if (bia == null || _dangMoBia)
                return;

            _dangMoBia = true;

            LopChuyenCanh? lop = HoatAnh.DuocPhep ? TaoLopChuyenCanh(bia, tam) : null;

            if (lop == null)
            {
                int cao = bia.Height;

                HoatAnh.Chay(
                    this, 560,
                    t => bia.Top = -(int)(cao * t),
                    () => DongBia(bia, null));

                return;
            }

            LopChuyenCanh lopCC = lop;

            HoatAnh.Chay(
                this, 700,
                t =>
                {
                    // Bia that an di o nhip dau tien (lop chuyen canh da che kin), de ngung ve tiep.
                    if (bia.Visible)
                        bia.Visible = false;

                    lopCC.DatTienDo(t);
                },
                () => DongBia(bia, lopCC),
                HoatAnh.RaEm);
        }

        private void DongBia(LopBiaChao bia, LopChuyenCanh? lop)
        {
            if (lop != null)
            {
                Controls.Remove(lop);
                lop.Dispose();
            }

            Controls.Remove(bia);
            bia.Dispose();
            _bia = null;
            _dangMoBia = false;
            DatConTroBanDau();
        }

        // Chup bia chao va noi dung form (khong co bia) thanh hai anh, roi dat lop chuyen canh len tren cung.
        private LopChuyenCanh? TaoLopChuyenCanh(LopBiaChao bia, Point tam, float tienDoDau = 0f)
        {
            Bitmap? anhBia = null;
            Bitmap? anhForm = null;

            try
            {
                Size kt = ClientSize;

                if (kt.Width < 100 || kt.Height < 100)
                    return null;

                anhBia = new Bitmap(kt.Width, kt.Height, PixelFormat.Format32bppPArgb);
                bia.DrawToBitmap(anhBia, new Rectangle(Point.Empty, kt));

                anhForm = ChupNoiDungForm(kt);

                if (anhForm == null)
                {
                    anhBia.Dispose();
                    return null;
                }

                var lop = new LopChuyenCanh(anhBia, anhForm, tam)
                {
                    Name = "lopChuyenCanh",
                    Bounds = ClientRectangle
                };

                lop.DatTienDoKhongVe(tienDoDau);

                Controls.Add(lop);
                lop.BringToFront();
                lop.CreateControl();
                lop.Refresh();
                return lop;
            }
            catch (Exception)
            {
                anhBia?.Dispose();
                anhForm?.Dispose();
                return null;
            }
        }

        // Ve cac control cua form (tru bia) len mot anh, tu duoi len tren theo thu tu chong lop.
        private Bitmap? ChupNoiDungForm(Size kt)
        {
            var anh = new Bitmap(kt.Width, kt.Height, PixelFormat.Format32bppPArgb);

            try
            {
                using (Graphics g = Graphics.FromImage(anh))
                    g.Clear(BackColor);

                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    Control c = Controls[i];

                    if (c == _bia || !c.Visible || c.Width <= 0 || c.Height <= 0)
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

        // Esc = thoat (chuc nang cua nut Thoat cu trong BT 2.2).
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            // v23: Alt + mui ten trai = Quay lai man hinh chao (giong trinh duyet), nhu bam nut "Quay lai".
            if (keyData == (Keys.Alt | Keys.Left) && _bia == null && !_dangMoBia &&
                _nutQuayLai != null && _nutQuayLai.Enabled)
            {
                Point giua = _nutQuayLai.PointToScreen(
                    new Point(_nutQuayLai.Width / 2, _nutQuayLai.Height / 2));

                QuayVeBia(PointToClient(giua));
                return true;
            }

            // v21: bia chao dang che thi MOI phim deu bi nuot va KHONG mo bia (chi bam chuot moi mo).
            // Nuot ca Enter / Tab / chu cai de khong bam Dang nhap ngam, khong go chu vao o nhap dang bi che.
            if (_bia != null)
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ----- Trang thai man hinh: Cho nhap / Dang xu ly / Co loi / Thanh cong -----

        private void DatLoi(OnhapLieu o, Label nhan, string noiDung)
        {
            nhan.Text = noiDung;
            o.CoLoi = true;
        }

        private void GoLoi(OnhapLieu o, Label nhan)
        {
            if (_khoaGoLoi)
                return;

            nhan.Text = string.Empty;
            o.CoLoi = false;
        }

        // v27: dang tick "Nho mat khau" thi quay ve bia / dang xuat roi vao lai van thay mat khau da nho
        // (doc tu file DPAPI). Chua tick, hoac chua tung dang nhap thanh cong (chua co gi de nho) thi xoa o mat khau.
        private void NapLaiThongTinDaNho()
        {
            string? mk = chkNhoTaiKhoan.Checked ? CauHinhCucBo.DocMatKhauDaNho() : null;

            if (!string.IsNullOrEmpty(mk))
            {
                _khoaGoLoi = true;
                txtMatKhau.Text = mk;
                _khoaGoLoi = false;
            }
            else
            {
                XoaONhap(txtMatKhau);
            }
        }

        private void XoaONhap(OnhapLieu o)
        {
            _khoaGoLoi = true;
            o.Clear();
            _khoaGoLoi = false;
        }

        private void XoaLoi()
        {
            lblLoiTenDangNhap.Text = string.Empty;
            lblLoiMatKhau.Text = string.Empty;
            lblLoiChung.Text = string.Empty;
            txtTenDangNhap.CoLoi = false;
            txtMatKhau.CoLoi = false;
        }

        private void DatDangXuLy(bool dangXuLy)
        {
            btnDangNhap.Enabled = !dangXuLy;
            btnDangNhap.Text = dangXuLy ? "Đang gửi..." : "Đăng nhập";
            lnkChuyenDangKy.Enabled = !dangXuLy;
            btnVssId.Enabled = !dangXuLy;
            btnGoogle.Enabled = !dangXuLy;
            thanhTab.Enabled = !dangXuLy;
            lnkQuenMatKhau.Enabled = !dangXuLy;

            if (_nutQuayLai != null)
                _nutQuayLai.Enabled = !dangXuLy;

            UseWaitCursor = dangXuLy;
        }

        // ----- Su kien -----

        private async void btnDangNhap_Click(object? sender, EventArgs e)
        {
            XoaLoi();

            string ten = txtTenDangNhap.Text.Trim();
            string matKhau = txtMatKhau.Text;

            // Kiem tra tai cho truoc khi gui di (luat that nam o server).
            bool thieu = false;

            if (ten.Length == 0)
            {
                DatLoi(txtTenDangNhap, lblLoiTenDangNhap, "Vui lòng nhập tên đăng nhập.");
                thieu = true;
            }

            if (matKhau.Length == 0)
            {
                DatLoi(txtMatKhau, lblLoiMatKhau, "Vui lòng nhập mật khẩu.");
                thieu = true;
            }

            if (thieu)
            {
                if (ten.Length == 0)
                    txtTenDangNhap.DatConTro();
                else
                    txtMatKhau.DatConTro();

                return;
            }

            DatDangXuLy(true);

            try
            {
                KetQuaXacThuc kq =
                    await AppServices.Auth.DangNhapAsync(ten, matKhau);

                switch (kq.Ma)
                {
                    case MaKetQua.ThanhCong:
                        CauHinhCucBo.LuuTenDaNho(chkNhoTaiKhoan.Checked ? ten : null);
                        CauHinhCucBo.LuuMatKhauDaNho(chkNhoTaiKhoan.Checked ? matKhau : null);
                        MoTrangChu(kq.HoTen ?? ten);
                        break;

                    case MaKetQua.SaiThongTin:
                        // Cung mot thong bao cho "khong ton tai" va "sai mat khau".
                        XoaONhap(txtMatKhau);
                        DatLoi(txtMatKhau, lblLoiMatKhau,
                            "Tên đăng nhập hoặc mật khẩu không đúng.");
                        txtMatKhau.DatConTro();
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
                DatDangXuLy(false);
            }
        }

        private void MoTrangChu(string hoTen)
        {
            Hide();

            try
            {
                using var frm = new FrmTrangChu(hoTen);
                frm.ShowDialog();
            }
            finally
            {
                NapLaiThongTinDaNho();
                XoaLoi();
                Show();
                txtMatKhau.DatConTro();
            }
        }

        // Bam tab "Dang ky" tren thanh tab: mo form Dang ky (tab Dang nhap la tab dang chon nen khong lam gi).
        private void thanhTab_TabBam(object? sender, int chiSo)
        {
            if (chiSo == 1)
                MoFormDangKy();
        }

        // ----- Chuc nang mo rong (ngoai pham vi mon hoc): chi thong bao, chua co logic that -----
        private void btnVssId_Click(object? sender, EventArgs e)
        {
            ThongBaoMoRong(
                "Đăng nhập bằng VssID (ứng dụng của Bảo hiểm xã hội Việt Nam)",
                "cần liên kết với hệ thống VssID");
        }

        private void btnGoogle_Click(object? sender, EventArgs e)
        {
            ThongBaoMoRong(
                "Đăng nhập bằng tài khoản Google",
                "cần đăng ký ứng dụng với Google (OAuth)");
        }

        private static void ThongBaoMoRong(string tenChucNang, string lyDo)
        {
            MessageBox.Show(
                tenChucNang + " là chức năng mở rộng.\n\n" +
                "Chức năng này " + lyDo + " nên chưa có trong bản đồ án này. " +
                "Bạn vui lòng đăng nhập bằng tên đăng nhập và mật khẩu.",
                "Chức năng mở rộng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ----- The ghi chu ten dang nhap (bam bieu tuong i), tao khi can de form mo nhanh -----
        private TheGhiChu? _theGhiChu;

        private int Px(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        private void txtTenDangNhap_NutGhiChuBam(object? sender, EventArgs e)
        {
            if (_theGhiChu == null)
            {
                var the = new TheGhiChu();
                the.DatNoiDung(
                    "Tên đăng nhập có thể là:",
                    "Từ 3 đến 20 ký tự",
                    "Chữ không dấu, chữ số hoặc dấu gạch dưới (_)",
                    "Là tên bạn đã tự đặt khi đăng ký tài khoản");
                the.Click += (s, ev) => the.Visible = false;
                pnlCuon.Controls.Add(the);
                _theGhiChu = the;
            }

            if (_theGhiChu.Visible)
            {
                _theGhiChu.Visible = false;
                return;
            }

            _theGhiChu.MuiTenOPhiaDuoi = false;
            _theGhiChu.Location = new Point(txtTenDangNhap.Left, txtTenDangNhap.Bottom + Px(2));
            _theGhiChu.Size = new Size(txtTenDangNhap.Width, _theGhiChu.TinhChieuCao());
            _theGhiChu.MuiTenX = txtTenDangNhap.TamNutGhiChuX();
            _theGhiChu.BringToFront();
            _theGhiChu.Visible = true;
        }

        private void AnTheGhiChu()
        {
            if (_theGhiChu != null && _theGhiChu.Visible)
                _theGhiChu.Visible = false;
        }

        private void lnkChuyenDangKy_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            MoFormDangKy();
        }

        private void lnkQuenMatKhau_LinkClicked(
            object? sender,
            LinkLabelLinkClickedEventArgs e)
        {
            // TODO Buoc 4.3: noi luong FORGOT_PWD qua TCP.
            MessageBox.Show(
                "Chức năng khôi phục mật khẩu đang được hoàn thiện.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void MoFormDangKy()
        {
            string? tenVuaDangKy = null;
            Point? diemVeBia = null;
            Bitmap? anhHienTai = null;

            try
            {
                // v25: chup form Dang nhap hien tai de form Dang ky lam hieu ung chuyen trang tu anh nay.
                // Khong Hide(): form Dang ky phu kin dung len tren cung vi tri, khong bi nhay man hinh.
                AnTheGhiChu();

                if (HoatAnh.DuocPhep && _bia == null)
                    anhHienTai = ChupNoiDungForm(ClientSize);

                using var frm = new FrmDangKy
                {
                    ViTriMo = Location,
                    AnhFormDangNhap = anhHienTai
                };

                frm.ShowDialog(this);
                tenVuaDangKy = frm.TenVuaDangKy;
                diemVeBia = frm.DiemQuayVeBia;
            }
            finally
            {
                anhHienTai?.Dispose();
                XoaLoi();

                if (tenVuaDangKy != null)
                {
                    // Dang ky xong: dien san ten, dua con tro vao o mat khau.
                    txtTenDangNhap.Text = tenVuaDangKy;
                    XoaONhap(txtMatKhau);
                    txtMatKhau.DatConTro();
                }
                else if (diemVeBia == null)
                {
                    txtTenDangNhap.DatConTro();
                }
            }

            // v23: nguoi dung bam "Quay lai" tren form Dang ky -> ve thang man hinh chao.
            if (diemVeBia is Point diem)
                QuayVeBia(PointToClient(diem));
        }
    }
}
