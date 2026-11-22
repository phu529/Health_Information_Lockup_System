using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HealthClient.UI
{
    /// <summary>
    /// Lop bia chao (v35): man hinh mo dau, thiet ke nhu trang web y te / nha thuoc that (Pharmacity, Long Chau, Mayo Clinic:
    /// o tim kiem la trung tam, nen sang phang, tin hieu tin cay hien san, it thu gay vuong).
    ///
    /// Thay doi so voi v34 (theo yeu cau 01/11/2026):
    ///  - BO thanh menu tren (Trieu chung / Benh ly / Thuoc / Dang nhap) va nut "Xem demo": cac chuc nang chi hien SAU khi dang nhap.
    ///  - Demo go chu o the ben phai CHI CHAY khi re chuot vao the (de nguoi dung xem thu cach tra cuu), moi lan mot bo trieu chung
    ///    ngau nhien (toi da 3 trieu chung), toc do vua phai; xong thi dung han. Luc dung yen the hien san mot ket qua mau.
    ///  - Logo y te mau DO + TRANG (dia do, chu thap trang).
    ///  - v44: goc PHAI TREN (cho truoc day la thanh menu) co TRAI TIM xanh ngoc dap nhip (UI/TimDap.cs), can giua theo hang voi logo ben trai,
    ///    mep phai thang hang voi mep phai cua the va duong ke. Tim dap theo Timer dien tim CO SAN (4 nhip trong luc duong dien tim chay,
    ///    va moi lan re chuot vao tim) roi nghi han: khong them Timer, khong them Bitmap, luc dung yen CPU = 0.
    ///  - Dong chu nho phia tren tieu de: khau hieu tin cay "Uy tin - Tin cay - Dong hanh" (thay danh sach nguon).
    ///  - Sua loi chu bi khuyet: khung chua chu cao hon chieu cao dong chu va ve voi NoClipping (duoi chu g, dau thanh khong bi cat).
    ///
    /// Hieu nang (ngan sach quy tac 13): phan tinh (nen, tieu de, the, chan trang) duoc ve MOT LAN vao mot anh dem 24 bit
    /// (~1,7 MB), moi lan ve lai chi sao chep dung vung bi ban (Rectangle bi hong), roi ve them phan dong. Chi co mot Timer 33 ms,
    /// chay ~3 giay moi lan demo roi tu dung; xin cho tu GioiHanHoatAnh; dung ngay khi cua so mat tieu diem. Luc dung yen CPU = 0.
    ///
    /// Bam chuot TRAI vao nut (hoac bat ky cho nao tren bia) moi mo vao Dang nhap; phim, chuot phai khong mo. Diem bam (toa do cua bia)
    /// duoc gui kem su kien YeuCauMo de FrmDangNhap lam hieu ung chuyen canh. Khong cham toi logic dang nhap, mang hay du lieu.
    /// </summary>
    public sealed class LopBiaChao : Panel
    {
        // Bo trieu chung mau cho demo (toi da 3 trieu chung moi bo). Chi la VI DU MINH HOA, khong phai du lieu y khoa.
        private static readonly (string Trieu, string[] Ten, float[] TyLe)[] BoMau =
        {
            ("Sốt, ho, đau họng", new[] { "Cảm cúm", "Viêm họng cấp", "Viêm phế quản" }, new[] { 0.84f, 0.58f, 0.34f }),
            ("Đau đầu, buồn nôn", new[] { "Đau nửa đầu", "Đau đầu do căng thẳng", "Rối loạn tiền đình" }, new[] { 0.80f, 0.56f, 0.30f }),
            ("Đau bụng, tiêu chảy", new[] { "Rối loạn tiêu hóa", "Viêm dạ dày - ruột", "Ngộ độc thực phẩm" }, new[] { 0.78f, 0.60f, 0.33f }),
            ("Ngứa, nổi mẩn đỏ", new[] { "Viêm da dị ứng", "Mề đay", "Chàm (eczema)" }, new[] { 0.82f, 0.57f, 0.31f }),
            ("Sổ mũi, hắt hơi, ngạt mũi", new[] { "Viêm mũi dị ứng", "Cảm lạnh thông thường", "Viêm xoang" }, new[] { 0.83f, 0.60f, 0.35f }),
            ("Ho khan, khàn tiếng", new[] { "Viêm thanh quản", "Viêm họng", "Trào ngược dạ dày" }, new[] { 0.79f, 0.55f, 0.32f }),
        };

        // v40: cau hoi + loi khuyen RIENG cho tung bo trieu chung (cung chi so voi BoMau). Chi la VI DU MINH HOA giao dien,
        // khong thay the tu van cua bac si; nguyen tac chung: dau hieu nang / keo dai / kem theo dau hieu bat thuong thi di kham.
        private static readonly (string Hoi, string Dap)[] LoiKhuyen =
        {
            ("Khi nào nên đi khám?", "Sốt cao kéo dài trên 3 ngày hoặc khó thở"),
            ("Dấu hiệu cần đi khám ngay", "Đau đầu dữ dội đột ngột, kèm yếu liệt hoặc nói khó"),
            ("Khi nào nên đi khám?", "Tiêu chảy quá 2 ngày, mất nước hoặc đi ngoài ra máu"),
            ("Dấu hiệu cần đi khám ngay", "Mẩn đỏ lan nhanh, sưng môi mặt hoặc khó thở"),
            ("Khi nào nên đi khám?", "Triệu chứng kéo dài trên 10 ngày hoặc sốt cao"),
            ("Khi nào nên đi khám?", "Khàn tiếng trên 2 tuần hoặc ho ra máu"),
        };

        private const string KhauHieu = "Tin cậy - Gần gũi - Đồng hành";

        private static readonly Color NenTrang = Color.FromArgb(246, 249, 253);
        private static readonly Color VienThe = Color.FromArgb(222, 230, 242);
        private static readonly Color MauXanhNhat = Color.FromArgb(232, 240, 253);
        private static readonly Color DoYTe = Color.FromArgb(214, 40, 40);
        private static readonly Random Ngau = new();

        // Thoi gian demo (giay): vua phai, khong qua nhanh / qua cham.
        private const double ThoiTre = 0.40;
        private const double ThoiChu = 0.09;
        private const double ThoiNghi = 0.45;
        private const double ThoiHang = 0.16;
        private const double ThoiThanh = 0.75;

        private readonly NutBoGoc _nut;
        private readonly System.Windows.Forms.Timer _bo = new() { Interval = 33 };
        private readonly System.Windows.Forms.Timer _boEcg = new() { Interval = 33 };   // v42: Timer dien tim, chi chay ~3,6 s roi tu dung

        private const double ThoiEcg = 3.6;   // v42b: cham hon (truoc 1,25 s), van toc KHONG DOI nhu man hinh monitor
        private bool _ecgChay;
        private bool _ecgGiuCho;
        private double _ecgT0;
        private double _ecgKetThuc = -10;
        private readonly Stopwatch _dongHo = Stopwatch.StartNew();

        private Bitmap? _nen;
        private Size _nenKichThuoc;
        private int _nenDpi;

        private Rectangle _the;
        private Rectangle _o;
        private Rectangle _vungDong;
        private Rectangle _tim;       // v44: hinh tron nen cua trai tim o goc phai tren
        private Rectangle _vungTim;   // v44: vung ve lai khi tim dap (hinh tron + vong lan toa)
        private bool _trenTim;
        private Rectangle _nho;   // the nho "khi nao nen di kham" (v40: ve dong, doi noi dung theo trieu chung)

        private bool _daGui;
        private bool _chay;
        private bool _giuCho;
        private bool _trenThe;
        private double _t0;
        private double _tKetThuc = -10;
        private int _chon;
        private int _nChu = int.MaxValue;
        private readonly float[] _tien = { 1f, 1f, 1f };

        public event EventHandler<Point>? YeuCauMo;

        public LopBiaChao()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            BackColor = NenTrang;
            TabStop = false;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.Pane;
            AccessibleName = "Màn hình chào. Nhấp chuột để bắt đầu tra cứu.";

            _bo.Tick += (s, e) => Nhip();
            _boEcg.Tick += (s, e) => NhipEcg();

            _nut = new NutBoGoc
            {
                Name = "btnBatDauTraCuu",
                Text = "Bắt đầu tra cứu",
                MauNen = Theme.Navy,
                MauRe = Theme.Teal,
                MauChu = Color.White,
                Size = new Size(252, 64),
                TabStop = false,
                Cursor = Cursors.Hand
            };

            // Chi bam chuot TRAI moi mo (Enter / Space tren nut khong tinh).
            _nut.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    GuiYeuCau(new Point(_nut.Left + e.X, _nut.Top + e.Y));
            };

            _nut.MouseEnter += (s, e) => BatEcg(0);   // re chuot vao nut: duong dien tim chay lai mot lan

            Controls.Add(_nut);

            DoHieuNang.KichHoat();   // Ctrl + Shift + F9: bang do CPU / RAM
        }

        private float K => Math.Max(0.5f, Math.Min(Width / 920f, Height / 640f));

        private int P(float v) => (int)Math.Round(v * K);

        private void GuiYeuCau(Point diem)
        {
            if (_daGui)
                return;

            _daGui = true;
            Dung(false);
            DungEcg();
            YeuCauMo?.Invoke(this, diem);
        }

        // ----- Bo cuc -----

        private void TinhVung()
        {
            int rongThe = P(368);
            _the = new Rectangle(Width - P(56) - rongThe, P(100), rongThe, P(372));
            _o = new Rectangle(_the.X + P(20), _the.Y + P(66), _the.Width - P(40), P(42));
            _vungDong = new Rectangle(_the.X + P(2), _the.Y + P(60), _the.Width - P(4), P(272));

            // The nho rong hon (262 thay vi 222) va cao 80 de cau tra loi dai xuong duoc 2 dong, khong bi cat chu.
            _nho = new Rectangle(_the.X - P(44), _the.Bottom - P(40), P(262), P(80));
            _vungDong = Rectangle.Union(_vungDong, Rectangle.Inflate(_nho, P(24), P(24)));

            // v44: tim o goc phai tren: tam cung hang voi logo (y = 22 + 34/2), mep phai = mep phai cua the (Width - 56).
            int dTim = P(52);
            _tim = new Rectangle(Width - P(56) - dTim, P(39) - dTim / 2, dTim, dTim);
            _vungTim = Rectangle.Inflate(_tim, P(8), P(8));
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (Width > 0 && Height > 0)
            {
                TinhVung();
                _nut.Size = new Size(P(252), P(64));
                _nut.Location = new Point(P(56), P(344));
            }
        }

        // ----- Chuot -----

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
                GuiYeuCau(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool tren = _the.Contains(e.Location);

            if (tren && !_trenThe)
                Bat();

            _trenThe = tren;

            // v44: re chuot vao trai tim thi tim dap lai (cung nhip voi duong dien tim; co thoi gian nghi nen khong lap lien tuc).
            bool trenTim = _tim.Contains(e.Location);

            if (trenTim && !_trenTim)
                BatEcg(0);

            _trenTim = trenTim;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _trenThe = false;
            base.OnMouseLeave(e);
        }

        // ----- Demo go chu (chi chay khi re chuot vao the) -----

        private double Giay() => _dongHo.Elapsed.TotalSeconds;

        private void DatTrangThaiCuoi()
        {
            _nChu = int.MaxValue;
            _tien[0] = _tien[1] = _tien[2] = 1f;
        }

        private void Bat()
        {
            if (_chay || _daGui || Giay() - _tKetThuc < 0.8)
                return;

            // Chon bo trieu chung khac bo dang hien.
            int moi = _chon;

            while (moi == _chon)
                moi = Ngau.Next(BoMau.Length);

            _chon = moi;

            if (!HoatAnh.DuocPhep || !GioiHanHoatAnh.Xin())
            {
                // Tat hieu ung hoac het cho: hien thang ket qua cua bo moi, khong chay.
                DatTrangThaiCuoi();
                Invalidate(_vungDong);
                return;
            }

            _giuCho = true;
            _nChu = 0;
            _tien[0] = _tien[1] = _tien[2] = 0f;
            _t0 = Giay();
            _chay = true;
            _bo.Start();
            Invalidate(_vungDong);
        }

        private void Dung(bool veLai)
        {
            _bo.Stop();
            _chay = false;
            _tKetThuc = Giay();

            if (_giuCho)
            {
                GioiHanHoatAnh.Tra();
                _giuCho = false;
            }

            DatTrangThaiCuoi();

            if (veLai && IsHandleCreated)
                Invalidate(_vungDong);
        }

        private void Nhip()
        {
            Form? f = FindForm();

            if (f == null || f.WindowState == FormWindowState.Minimized || Form.ActiveForm != f || !Visible)
            {
                Dung(true);
                return;
            }

            string q = BoMau[_chon].Trieu;
            double t = Giay() - _t0;
            double tKetQua = ThoiTre + q.Length * ThoiChu + ThoiNghi;
            double tTong = tKetQua + 2 * ThoiHang + ThoiThanh;

            if (t >= tTong)
            {
                Dung(true);
                return;
            }

            int chu = (int)Math.Clamp(Math.Floor((t - ThoiTre) / ThoiChu), 0, q.Length);
            bool doi = chu != _nChu;
            _nChu = chu;

            for (int i = 0; i < 3; i++)
            {
                double x = Math.Clamp((t - tKetQua - i * ThoiHang) / ThoiThanh, 0.0, 1.0);
                float moi = (float)(1.0 - Math.Pow(1.0 - x, 3.0));   // easeOutCubic

                if (Math.Abs(moi - _tien[i]) > 0.001f)
                {
                    _tien[i] = moi;
                    doi = true;
                }
            }

            if (doi)
                Invalidate(_vungDong);
        }

        // ----- Duong dien tim (hieu ung huu han: chay MOT lan ~1,25 giay roi tu dung) -----

        private float YEcg => P(538);

        private Rectangle VungEcg() => new Rectangle(0, (int)(YEcg - P(36)), Width, P(66));   // chua ca quang sang quanh dau

        // Mau mot nhip tim that (v42b): song P nho, phuc bo QRS nhon, doan ST phang, song T tron. (vi tri 0..1, bien do; am = len tren)
        private static readonly float[,] NhipMau = TaoNhipMau();

        private static float[,] TaoNhipMau()
        {
            var ds = new List<(float, float)> { (0f, 0f) };

            void Bump(float tam, float rong, float cao)
            {
                for (int i = -4; i <= 4; i++)
                {
                    float s = i / 4f;
                    ds.Add((tam + s * rong / 2f, cao * 0.5f * (1f + (float)Math.Cos(Math.PI * s))));
                }
            }

            Bump(0.10f, 0.12f, -0.16f);                                                         // song P
            ds.AddRange(new[] { (0.30f, 0f), (0.33f, 0.10f), (0.365f, -1f), (0.40f, 0.22f), (0.43f, 0f) });   // QRS
            Bump(0.58f, 0.20f, -0.30f);                                                         // song T
            ds.Add((1f, 0f));

            var kq = new float[ds.Count, 2];

            for (int i = 0; i < ds.Count; i++)
            {
                kq[i, 0] = ds[i].Item1;
                kq[i, 1] = ds[i].Item2;
            }

            return kq;
        }

        private float ChuKyEcg => P(250);

        private float BienEcg => P(22);

        private PointF[] DiemEcg()
        {
            float dai = ChuKyEcg;
            int n = NhipMau.GetLength(0);
            int soNhip = (int)Math.Ceiling(Width / dai) + 2;
            var ds = new List<PointF>(soNhip * n);

            for (int k = -1; k < soNhip; k++)
                for (int i = (k < 0 ? 0 : 1); i < n; i++)
                    ds.Add(new PointF(k * dai + NhipMau[i, 0] * dai, YEcg + NhipMau[i, 1] * BienEcg));

            return ds.ToArray();
        }

        private float YTai(float x)
        {
            float dai = ChuKyEcg;
            float u = x / dai - (float)Math.Floor(x / dai);
            int n = NhipMau.GetLength(0);

            for (int i = 1; i < n; i++)
            {
                if (u <= NhipMau[i, 0])
                {
                    float w = NhipMau[i, 0] - NhipMau[i - 1, 0];
                    float s = w <= 0f ? 1f : (u - NhipMau[i - 1, 0]) / w;
                    return YEcg + (NhipMau[i - 1, 1] + (NhipMau[i, 1] - NhipMau[i - 1, 1]) * s) * BienEcg;
                }
            }

            return YEcg;
        }

        private void VeEcg(Graphics g)
        {
            double t = Giay() - _ecgT0;

            if (t < 0 || t >= ThoiEcg)
                return;

            float p = (float)(t / ThoiEcg);
            float duoi = P(300);
            float dau = (Width + duoi) * p;
            float duoiX = dau - duoi;
            float tu = Math.Max(0f, duoiX);
            float den = Math.Min(Width, dau);

            if (den <= tu)
                return;

            GraphicsState st = g.Save();

            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.IntersectClip(new RectangleF(tu, YEcg - P(26), den - tu, P(44)));

                PointF[] diem = DiemEcg();

                using (var co1 = new LinearGradientBrush(new PointF(duoiX, 0), new PointF(dau, 0), Color.FromArgb(0, 120, 185, 255), Color.FromArgb(80, 120, 185, 255)))
                using (var co2 = new LinearGradientBrush(new PointF(duoiX, 0), new PointF(dau, 0), Color.FromArgb(0, 56, 140, 235), Color.FromArgb(255, 56, 140, 235)))
                using (var but1 = new Pen(co1, Math.Max(5f, P(7f))))
                using (var but2 = new Pen(co2, Math.Max(1.8f, P(2.2f))))
                {
                    but1.LineJoin = LineJoin.Round;
                    but2.LineJoin = LineJoin.Round;
                    g.DrawLines(but1, diem);
                    g.DrawLines(but2, diem);
                }
            }
            finally
            {
                g.Restore(st);
            }

            if (dau > 0 && dau < Width)
            {
                float y = YTai(dau);

                using (var q1 = new SolidBrush(Color.FromArgb(16, 90, 170, 255)))
                using (var q2 = new SolidBrush(Color.FromArgb(34, 90, 170, 255)))
                using (var loi = new SolidBrush(Color.FromArgb(56, 140, 235)))
                {
                    g.FillEllipse(q1, dau - P(14), y - P(14), P(28), P(28));
                    g.FillEllipse(q2, dau - P(8.5f), y - P(8.5f), P(17), P(17));
                    g.FillEllipse(loi, dau - P(3.2f), y - P(3.2f), P(6.4f), P(6.4f));
                }
            }
        }

        private void BatEcg(double troSau)
        {
            if (_ecgChay || _daGui || !IsHandleCreated || !HoatAnh.DuocPhep)
                return;

            if (Giay() - _ecgKetThuc < 0.5 || !GioiHanHoatAnh.Xin())
                return;

            _ecgGiuCho = true;
            _ecgChay = true;
            _ecgT0 = Giay() + troSau;
            _boEcg.Start();
        }

        private void DungEcg()
        {
            _boEcg.Stop();
            _ecgChay = false;
            _ecgKetThuc = Giay();

            if (_ecgGiuCho)
            {
                GioiHanHoatAnh.Tra();
                _ecgGiuCho = false;
            }

            if (IsHandleCreated)
            {
                Invalidate(VungEcg());
                Invalidate(_vungTim);   // tra trai tim ve dang nghi (ban tinh trong anh dem)
            }
        }

        private void NhipEcg()
        {
            Form? f = FindForm();
            double t = Giay() - _ecgT0;

            if (f == null || f.WindowState == FormWindowState.Minimized || !Visible || _daGui ||
                (Form.ActiveForm != f && t > 0.4) || t >= ThoiEcg)
            {
                DungEcg();
                return;
            }

            Invalidate(VungEcg());
            Invalidate(_vungTim);   // v44: trai tim dap theo cung nhip Timer nay (~30 khung/giay, chi vung ~70x70 px)
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BeginInvoke(new Action(() =>
            {
                if (Visible)
                    BatEcg(0.30);
            }));
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Moi lan bia hien ra (mo ung dung, hoac bam Quay lai) duong dien tim chay mot lan.
            if (Visible && IsHandleCreated)
                BeginInvoke(new Action(() => BatEcg(0.30)));
        }

        // ----- Ve -----

        private static GraphicsPath TheBoGoc(RectangleF r, float bk)
        {
            var p = new GraphicsPath();
            float d = Math.Max(1f, Math.Min(bk * 2f, Math.Min(r.Width, r.Height)));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // v41: mot dong chu da dinh vi. Khi ve lop tinh vao bitmap dem, Chu() chi GHI LAI (khong ve), de OnPaint ve chu truc
        // tiep len man hinh. Khi khong ghi (phan dong VeDong) thi ve ngay.
        private readonly record struct MucChu(string T, Font F, Rectangle R, Color M, TextFormatFlags C);

        private List<MucChu>? _ghi;
        private MucChu[] _chuTinh = Array.Empty<MucChu>();

        // Moi dong chu ve voi NoClipping de duoi chu (g, y, p) va dau thanh khong bi cat.
        private void Chu(Graphics g, string t, Font f, RectangleF r, Color mau, TextFormatFlags co)
        {
            TextFormatFlags c = co | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.NoClipping;
            Rectangle rr = Rectangle.Round(r);

            if (_ghi != null)
            {
                _ghi.Add(new MucChu(t, f, rr, mau, c));
                return;
            }

            TextRenderer.DrawText(g, t, f, rr, mau, c);
        }

        // Logo y te: dia DO + chu thap TRANG.
        private static void VeLogo(Graphics g, float x, float y, float d)
        {
            using (var nen = new SolidBrush(DoYTe))
                g.FillEllipse(nen, x, y, d, d);

            float cx = x + d / 2f;
            float cy = y + d / 2f;
            float dai = d * 0.52f;
            float day = d * 0.17f;

            using (var b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, cx - day / 2f, cy - dai / 2f, day, dai);
                g.FillRectangle(b, cx - dai / 2f, cy - day / 2f, dai, day);
            }
        }

        private static void VeTich(Graphics g, float x, float y, float d, Color nen)
        {
            using (var co = new SolidBrush(nen))
                g.FillEllipse(co, x, y, d, d);

            using (var but = new Pen(Color.White, Math.Max(1.4f, d * 0.12f)))
            {
                but.StartCap = LineCap.Round;
                but.EndCap = LineCap.Round;
                but.LineJoin = LineJoin.Round;
                g.DrawLines(but, new[]
                {
                    new PointF(x + d * 0.27f, y + d * 0.52f),
                    new PointF(x + d * 0.43f, y + d * 0.68f),
                    new PointF(x + d * 0.74f, y + d * 0.32f)
                });
            }
        }

        private void VeThe(Graphics g, RectangleF r, float bk)
        {
            for (int i = 4; i >= 1; i--)
            {
                var b = new RectangleF(r.X - i, r.Y + i * 2f, r.Width + 2 * i, r.Height + i);

                using (GraphicsPath dp = TheBoGoc(b, bk + i))
                using (var co = new SolidBrush(Color.FromArgb(7, 20, 50, 110)))
                    g.FillPath(co, dp);
            }

            using (GraphicsPath dp = TheBoGoc(r, bk))
            {
                using (var nen = new SolidBrush(Color.White))
                    g.FillPath(nen, dp);

                using (var vien = new Pen(VienThe, 1f))
                    g.DrawPath(vien, dp);
            }
        }

        private void TaoNen()
        {
            _nen?.Dispose();
            _nen = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            _nenKichThuoc = Size;
            _nenDpi = DeviceDpi;

            using Graphics g = Graphics.FromImage(_nen);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var nen = new LinearGradientBrush(new Rectangle(0, 0, Math.Max(2, Width), Math.Max(2, Height)), Color.FromArgb(250, 252, 255), Color.FromArgb(232, 241, 254), 90f))
                g.FillRectangle(nen, 0, 0, Width, Height);

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

            if (Width < 560 || Height < 380)
                return;

            if (_nen == null || _nenKichThuoc != Size || _nenDpi != DeviceDpi)
            {
                TinhVung();
                TaoNen();
            }

            Graphics g = e.Graphics;
            Rectangle clip = Rectangle.Intersect(e.ClipRectangle, new Rectangle(Point.Empty, Size));

            if (clip.IsEmpty)
                return;

            // 1) Sao chep lop tinh (chi dung vung bi ban), 2) ve them phan dong neu vung ban cham toi.
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

            if (clip.IntersectsWith(_vungDong))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                VeDong(g);
            }

            if (_ecgChay && clip.IntersectsWith(VungEcg()))
                VeEcg(g);

            if (_ecgChay && clip.IntersectsWith(_vungTim))
            {
                double t = Giay() - _ecgT0;
                TimDap.Ve(g, _tim, TimDap.Nhip(t), TimDap.VongSong(t));
            }
        }

        private void VeTinh(Graphics g)
        {
            const TextFormatFlags trai = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            float x0 = P(56);

            // Vong tron nhat lam chieu sau cho ben phai (khong cham vung nut ben trai).
            using (var nen = new SolidBrush(Color.FromArgb(226, 237, 252)))
                g.FillEllipse(nen, Width - P(380), P(120), P(520), P(520));

            // Thanh dau trang: chi logo + ten (khong menu).
            VeLogo(g, x0, P(22), P(34));
            Chu(g, "Tra cứu sức khỏe", Theme.Chu(13F, FontStyle.Bold), new RectangleF(x0 + P(46), P(22), P(280), P(34)), Theme.Navy, trai);

            using (var ke = new Pen(Color.FromArgb(226, 233, 243), 1f))
                g.DrawLine(ke, x0, P(70), Width - x0, P(70));

            // v44: trai tim o goc phai tren (dang nghi); khi dap duoc ve lai dong o OnPaint.
            TimDap.Ve(g, _tim, 0f, 0f);

            // Khau hieu tin cay trong vien thuoc nho.
            Font phongNho = Theme.Chu(9.5F);
            Size dN = TextRenderer.MeasureText(g, KhauHieu, phongNho, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            var the = new RectangleF(x0, P(100), dN.Width + P(46), P(30));

            using (GraphicsPath dp = TheBoGoc(the, the.Height / 2f))
            {
                using (var nen = new SolidBrush(Color.White))
                    g.FillPath(nen, dp);

                using (var vien = new Pen(VienThe, 1f))
                    g.DrawPath(vien, dp);
            }

            using (var cham = new SolidBrush(DoYTe))
                g.FillEllipse(cham, the.X + P(14), the.Y + the.Height / 2f - P(3), P(6), P(6));

            Chu(g, KhauHieu, phongNho, new RectangleF(the.X + P(28), the.Y, dN.Width + P(14), the.Height), Theme.ChuPhu, trai);

            // Tieu de lon: moi dong cao 54 (cho dong chu 28 pt), khong bi cat duoi chu.
            Font phongTo = Theme.Chu(28F, FontStyle.Bold);
            Chu(g, "Tra cứu triệu chứng", phongTo, new RectangleF(x0, P(142), P(460), P(54)), Theme.Navy, trai);
            Chu(g, "và thông tin sức khỏe", phongTo, new RectangleF(x0, P(192), P(460), P(54)), Theme.Navy, trai);

            Chu(
                g, "Chọn triệu chứng bạn đang gặp, xem các bệnh lý có thể liên quan và tra cứu thông tin thuốc mang tính tham khảo.",
                Theme.Chu(12F), new RectangleF(x0, P(256), P(420), P(80)), Theme.ChuPhu,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak);

            // Ba y chinh duoi nut.
            string[] y3 = { "Chọn nhiều triệu chứng cùng lúc", "Đối chiếu nguồn WHO, Bộ Y tế, MSD Manual", "Mật khẩu bảo mật 2 lớp: PBKDF2 + TLS" };

            for (int i = 0; i < 3; i++)
            {
                float yy = P(428 + i * 30);
                VeTich(g, x0, yy, P(20), Theme.Teal);
                Chu(g, y3[i], Theme.Chu(10.5F), new RectangleF(x0 + P(30), yy - P(2), P(420), P(24)), Theme.ChuChinh, trai);
            }

            // v42: duong dien tim nen (xanh nhat, tinh) thay cho duong ke; vet xanh dam chay qua la phan dong (VeEcg).
            using (var but = new Pen(Color.FromArgb(70, 92, 160, 238), Math.Max(1.2f, P(1.4f))))
            {
                but.LineJoin = LineJoin.Round;
                g.DrawLines(but, DiemEcg());
            }

            // Dai nguon tham khao (dung theo bang phan cong: WHO, Bo Y te, Cuc Quan ly Duoc, MSD Manual, BV Tam Anh).
            Chu(g, "Nguồn tham khảo", Theme.Chu(9.5F, FontStyle.Bold), new RectangleF(x0, P(562), P(130), P(24)), Theme.ChuChinh, trai);
            Chu(
                g, "WHO  ·  Bộ Y tế  ·  Cục Quản lý Dược  ·  MSD Manual  ·  BV Tâm Anh", Theme.Chu(9.5F),
                new RectangleF(x0 + P(136), P(562), Width - 2 * x0 - P(136), P(24)), Theme.ChuPhu, trai);

            // Chan trang: cau luu y y khoa, v42: chu IN DAM, mau do dam hon (chi doi chu, khong them the).
            Chu(
                g, "Thông tin chỉ mang tính tham khảo, không thể thay thế chẩn đoán của bác sĩ.",
                Theme.Chu(10.5F, FontStyle.Bold), new RectangleF(x0, Height - P(40), Width - 2 * x0, P(26)), Color.FromArgb(200, 30, 30), trai);

            VeVoThe(g);
        }

        // Phan tinh cua the xem truoc (vo the, tieu de, o tim kiem trong, nhan, thanh nen, ghi chu, the nho).
        private void VeVoThe(Graphics g)
        {
            const TextFormatFlags trai = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            const TextFormatFlags phai = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;

            VeThe(g, _the, P(20));

            float xl = _the.X + P(20);
            float rong = _the.Width - P(40);

            VeLogo(g, xl, _the.Y + P(16), P(24));
            Chu(g, "Tra cứu triệu chứng", Theme.Chu(10.5F, FontStyle.Bold), new RectangleF(xl + P(34), _the.Y + P(16), P(220), P(24)), Theme.Navy, trai);

            using (var ke = new Pen(Color.FromArgb(232, 237, 245), 1f))
                g.DrawLine(ke, xl, _the.Y + P(52), xl + rong, _the.Y + P(52));

            // O tim kiem (nen + kinh lup; chu go chu la phan dong).
            using (GraphicsPath dp = TheBoGoc(_o, P(12)))
            {
                using (var nen = new SolidBrush(Color.FromArgb(243, 247, 252)))
                    g.FillPath(nen, dp);

                using (var vien = new Pen(VienThe, 1f))
                    g.DrawPath(vien, dp);
            }

            float mx = _o.X + P(16);
            float my = _o.Y + _o.Height / 2f;

            using (var but = new Pen(Theme.ChuPhu, Math.Max(1.5f, P(1.8f))))
            {
                but.StartCap = LineCap.Round;
                but.EndCap = LineCap.Round;
                g.DrawEllipse(but, mx, my - P(7), P(11), P(11));
                g.DrawLine(but, mx + P(9.5f), my + P(3.5f), mx + P(14), my + P(8));
            }

            Chu(g, "Gợi ý tham khảo", Theme.Chu(9.5F, FontStyle.Bold), new RectangleF(xl, _the.Y + P(166), rong, P(22)), Theme.ChuPhu, trai);

            // Thanh nen (rong) cua 3 hang ket qua.
            for (int i = 0; i < 3; i++)
            {
                var rai = new RectangleF(xl, _the.Y + P(192) + i * P(48) + P(28), rong, P(7));

                using (GraphicsPath dp = TheBoGoc(rai, rai.Height / 2f))
                using (var nen = new SolidBrush(Color.FromArgb(232, 238, 247)))
                    g.FillPath(nen, dp);
            }

            Chu(g, "Ví dụ minh họa giao diện", Theme.Chu(8.5F), new RectangleF(xl, _the.Bottom - P(30), rong, P(20)), Color.FromArgb(130, 140, 156), phai);
        }

        // Phan dong: chu dang go, nhan trieu chung, ten + thanh muc cua 3 benh ly.
        private void VeDong(Graphics g)
        {
            const TextFormatFlags trai = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            const TextFormatFlags phai = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;

            var bo = BoMau[_chon];
            string q = bo.Trieu;
            int nChu = Math.Min(_nChu, q.Length);
            string go = q.Substring(0, nChu);
            bool dangGo = _chay && nChu < q.Length;

            // Cho de thay chu bi ghi de: lap lai nen o tim kiem (da duoc sao chep tu lop tinh nen khong can ve lai).
            var vungChu = new Rectangle(_o.X + P(40), _o.Y, _o.Width - P(50), _o.Height);
            Font phongO = Theme.Chu(10.5F);

            if (go.Length == 0 && _chay)
            {
                Chu(g, "Nhập triệu chứng...", phongO, vungChu, Color.FromArgb(140, 150, 166), trai);
            }
            else
            {
                Chu(g, go, phongO, vungChu, Theme.ChuChinh, trai);

                if (dangGo)
                {
                    Size dg = TextRenderer.MeasureText(g, go.Length == 0 ? " " : go + "|", phongO, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                    Size dgach = TextRenderer.MeasureText(g, "|", phongO, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                    float cx = vungChu.X + dg.Width - dgach.Width + 1;

                    using (var but = new Pen(Theme.Navy, Math.Max(1.5f, P(1.5f))))
                        g.DrawLine(but, cx, _o.Y + P(11), cx, _o.Bottom - P(11));
                }
            }

            // Nhan trieu chung: chi hien cac trieu chung da go xong (sau dau phay), hoac tat ca khi go xong.
            string[] tach = q.Split(new[] { ", " }, StringSplitOptions.None);
            int xong = go.Count(c => c == ',') + (nChu >= q.Length ? 1 : 0);
            float cxChip = _the.X + P(20);
            float cyChip = _the.Y + P(122);
            Font phongChip = Theme.Chu(9.5F, FontStyle.Bold);

            for (int i = 0; i < tach.Length && i < xong; i++)
            {
                Size dn = TextRenderer.MeasureText(g, tach[i], phongChip, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                var chip = new RectangleF(cxChip, cyChip, dn.Width + P(28), P(28));

                using (GraphicsPath dp = TheBoGoc(chip, chip.Height / 2f))
                using (var nen = new SolidBrush(MauXanhNhat))
                    g.FillPath(nen, dp);

                Chu(g, tach[i], phongChip, new RectangleF(chip.X + P(12), chip.Y, dn.Width + P(14), chip.Height), Theme.Teal, trai);
                cxChip += chip.Width + P(8);
            }

            // Ba hang ket qua.
            float xl = _the.X + P(20);
            float rong = _the.Width - P(40);
            Color[] mau = { Theme.Teal, Color.FromArgb(70, 120, 214), Color.FromArgb(140, 170, 228) };

            for (int i = 0; i < 3; i++)
            {
                float yy = _the.Y + P(192) + i * P(48);
                float p = _tien[i];

                if (p <= 0.001f)
                {
                    // Khung xam cho (skeleton) trong luc dang go.
                    var kh = new RectangleF(xl, yy + P(6), P(120), P(10));

                    using (GraphicsPath dp = TheBoGoc(kh, kh.Height / 2f))
                    using (var nen = new SolidBrush(Color.FromArgb(232, 238, 247)))
                        g.FillPath(nen, dp);

                    continue;
                }

                float tyLe = bo.TyLe[i];
                string muc = tyLe >= 0.7f ? "Phù hợp cao" : tyLe >= 0.45f ? "Trung bình" : "Thấp";

                Chu(g, bo.Ten[i], Theme.Chu(11F, FontStyle.Bold), new RectangleF(xl, yy, rong * 0.62f, P(22)), Theme.ChuChinh, trai);
                Chu(g, muc, Theme.Chu(9.5F), new RectangleF(xl + rong * 0.5f, yy, rong * 0.5f, P(22)), Theme.ChuPhu, phai);

                var day = new RectangleF(xl, yy + P(28), Math.Max(P(7), rong * tyLe * p), P(7));

                using (GraphicsPath dp = TheBoGoc(day, day.Height / 2f))
                using (var nen = new SolidBrush(mau[i]))
                    g.FillPath(nen, dp);
            }

            // The nho "khi nao nen di kham": chi hien khi 3 thanh ket qua da day, noi dung theo bo trieu chung dang chon.
            if (_tien[2] >= 0.99f)
            {
                var ls = LoiKhuyen[_chon];
                RectangleF nho = _nho;
                VeThe(g, nho, P(16));

                using (var cam = new SolidBrush(Color.FromArgb(217, 119, 6)))
                    g.FillEllipse(cam, nho.X + P(16), nho.Y + P(17), P(10), P(10));

                Chu(g, ls.Hoi, Theme.Chu(10F, FontStyle.Bold), new RectangleF(nho.X + P(34), nho.Y + P(10), nho.Width - P(44), P(24)), Theme.Navy, trai);

                TextRenderer.DrawText(
                    g, ls.Dap, Theme.Chu(9F),
                    new Rectangle((int)nho.X + P(34), (int)nho.Y + P(36), (int)nho.Width - P(46), P(38)),
                    Theme.ChuPhu, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bo.Stop();
                _bo.Dispose();
                _boEcg.Stop();
                _boEcg.Dispose();

                if (_ecgGiuCho)
                {
                    GioiHanHoatAnh.Tra();
                    _ecgGiuCho = false;
                }

                if (_giuCho)
                {
                    GioiHanHoatAnh.Tra();
                    _giuCho = false;
                }

                _nen?.Dispose();
                _nen = null;
            }

            base.Dispose(disposing);
        }
    }
}
