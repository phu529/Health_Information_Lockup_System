using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Nut bam phang, hien dai (v17, tham khao weeblab.net): hai kieu.
    ///  - Dac (KieuVien = false): nen MauNen (navy), re chuot DOI MAU sang MauRe (xanh), chu trang.
    ///  - Vien (KieuVien = true): nen trang, vien xam nhat; re chuot doi sang nen xanh nhat + vien va chu MauNen.
    /// Khi re chuot nut noi nhe 3 px, mui ten truot vao (nut rong tu 170 px). Khi bam: nut lun xuong va co vong sang.
    /// v17: DA BO duong dien tim chay qua nut. Co khung nhan phim khi dung Tab.
    /// Chi dung MOT Timer, tu dung khi het chuyen dong; tat han neu Windows tat hieu ung.
    /// </summary>
    public class NutBoGoc : Button
    {
        private const int ThoiGianRippleMs = 520;
        private const int ThoiGianNoiMs = 220;
        private const int ChoBong = 8;   // phan day nut chua bong do

        private readonly System.Windows.Forms.Timer _bo = new() { Interval = 24 };
        private long _lanCuoi;

        private bool _re;
        private bool _nhan;
        private bool _tuChuot;
        private float _noi;              // 0 = nam yen, 1 = noi len het co
        private bool _dangRipple;
        private long _batDauRipple;
        private Point _tamRipple;
        private float[] _sacChu = Array.Empty<float>();   // v31: do "len mau" cua tung chu (0..1), kieu Coolors
        private float[] _tamChu = Array.Empty<float>();   // toa do giua cua tung chu (do khi ve), de tinh khoang cach toi con tro
        private float _tamChuY;
        // v34: dem be rong cong don cua tung chu (chi do lai khi doi chu, phong hoac DPI) -> khong do lai moi khung hinh.
        private int[] _truoc = Array.Empty<int>();
        private string _truocChuoi = string.Empty;
        private Font? _truocPhong;
        private int _truocDpi;
        private float _quet = 1f;                         // v31: vet sang quet qua nut dac khi re chuot vao (1 = xong)
        private long _batDauQuet;
        private long _batDauSong;                         // v32: song mau chay qua tung chu mot lan khi re chuot vao
        private Point _chuot;            // v30: vi tri con tro, de ve vung sang chay theo chuot (spotlight)

        /// <summary>Bieu tuong ve ben trai chu: "G" = Google, "V" = VssID. Gan trong code, khong ghi vao Designer.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? KyHieu { get; set; }

        // Cac thuoc tinh nay duoc Designer.cs gan, nen phai khai bao la se duoc ghi ra ma thiet ke (Visible).
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color MauNen { get; set; } = Theme.Teal;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color MauChu { get; set; } = Color.White;

        // Mau nen khi re chuot cua nut dac. Color.Empty = tu tinh (sang hon MauNen mot chut).
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color MauRe { get; set; } = Color.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool KieuVien { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int BanKinh { get; set; } = 6;

        public NutBoGoc()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = Theme.Chu(13F, FontStyle.Bold);

            _bo.Tick += (s, e) => Nhip();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _bo.Dispose();

            base.Dispose(disposing);
        }

        // ----- Chuyen dong -----

        private void ChayNhip()
        {
            if (IsDisposed)
                return;

            if (!HoatAnh.DuocPhep)
            {
                _noi = (_re && Enabled) ? 1f : 0f;
                _dangRipple = false;
                Invalidate();
                return;
            }

            if (!_bo.Enabled)
            {
                _lanCuoi = Stopwatch.GetTimestamp();
                _bo.Start();
            }
        }

        private float TienDoRipple() =>
            (float)(Stopwatch.GetElapsedTime(_batDauRipple).TotalMilliseconds / ThoiGianRippleMs);

        private void Nhip()
        {
            if (IsDisposed)
            {
                _bo.Stop();
                return;
            }

            long bayGio = Stopwatch.GetTimestamp();
            float dt = (float)Stopwatch.GetElapsedTime(_lanCuoi, bayGio).TotalMilliseconds;
            _lanCuoi = bayGio;

            bool conChay = false;

            float dich = (_re && Enabled) ? 1f : 0f;

            if (_noi != dich)
            {
                float buoc = dt / ThoiGianNoiMs;
                _noi = _noi < dich ? Math.Min(dich, _noi + buoc) : Math.Max(dich, _noi - buoc);
                conChay |= _noi != dich;
            }

            // v31: chu doi mau theo con tro (kieu Coolors): chu nao gan con tro thi len mau cau vong, roi roi ra thi nhat dan.
            if (Enabled && !string.IsNullOrEmpty(Text))
            {
                if (_sacChu.Length != Text.Length)
                    _sacChu = new float[Text.Length];

                Point con = _re ? PointToClient(MousePosition) : Point.Empty;
                float ban = 58f * DeviceDpi / 96f;
                bool conSang = _re;

                for (int i = 0; i < _sacChu.Length; i++)
                {
                    float muc = 0f;

                    if (_re && _tamChu.Length == _sacChu.Length && !char.IsWhiteSpace(Text[i]))
                    {
                        float dx = con.X - _tamChu[i];
                        float dy = (con.Y - _tamChuY) * 0.6f;
                        float d = (float)Math.Sqrt(dx * dx + dy * dy);
                        float x = Math.Clamp(1f - d / ban, 0f, 1f);
                        muc = x * x * (3f - 2f * x);
                    }

                    // v32: khi vua re chuot vao, mot "song mau" quet qua tung chu tu trai sang phai (moi chu lech 38 ms),
                    // sau do chu chi sang theo con tro nhu cu (hieu ung chu cua coolors.co).
                    if (_re && HoatAnh.DuocPhep && !char.IsWhiteSpace(Text[i]))
                    {
                        float ms = (float)Stopwatch.GetElapsedTime(_batDauSong).TotalMilliseconds;
                        float pp = (ms - i * 38f) / 240f;

                        if (pp > 0f && pp < 1f)
                            muc = Math.Max(muc, (float)Math.Sin(Math.PI * pp) * 0.9f);

                        if (ms < i * 38f + 240f)
                            conSang = true;
                    }

                    float hienTai = _sacChu[i];
                    _sacChu[i] = muc > hienTai
                        ? Math.Min(muc, hienTai + dt / 100f)
                        : Math.Max(muc, hienTai - dt / 560f);

                    if (_sacChu[i] > 0.002f)
                        conSang = true;
                }

                conChay |= conSang;
            }
            else if (_sacChu.Length > 0)
            {
                Array.Clear(_sacChu, 0, _sacChu.Length);
            }

            if (_quet < 1f)
            {
                _quet = Math.Min(1f, (float)(Stopwatch.GetElapsedTime(_batDauQuet).TotalMilliseconds / 700.0));
                conChay = true;
            }

            if (_dangRipple)
            {
                if (TienDoRipple() >= 1f)
                    _dangRipple = false;
                else
                    conChay = true;
            }

            Invalidate();

            if (!conChay)
                _bo.Stop();
        }

        private void BatDauRipple(Point tam)
        {
            if (!HoatAnh.DuocPhep || !Enabled)
                return;

            _tamRipple = tam;
            _batDauRipple = Stopwatch.GetTimestamp();
            _dangRipple = true;
            ChayNhip();
        }

        // ----- Ve -----

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Parent is INenChoCon cha)
                cha.VeNenChoCon(g, Bounds);
            else
                g.Clear(Parent?.BackColor ?? Color.White);

            int caoThan = Height - ChoBong - 3;

            if (caoThan < 8 || Width < 16)
                return;

            float px = DeviceDpi / 96f;

            // Nut nang len toi da 3px khi re chuot, lun xuong 1px khi bam
            float nang = !Enabled ? 0f : _nhan ? -1f : _noi * 2f;
            var than = new Rectangle(1, 3 - (int)Math.Round(nang), Width - 3, caoThan);
            int banKinh = than.Height / 2;   // v31: bo tron hoan toan (kieu vien thuoc)

            VeBong(g, than, banKinh, nang);

            Color chu;
            float k = !Enabled ? 0f : _nhan ? 1f : _noi;

            using (GraphicsPath duong = Theme.DuongBoGoc(than, banKinh))
            {
                if (!Enabled)
                {
                    using (var co = new SolidBrush(KieuVien ? Theme.NenTrang : Theme.XamNhat))
                    using (var but = new Pen(KieuVien ? Theme.XamVien : Theme.XamNhat, 1.5f))
                    {
                        g.FillPath(co, duong);
                        g.DrawPath(but, duong);
                    }

                    chu = Theme.XamChu;
                }
                else if (KieuVien)
                {
                    // v30: nut vien kieu "ghost" hien dai: nen trang tinh, vien mong xam xanh; re chuot -> vien doi sang
                    // MauNen, nen anh xanh nhat va co vung sang nhe chay theo con tro.
                    Color nenRe = _nhan ? Theme.TealNhat2 : Theme.TealNhat;
                    Color nenTren = Theme.Tron(Color.White, nenRe, k);
                    Color nenDuoi = Theme.Tron(Color.FromArgb(247, 250, 254), nenRe, k);
                    Color vien = Theme.Tron(Color.FromArgb(208, 217, 232), MauNen, k * 0.85f);

                    using (var co = new LinearGradientBrush(than, nenTren, nenDuoi, LinearGradientMode.Vertical))
                    using (var but = new Pen(vien, 1.25f))
                    {
                        g.FillPath(co, duong);
                        VeSpotlight(g, duong, Theme.Teal, 40);
                        g.DrawPath(but, duong);
                    }

                    chu = Theme.Tron(Theme.Navy, MauNen, k);
                }
                else
                {
                    // v30: nut dac: nen navy chuyen mau doc, vien "kinh" sang o tren mat dan xuong duoi,
                    // re chuot -> xanh hon + vung sang chay theo con tro.
                    Color mauRe = MauRe.IsEmpty ? Theme.Tron(MauNen, Color.White, 0.14f) : MauRe;
                    Color nen = Theme.Tron(MauNen, mauRe, _noi);

                    if (_nhan)
                        nen = Theme.Tron(nen, Color.Black, 0.14f);

                    using (var co = new LinearGradientBrush(
                        than, Theme.Tron(nen, Color.White, 0.14f), Theme.Tron(nen, Color.Black, 0.12f), LinearGradientMode.Vertical))
                    {
                        g.FillPath(co, duong);
                    }

                    VeSpotlight(g, duong, Color.White, 64);

                    // v31: vet sang cheo quet qua nut dac mot lan khi re chuot vao.
                    if (_quet < 1f)
                    {
                        float rongBang = than.Width * 0.30f;
                        float xq = than.X - rongBang + (than.Width + 2f * rongBang) * HoatAnh.RaCham(_quet);
                        float nghieng = than.Height * 0.5f;
                        GraphicsState trangThaiQuet = g.Save();
                        g.SetClip(duong, CombineMode.Intersect);

                        using (var coQuet = new LinearGradientBrush(
                            new RectangleF(xq - nghieng, than.Y, rongBang + 2f * nghieng, than.Height),
                            Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal))
                        {
                            coQuet.InterpolationColors = new ColorBlend(3)
                            {
                                Colors = new[] { Color.FromArgb(0, 255, 255, 255), Color.FromArgb(70, 255, 255, 255), Color.FromArgb(0, 255, 255, 255) },
                                Positions = new[] { 0f, 0.5f, 1f }
                            };

                            g.FillPolygon(coQuet, new[]
                            {
                                new PointF(xq, than.Y), new PointF(xq + rongBang, than.Y),
                                new PointF(xq + rongBang - nghieng, than.Bottom), new PointF(xq - nghieng, than.Bottom)
                            });
                        }

                        g.Restore(trangThaiQuet);
                    }

                    using (var kinh = new LinearGradientBrush(
                        than, Color.FromArgb(_nhan ? 40 : 125, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
                    using (var viKinh = new Pen(kinh, 1.2f))
                    {
                        g.DrawPath(viKinh, duong);
                    }

                    chu = MauChu;
                }

                if (_dangRipple)
                    VeRipple(g, duong);

                if (Focused && ShowFocusCues)
                {
                    var trong = Rectangle.Inflate(than, -5, -5);

                    using (GraphicsPath dg = Theme.DuongBoGoc(trong, Math.Max(2, banKinh - 5)))
                    using (var butFocus = new Pen(KieuVien ? Theme.Navy : MauChu, 2f))
                    {
                        g.DrawPath(butFocus, dg);
                    }
                }
            }

            // v30: nut co bieu tuong (Google / VssID): bieu tuong + chu can giua cung nhau, khong co mui ten.
            if (!string.IsNullOrEmpty(KyHieu))
            {
                int icon = (int)Math.Round(24 * px);
                int khe = (int)Math.Round(10 * px);
                Size kt = TextRenderer.MeasureText(
                    g, Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                int x0 = than.X + (than.Width - (icon + khe + kt.Width)) / 2;

                VeKyHieu(g, new Rectangle(x0, than.Y + (than.Height - icon) / 2, icon, icon));

                VeChu(g, new Rectangle(x0 + icon + khe, than.Y, kt.Width + 6, than.Height), false, chu);
                return;
            }

            // Mui ten truot vao tu ben phai khi re chuot, chu nhuong sang trai mot chut
            bool coMuiTen = Enabled && Width >= (int)(170 * px);
            int doiChu = coMuiTen ? (int)(14 * px * _noi) : 0;

            if (coMuiTen && _noi > 0.02f)
            {
                float ax = than.Right - 36 * px + (1f - _noi) * 14 * px;
                float ay = than.Y + than.Height / 2f;

                using (var but = new Pen(Color.FromArgb((int)(255 * Math.Min(1f, _noi * 1.2f)), chu), 2.6f))
                {
                    but.StartCap = LineCap.Round;
                    but.EndCap = LineCap.Round;
                    but.LineJoin = LineJoin.Round;
                    g.DrawLine(but, ax, ay, ax + 15 * px, ay);
                    g.DrawLines(but, new[]
                    {
                        new PointF(ax + 9 * px, ay - 6.5f * px),
                        new PointF(ax + 15 * px, ay),
                        new PointF(ax + 9 * px, ay + 6.5f * px)
                    });
                }
            }

            VeChu(g, new Rectangle(than.X - doiChu, than.Y, than.Width, than.Height), true, chu);
        }

        // v31: ve chu theo tung ky tu de tung chu co the len mau rieng (hieu ung kieu Coolors: re chuot vao chu thi chu
        // sang mau cau vong, bo chuot ra thi mat mau dan). Vi tri tung chu = tong be rong cac chu truoc nen khop voi ve ca chuoi.
        private void VeChu(Graphics g, Rectangle vung, bool giua, Color chuNen)
        {
            string t = Text;

            if (string.IsNullOrEmpty(t))
                return;

            const TextFormatFlags dd = TextFormatFlags.NoPadding;
            int n = t.Length;

            if (!ReferenceEquals(_truocPhong, Font) || _truocChuoi != t || _truocDpi != DeviceDpi || _truoc.Length != n + 1)
            {
                var vo = new Size(int.MaxValue, int.MaxValue);
                int gach = TextRenderer.MeasureText(g, "||", Font, vo, dd).Width;
                _truoc = new int[n + 1];

                for (int i = 1; i <= n; i++)
                    _truoc[i] = TextRenderer.MeasureText(g, "|" + t.Substring(0, i) + "|", Font, vo, dd).Width - gach;

                _truocPhong = Font;
                _truocChuoi = t;
                _truocDpi = DeviceDpi;
            }

            int[] truoc = _truoc;
            int x0 = giua ? vung.X + (vung.Width - truoc[n]) / 2 : vung.X;

            if (_tamChu.Length != n)
                _tamChu = new float[n];

            _tamChuY = vung.Y + vung.Height / 2f;

            bool coMau = _re;

            for (int j = 0; !coMau && j < _sacChu.Length; j++)
                if (_sacChu[j] > 0.003f)
                    coMau = true;

            for (int i = 0; i < n; i++)
                _tamChu[i] = x0 + truoc[i] + (truoc[i + 1] - truoc[i]) / 2f;

            // v34: khong co chu nao dang len mau (luc dung yen): ve ca chuoi mot lan, nhanh hon ve tung chu.
            if (!coMau || !Enabled)
            {
                TextRenderer.DrawText(
                    g, t, Font, new Rectangle(x0, vung.Y, truoc[n] + 8, vung.Height), chuNen,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | dd);
                return;
            }

            for (int i = 0; i < n; i++)
            {
                if (char.IsWhiteSpace(t[i]))
                    continue;

                int xi = x0 + truoc[i];
                int wi = truoc[i + 1] - truoc[i];
                float muc = i < _sacChu.Length ? _sacChu[i] : 0f;
                Color mau = muc > 0.003f ? Theme.Tron(chuNen, MauCauVong(i, n), muc) : chuNen;

                TextRenderer.DrawText(
                    g, t[i].ToString(), Font, new Rectangle(xi, vung.Y, wi + 8, vung.Height), mau,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | dd);
            }
        }

        // Mau cau vong theo vi tri chu trong tu (xanh ngoc -> xanh duong -> tim -> hong), do sang tuy nen sang hay toi.
        private Color MauCauVong(int i, int n)
        {
            // v32: bang mau lay theo coolors.co (xanh troi -> chàm -> tim -> hong -> do -> cam -> vang -> xanh la).
            return PhoMau.Lay(n <= 1 ? 0f : i / (float)(n - 1), !KieuVien);
        }

        // Vong tron sang toa ra tu diem bam (bi mat o v32, bo sung lai tu v28).
        private void VeRipple(Graphics g, GraphicsPath duong)
        {
            float t = Math.Min(1f, TienDoRipple());
            float r = (float)Math.Sqrt((double)Width * Width + (double)Height * Height) * HoatAnh.RaCham(t);
            int alpha = (int)((KieuVien ? 60 : 90) * (1f - t));

            if (alpha <= 0 || r < 1f)
                return;

            GraphicsState trangThai = g.Save();
            g.SetClip(duong, CombineMode.Intersect);

            using (var co = new SolidBrush(Color.FromArgb(alpha, KieuVien ? Theme.Teal : Color.White)))
            {
                g.FillEllipse(co, _tamRipple.X - r, _tamRipple.Y - r, r * 2f, r * 2f);
            }

            g.Restore(trangThai);
        }

        // v30: bong khuech tan phang (khong con "bang nhua" duoi day nut): nhieu lop mo, neo o mat dat, dam len khi re chuot.
        private void VeBong(Graphics g, Rectangle than, int banKinh, float nang)
        {
            if (!Enabled)
                return;

            float dam = KieuVien ? 0.5f : 1f;
            Color mauBong = Theme.Tron(Color.FromArgb(10, 28, 70), Theme.Teal, _noi * 0.7f);

            for (int k = 3; k >= 0; k--)
            {
                var r = new Rectangle(
                    than.X + 2 + k,
                    5 + k,
                    Math.Max(8, than.Width - 4 - 2 * k),
                    than.Height);

                int alpha = (int)((7 + 15 * _noi) * dam * (1f - k / 4f));

                if (alpha <= 0)
                    continue;

                using (GraphicsPath duong = Theme.DuongBoGoc(r, Math.Min(banKinh, r.Height / 2)))
                using (var co = new SolidBrush(Color.FromArgb(alpha, mauBong)))
                {
                    g.FillPath(co, duong);
                }
            }
        }

        // Vung sang mem chay theo con tro (kieu "spotlight" cua cac trang web hien dai), cat theo hinh nut.
        private void VeSpotlight(Graphics g, GraphicsPath duong, Color mau, int alphaToiDa)
        {
            if (_noi < 0.02f || !Enabled)
                return;

            float r = Width * 0.5f;
            GraphicsState trangThai = g.Save();
            g.SetClip(duong, CombineMode.Intersect);

            using (var vong = new GraphicsPath())
            {
                vong.AddEllipse(_chuot.X - r, _chuot.Y - r, r * 2f, r * 2f);

                using (var cop = new PathGradientBrush(vong))
                {
                    cop.CenterColor = Color.FromArgb((int)(alphaToiDa * _noi), mau);
                    cop.SurroundColors = new[] { Color.FromArgb(0, mau) };
                    g.FillPath(cop, vong);
                }
            }

            g.Restore(trangThai);
        }

        private void VeKyHieu(Graphics g, Rectangle o)
        {
            float cx = o.X + o.Width / 2f;
            float cy = o.Y + o.Height / 2f;
            float px = DeviceDpi / 96f;

            if (KyHieu == "G")
            {
                float ban = o.Width * 0.38f;
                float day = o.Width * 0.17f;
                var khung = new RectangleF(cx - ban, cy - ban, ban * 2f, ban * 2f);

                void Cung(Color mau, float batDau, float quet)
                {
                    using (var but = new Pen(mau, day))
                    {
                        g.DrawArc(but, khung, batDau, quet);
                    }
                }

                Cung(Color.FromArgb(234, 67, 53), 200f, 120f);
                Cung(Color.FromArgb(251, 188, 5), 135f, 65f);
                Cung(Color.FromArgb(52, 168, 83), 45f, 90f);
                Cung(Color.FromArgb(66, 133, 244), 0f, 45f);

                using (var thanh = new Pen(Color.FromArgb(66, 133, 244), day))
                {
                    g.DrawLine(thanh, cx, cy, cx + ban + day / 2f, cy);
                }
            }
            else if (KyHieu == "V")
            {
                // v32: bieu tuong VssID / BHXH Viet Nam ve lai chinh xac hon (xem UI/BieuTuongVssId.cs).
                BieuTuongVssId.Ve(g, o);
            }
        }

        // ----- Su kien -----

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            _chuot = mevent.Location;

            if (_re)
                Invalidate();

            base.OnMouseMove(mevent);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _re = true;
            _quet = 0f;
            _batDauQuet = Stopwatch.GetTimestamp();
            _batDauSong = Stopwatch.GetTimestamp();
            ChayNhip();
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _re = false;
            _nhan = false;
            _tuChuot = false;
            ChayNhip();
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _nhan = true;
            _tuChuot = true;

            BatDauRipple(mevent.Location);
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _nhan = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnClick(EventArgs e)
        {
            // Bam bang phim (Enter, Space, AcceptButton) khong co toa do chuot: toa tu tam nut.
            if (!_tuChuot)
                BatDauRipple(new Point(Width / 2, Height / 2));

            _tuChuot = false;
            base.OnClick(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            ChayNhip();
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }
    }
}
