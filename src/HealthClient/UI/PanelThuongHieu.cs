using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HealthClient.UI
{
    /// <summary>
    /// Bang thuong hieu ben trai man hinh Dang nhap / Dang ky (v35).
    ///
    /// Thay doi so voi v34 (theo yeu cau 01/11/2026):
    ///  - Logo y te giu mau DO + TRANG. Re chuot vao logo: MOT hieu ung duy nhat, nhe, huu han (~1 giay): mot DUONG DIEN TIM chay
    ///    ngang qua logo (trang o ngoai dia, xanh dam trong dia). Het hieu ung thi dung han, Timer tat.
    ///  - Dong khau hieu "Suc khoe la tai san quy gia nhat" la chu dong kieu coolors.co (ChuDong): cham vao thi chu len mau va lac nhe.
    ///  - Ba dong tinh nang dung tu ngu tin cay, DUNG THEO BANG PHAN CONG: nguon tham khao (WHO, Bo Y te, MSD Manual, BV Tam Anh)
    ///    va mat khau bao mat 2 lop (bam PBKDF2 + duong truyen TLS).
    ///
    /// Hieu nang (ngan sach quy tac 13): phan tinh (nen, logo, tieu de, tinh nang) ve MOT LAN vao anh dem 24 bit (~0,7 MB);
    /// moi lan ve lai chi sao chep dung vung bi ban roi ve them phan dong. Hai hieu ung (duong dien tim, chu dong) xin cho tu
    /// GioiHanHoatAnh (toi da 2 cung luc), moi hieu ung chi ve lai dung vung cua no, tu dung khi xong hoac khi cua so mat tieu diem.
    /// Luc dung yen: khong Timer nao chay, CPU = 0. Chua goc tren trai 64 px cho nut "Quay lai".
    /// </summary>
    public class PanelThuongHieu : Panel
    {
        private const string KhauHieu = "Sức khỏe là tài sản quý giá nhất";

        private static readonly Color NenTren = Color.FromArgb(15, 54, 142);
        private static readonly Color NenDuoi = Color.FromArgb(9, 36, 104);
        private static readonly Color ChuMo = Color.FromArgb(178, 202, 242);
        private static readonly Color DoYTe = Color.FromArgb(214, 40, 40);

        private readonly ChuDong _khauHieu;
        private readonly System.Windows.Forms.Timer _boEcg = new() { Interval = 30 };
        private readonly Stopwatch _dongHo = Stopwatch.StartNew();

        private Bitmap? _nen;
        private Size _nenKichThuoc;
        private int _nenDpi;

        private bool _chuotTrenLogo;
        private bool _ecgChay;
        private bool _ecgGiuCho;
        private double _ecgT0;
        private double _ecgKetThuc = -10;

        public PanelThuongHieu()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            _khauHieu = new ChuDong(this);
            _boEcg.Tick += (s, e) => NhipEcg();
        }

        private int S(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        private Rectangle VungLogo()
        {
            int d = S(84);
            return new Rectangle((Width - d) / 2, S(70), d, d);
        }

        // Vung ve lai cua hieu ung duong dien tim: logo + hai ben.
        private Rectangle VungEcg()
        {
            Rectangle r = VungLogo();
            r.Inflate(S(44), S(44));
            return r;
        }

        private Rectangle VungKhauHieu() => new Rectangle(0, Height - S(52), Width, S(40));

        // ----- Chuot -----

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool tren = VungLogo().Contains(e.Location);

            if (tren && !_chuotTrenLogo)
            {
                _chuotTrenLogo = true;
                BatEcg();
            }
            else if (!tren && _chuotTrenLogo)
            {
                _chuotTrenLogo = false;
            }

            _khauHieu.ChuotDi(e.Location);
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _chuotTrenLogo = false;
            _khauHieu.ChuotRoi();
            base.OnMouseLeave(e);
        }

        // ----- Hieu ung duong dien tim (huu han) -----

        private const double ThoiGianEcg = 1.15;

        private void BatEcg()
        {
            if (_ecgChay || !HoatAnh.DuocPhep)
                return;

            if (_dongHo.Elapsed.TotalSeconds - _ecgKetThuc < 0.6 || !GioiHanHoatAnh.Xin())
                return;

            _ecgGiuCho = true;
            _ecgChay = true;
            _ecgT0 = _dongHo.Elapsed.TotalSeconds;
            _boEcg.Start();
            Invalidate(VungEcg());
        }

        private void DungEcg()
        {
            _boEcg.Stop();
            _ecgChay = false;
            _ecgKetThuc = _dongHo.Elapsed.TotalSeconds;

            if (_ecgGiuCho)
            {
                GioiHanHoatAnh.Tra();
                _ecgGiuCho = false;
            }

            if (IsHandleCreated)
                Invalidate(VungEcg());
        }

        private void NhipEcg()
        {
            Form? f = FindForm();

            if (f == null || f.WindowState == FormWindowState.Minimized || Form.ActiveForm != f || !Visible ||
                _dongHo.Elapsed.TotalSeconds - _ecgT0 >= ThoiGianEcg)
            {
                DungEcg();
                return;
            }

            Invalidate(VungEcg());
        }

        // v40: hieu ung logo khi re chuot vao (thay duong dien tim): quang mem + hai vong lan toa + hai cung sang quay quanh dia.
        // Gan voi hieu ung 3D cu nhung chi ve ~8 duong trong vung 172x172 px, MOT lan 1,15 s roi tat Timer (ban cu ve lai ca bang
        // 25 khung/giay lien tuc, kem duong dien tim va vat bay). Khong tao Bitmap/Brush lon, khong Timer chay khi dung yen.
        private void VeEcg(Graphics g)
        {
            double t = _dongHo.Elapsed.TotalSeconds - _ecgT0;
            float p = (float)Math.Clamp(t / ThoiGianEcg, 0.0, 1.0);

            Rectangle logo = VungLogo();
            float cx = logo.X + logo.Width / 2f;
            float cy = logo.Y + logo.Height / 2f;
            float r0 = logo.Width / 2f;
            float dem = (float)Math.Sin(p * Math.PI);   // 0 -> 1 -> 0: quang sang len roi tat dan

            GraphicsState st = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1) Quang mem: ba vanh trong suot quanh dia (ve vanh, khong to kin dia de khong lam nhat chu thap do).
            for (int i = 0; i < 3; i++)
            {
                float rr = r0 + S(6) + i * S(5);

                using (var but = new Pen(Color.FromArgb((int)(46 * dem / (i + 1)), 255, 255, 255), S(5)))
                    g.DrawEllipse(but, cx - rr, cy - rr, rr * 2f, rr * 2f);
            }

            // 2) Hai vong lan toa, vong thu hai tre 22%.
            for (int k = 0; k < 2; k++)
            {
                float pk = (p - k * 0.22f) / 0.78f;

                if (pk <= 0f || pk >= 1f)
                    continue;

                float e = 1f - (1f - pk) * (1f - pk) * (1f - pk);   // easeOutCubic
                float rr = r0 + S(8) + S(26) * e;

                using (var but = new Pen(Color.FromArgb((int)(190 * (1f - pk)), 255, 255, 255), 1.8f))
                    g.DrawEllipse(but, cx - rr, cy - rr, rr * 2f, rr * 2f);
            }

            // 3) Hai cung sang quay quanh dia (nhanh roi cham dan), mo dan o cuoi.
            float goc = -90f + 300f * (1f - (1f - p) * (1f - p));
            float rc = r0 + S(10);
            int al = (int)(235 * (1f - p * p));

            using (var cung = new Pen(Color.FromArgb(al, 255, 255, 255), S(3)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(cung, cx - rc, cy - rc, rc * 2f, rc * 2f, goc, 70f);

            using (var cung2 = new Pen(Color.FromArgb(al * 55 / 100, 255, 255, 255), S(2)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(cung2, cx - rc, cy - rc, rc * 2f, rc * 2f, goc + 180f, 40f);

            g.Restore(st);
        }

        // ----- Ve -----

        private void TaoNen()
        {
            _nen?.Dispose();
            _nen = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            _nenKichThuoc = Size;
            _nenDpi = DeviceDpi;

            using Graphics g = Graphics.FromImage(_nen);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            _ghi = new List<MucChu>();

            try
            {
                VeTinh(g);
            }
            finally
            {
                _chuTinh = _ghi.ToArray();
                _ghi = null;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width < 120 || Height < 120)
                return;

            if (_nen == null || _nenKichThuoc != Size || _nenDpi != DeviceDpi)
                TaoNen();

            Graphics g = e.Graphics;
            Rectangle clip = Rectangle.Intersect(e.ClipRectangle, new Rectangle(Point.Empty, Size));

            if (clip.IsEmpty)
                return;

            // v41: chep 1:1 (khong noi suy, khong tron mau) de khong lam mo hinh khoi, roi tra lai che do mac dinh.
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(_nen!, clip, clip.X, clip.Y, clip.Width, clip.Height, GraphicsUnit.Pixel);
            g.CompositingMode = CompositingMode.SourceOver;
            g.InterpolationMode = InterpolationMode.Default;
            g.PixelOffsetMode = PixelOffsetMode.Default;

            // v41: CHU TINH ve TRUC TIEP len man hinh (ClearType, sac net) thay vi nam trong bitmap dem (chu ve vao bitmap
            // mat khu rang cua ClearType nen bi nhoa/tach mau). Chi ve cac dong cham toi vung dang ve lai.
            foreach (MucChu m in _chuTinh)
            {
                Rectangle v = m.R;
                v.Inflate(8, 8);

                if (clip.IntersectsWith(v))
                    TextRenderer.DrawText(g, m.T, m.F, m.R, m.M, m.C);
            }

            if (clip.IntersectsWith(VungKhauHieu()))
            {
                Rectangle vung = VungKhauHieu();
                vung.Inflate(0, -S(2));
                _khauHieu.Dat(KhauHieu, Theme.Chu(11.5F, FontStyle.Bold), vung, true, true);
                _khauHieu.Ve(g, Color.FromArgb(232, 241, 255));
            }

            if (_ecgChay && clip.IntersectsWith(VungEcg()))
                VeEcg(g);
        }

        // v41: chu tinh KHONG ve vao bitmap dem (chu ve vao bitmap mat khu rang ClearType nen nhoa); chi ghi lai de OnPaint ve
        // truc tiep len man hinh.
        private readonly record struct MucChu(string T, Font F, Rectangle R, Color M, TextFormatFlags C);

        private List<MucChu>? _ghi;
        private MucChu[] _chuTinh = Array.Empty<MucChu>();

        private void GhiChu(Graphics g, string t, Font f, Rectangle r, Color mau, TextFormatFlags co)
        {
            if (_ghi != null)
            {
                _ghi.Add(new MucChu(t, f, r, mau, co));
                return;
            }

            TextRenderer.DrawText(g, t, f, r, mau, co);
        }

        private void VeTinh(Graphics g)
        {
            // 1) Nen xanh dam, doi mau rat nhe tu tren xuong; mot vong tron mo lam chieu sau.
            using (var nen = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), NenTren, NenDuoi, LinearGradientMode.Vertical))
                g.FillRectangle(nen, 0, 0, Width, Height);

            using (var mo = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
                g.FillEllipse(mo, Width - S(210), Height - S(230), S(380), S(380));

            // 2) Logo y te: dia TRANG + chu thap DO, vien ngoai mong.
            Rectangle logo = VungLogo();
            Rectangle vien = Rectangle.Inflate(logo, S(9), S(9));

            using (var but = new Pen(Color.FromArgb(70, 255, 255, 255), 1.5f))
                g.DrawEllipse(but, vien);

            using (var trang = new SolidBrush(Color.White))
                g.FillEllipse(trang, logo);

            float tam = logo.Width / 2f;
            float cx = logo.X + tam;
            float cy = logo.Y + tam;
            float dai = logo.Width * 0.50f;
            float day = logo.Width * 0.17f;

            using (var do_ = new SolidBrush(DoYTe))
            {
                g.FillRectangle(do_, cx - day / 2f, cy - dai / 2f, day, dai);
                g.FillRectangle(do_, cx - dai / 2f, cy - day / 2f, dai, day);
            }

            // 3) Ten he thong + gach chan ngan (moi dong cao 36 cho dong chu 20 pt).
            int y = logo.Bottom + S(26);
            const TextFormatFlags giua = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping;
            Font phongTen = Theme.Chu(20F, FontStyle.Bold);

            GhiChu(g, "Hệ thống tra cứu", phongTen, new Rectangle(0, y, Width, S(36)), Color.White, giua);
            GhiChu(g, "sức khỏe", phongTen, new Rectangle(0, y + S(34), Width, S(36)), Color.White, giua);

            int yGach = y + S(80);

            using (var gach = new Pen(Color.FromArgb(150, 106, 213, 255), 2f))
                g.DrawLine(gach, Width / 2f - S(18), yGach, Width / 2f + S(18), yGach);

            // 4) Mo ta ngan.
            int yMoTa = yGach + S(14);

            GhiChu(
                g, "Tra cứu thông tin triệu chứng, bệnh lý, thuốc và các vấn đề sức khỏe phổ biến.",
                Theme.Chu(10.5F), new Rectangle(S(40), yMoTa, Width - S(80), S(62)), ChuMo,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            // 5) Ba dong tinh nang (chi ve khi con du cho).
            int yTinhNang = yMoTa + S(84);
            int cao = S(54);
            int gioiHan = Height - S(74);
            string[] tieuDe = { "Tra cứu theo triệu chứng", "Nguồn tham khảo uy tín", "Mật khẩu bảo mật 2 lớp" };
            string[] phuDe = { "Chọn nhiều triệu chứng cùng lúc", "WHO · Bộ Y tế · MSD Manual · BV Tâm Anh", "Băm PBKDF2 · đường truyền TLS" };

            for (int i = 0; i < 3; i++)
            {
                int yDong = yTinhNang + i * cao;

                if (yDong + cao > gioiHan)
                    break;

                VeTinhNang(g, i, S(44), yDong, tieuDe[i], phuDe[i]);
            }

            // 6) Duong ke tren khau hieu (khau hieu la phan dong, ve rieng bang ChuDong).
            int yDay = Height - S(58);

            using (var ke = new Pen(Color.FromArgb(46, 255, 255, 255), 1f))
                g.DrawLine(ke, S(44), yDay, Width - S(44), yDay);
        }

        private void VeTinhNang(Graphics g, int loai, int x, int y, string tieuDe, string phuDe)
        {
            int d = S(38);
            var o = new Rectangle(x, y + S(4), d, d);

            using (var nen = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                g.FillEllipse(nen, o);

            using (var vien = new Pen(Color.FromArgb(56, 255, 255, 255), 1f))
                g.DrawEllipse(vien, o);

            VeBieuTuong(g, loai, o.X + d / 2f, o.Y + d / 2f, d * 0.26f);

            int xChu = x + d + S(14);
            int rong = Width - xChu - S(24);
            const TextFormatFlags dd = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;

            GhiChu(g, tieuDe, Theme.Chu(10.5F, FontStyle.Bold), new Rectangle(xChu, y + S(2), rong, S(22)), Color.White, dd);
            GhiChu(g, phuDe, Theme.Chu(9.5F), new Rectangle(xChu, y + S(24), rong, S(20)), ChuMo, dd);
        }

        // Bieu tuong net manh kieu Lucide (kinh lup, khien co dau tich, o khoa), ve bang but trang.
        private static void VeBieuTuong(Graphics g, int loai, float cx, float cy, float r)
        {
            using var but = new Pen(Color.White, Math.Max(1.4f, r * 0.2f));
            but.StartCap = LineCap.Round;
            but.EndCap = LineCap.Round;
            but.LineJoin = LineJoin.Round;

            if (loai == 0)
            {
                g.DrawEllipse(but, cx - r * 0.95f, cy - r * 0.95f, r * 1.4f, r * 1.4f);
                g.DrawLine(but, cx + r * 0.25f, cy + r * 0.25f, cx + r * 0.95f, cy + r * 0.95f);
            }
            else if (loai == 1)
            {
                g.DrawPolygon(but, new[]
                {
                    new PointF(cx, cy - r),
                    new PointF(cx + r * 0.85f, cy - r * 0.62f),
                    new PointF(cx + r * 0.85f, cy + r * 0.12f),
                    new PointF(cx, cy + r),
                    new PointF(cx - r * 0.85f, cy + r * 0.12f),
                    new PointF(cx - r * 0.85f, cy - r * 0.62f)
                });

                g.DrawLines(but, new[]
                {
                    new PointF(cx - r * 0.35f, cy),
                    new PointF(cx - r * 0.07f, cy + r * 0.3f),
                    new PointF(cx + r * 0.42f, cy - r * 0.25f)
                });
            }
            else
            {
                g.DrawRectangle(but, cx - r * 0.78f, cy - r * 0.08f, r * 1.56f, r * 1.0f);
                g.DrawArc(but, cx - r * 0.5f, cy - r * 0.95f, r * 1.0f, r * 1.7f, 180, 180);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _boEcg.Stop();
                _boEcg.Dispose();

                if (_ecgGiuCho)
                {
                    GioiHanHoatAnh.Tra();
                    _ecgGiuCho = false;
                }

                _khauHieu.Dispose();
                _nen?.Dispose();
                _nen = null;
            }

            base.Dispose(disposing);
        }
    }
}
