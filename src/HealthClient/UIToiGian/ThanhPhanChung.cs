using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    internal interface ITrang
    {
        IButtonControl? NutMacDinh { get; }
        FormField? OBatDau { get; }
    }

    internal enum KieuMinhHoa
    {
        DangNhap,
        DangKy
    }

    internal static class TaiNguyenAnh
    {
        private static readonly Dictionary<string, Image?> CacheGoc = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<(string Ten, int BanKinh), Bitmap?> CacheMo = new();

        public static Image? Lay(string tenFile)
        {
            if (CacheGoc.TryGetValue(tenFile, out Image? img)) return img;

            Assembly asm = typeof(TaiNguyenAnh).Assembly;
            string? res = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("." + tenFile, StringComparison.OrdinalIgnoreCase)
                                  || n.EndsWith(tenFile, StringComparison.OrdinalIgnoreCase));
            if (res != null)
            {
                using Stream? s = asm.GetManifestResourceStream(res);
                if (s != null)
                {
                    using var tmp = Image.FromStream(s);
                    img = new Bitmap(tmp);
                }
            }

            CacheGoc[tenFile] = img;
            return img;
        }

        public static void VeAnhMemCanh(Graphics g, Image anh, Rectangle vung, Color nenCha, int banKinhMo = 28) =>
            VeAnhMemCanh(g, null, anh, vung, banKinhMo);

        public static void VeAnhMemCanh(Graphics g, string? tenCache, Image anh, Rectangle vung, int banKinhMo = 28)
        {
            if (vung.Width < 16 || vung.Height < 16) return;

            float tiLeAnh = anh.Width / (float)anh.Height;
            float tiLeKhung = vung.Width / (float)vung.Height;
            float w, h;
            if (tiLeAnh > tiLeKhung)
            {
                w = vung.Width;
                h = w / tiLeAnh;
            }
            else
            {
                h = vung.Height;
                w = h * tiLeAnh;
            }
            var dest = new RectangleF(vung.X + (vung.Width - w) / 2f, vung.Y + (vung.Height - h) / 2f, w, h);

            if (banKinhMo <= 0)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(anh, dest);
                return;
            }

            int rSrc = (int)Math.Clamp(MathF.Round(banKinhMo * (anh.Width / Math.Max(1f, w))), 16f, Math.Min(anh.Width, anh.Height) * 0.16f);
            Image anhVe = anh;
            if (!string.IsNullOrEmpty(tenCache))
            {
                anhVe = LayHoacTaoMo(tenCache, anh, rSrc);
            }
            else
            {
                using var tam = TaoBanMo(anh, rSrc);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(tam, dest);
                return;
            }

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(anhVe, dest);
        }

        private static Bitmap LayHoacTaoMo(string tenCache, Image goc, int rSrc)
        {
            var key = (tenCache, rSrc);
            if (CacheMo.TryGetValue(key, out Bitmap? bmp) && bmp != null)
                return bmp;

            bmp = TaoBanMo(goc, rSrc);
            CacheMo[key] = bmp;
            return bmp;
        }

        private static Bitmap TaoBanMo(Image goc, int rSrc)
        {
            int w = goc.Width;
            int h = goc.Height;
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
                g.DrawImage(goc, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);

            int r = Math.Clamp(rSrc, 8, Math.Min(w, h) / 4);
            var rect = new Rectangle(0, 0, w, h);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int stride = data.Stride;
                byte[] buf = new byte[stride * h];
                Marshal.Copy(data.Scan0, buf, 0, buf.Length);

                int l = r;
                int rt = w - 1 - r;
                int t = r;
                int b = h - 1 - r;

                for (int y = 0; y < h; y++)
                {
                    int dy = y < t ? (t - y) : (y > b ? (y - b) : 0);
                    int row = y * stride;
                    bool trongDoc = dy == 0;

                    for (int x = 0; x < w; x++)
                    {
                        if (trongDoc && x == l)
                        {
                            x = rt;
                            continue;
                        }

                        int dx = x < l ? (l - x) : (x > rt ? (x - rt) : 0);
                        if (dx == 0 && dy == 0) continue;

                        float dist = (dx == 0) ? dy : (dy == 0) ? dx : MathF.Sqrt(dx * dx + dy * dy);
                        float u = Math.Clamp(1f - dist / r, 0f, 1f);
                        float k = 0.5f - 0.5f * MathF.Cos(u * MathF.PI);

                        int idx = row + x * 4 + 3;
                        buf[idx] = (byte)Math.Clamp((int)MathF.Round(buf[idx] * k), 0, 255);
                    }
                }

                Marshal.Copy(buf, 0, data.Scan0, buf.Length);
            }
            finally
            {
                bmp.UnlockBits(data);
            }
            return bmp;
        }
    }

    internal sealed class Logo : Control
    {
        public Logo()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.NenTrang);

            float w = Width - 2f;
            float h = Height - 2f;
            float ox = 1f, oy = 1f;

            using var heart = TaoDuongTraiTim(ox, oy, w, h);
            using var co = new LinearGradientBrush(new RectangleF(ox, oy, Math.Max(1f, w), Math.Max(1f, h)),
                AppTheme.ChinhSang, AppTheme.Chinh, LinearGradientMode.ForwardDiagonal);
            g.FillPath(co, heart);

            float yMid = oy + h * 0.46f;
            var pts = new[]
            {
                new PointF(ox + w * 0.14f, yMid),
                new PointF(ox + w * 0.34f, yMid),
                new PointF(ox + w * 0.42f, yMid - h * 0.19f),
                new PointF(ox + w * 0.52f, yMid + h * 0.21f),
                new PointF(ox + w * 0.62f, yMid - h * 0.12f),
                new PointF(ox + w * 0.69f, yMid),
                new PointF(ox + w * 0.86f, yMid),
            };
            using var but = new Pen(Color.White, Math.Max(1.6f, w / 15f))
            {
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            g.DrawLines(but, pts);
        }

        internal static GraphicsPath TaoDuongTraiTim(float x, float y, float w, float h)
        {
            var p = new GraphicsPath();
            p.AddBezier(
                new PointF(x + w * 0.50f, y + h * 0.90f),
                new PointF(x + w * 0.05f, y + h * 0.62f),
                new PointF(x + w * 0.00f, y + h * 0.26f),
                new PointF(x + w * 0.27f, y + h * 0.14f));
            p.AddBezier(
                new PointF(x + w * 0.27f, y + h * 0.14f),
                new PointF(x + w * 0.40f, y + h * 0.09f),
                new PointF(x + w * 0.48f, y + h * 0.20f),
                new PointF(x + w * 0.50f, y + h * 0.26f));
            p.AddBezier(
                new PointF(x + w * 0.50f, y + h * 0.26f),
                new PointF(x + w * 0.52f, y + h * 0.20f),
                new PointF(x + w * 0.60f, y + h * 0.09f),
                new PointF(x + w * 0.73f, y + h * 0.14f));
            p.AddBezier(
                new PointF(x + w * 0.73f, y + h * 0.14f),
                new PointF(x + w * 1.00f, y + h * 0.26f),
                new PointF(x + w * 0.95f, y + h * 0.62f),
                new PointF(x + w * 0.50f, y + h * 0.90f));
            p.CloseFigure();
            return p;
        }
    }

    // Cot minh hoa ben trai cua Dang nhap & Dang ky Ban 9:
    //  - Sua triet de cat net chu "quy" trong "Suc khoe la tai san quy gia nhat" (tang hCau = 40px, bo NoPadding).
    internal sealed class BrandPanel : Panel
    {
        private const string CauBatBuoc = "Sức khỏe là tài sản quý giá nhất";

        private readonly FlatButton _nutVe = new();
        private KieuMinhHoa _cheDo = KieuMinhHoa.DangNhap;
        private (LucideIconKind Icon, Color Mau, Color Nen, string Chu)[] _danhSachY = Array.Empty<(LucideIconKind, Color, Color, string)>();
        private string? _khauHieuPhu;

        public event EventHandler? QuayVeClick;

        public BrandPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = AppTheme.NenGradTren;

            _nutVe.Kieu = KieuNut.Chu;
            _nutVe.CanTrai = true;
            _nutVe.LucideIcon = LucideIconKind.ArrowLeft;
            _nutVe.Text = "Trang chủ";
            _nutVe.Font = AppTheme.ChuMuc;
            _nutVe.MauChuTuyChinh = AppTheme.Chu;
            _nutVe.Bounds = new Rectangle(this.Px(22), this.Px(14), this.Px(146), this.Px(36));
            _nutVe.Click += (_, e) => QuayVeClick?.Invoke(this, e);
            Controls.Add(_nutVe);
        }

        public void DatCheDo(
            KieuMinhHoa cheDo,
            (LucideIconKind Icon, Color Mau, Color Nen, string Chu)[]? danhSachY = null,
            string? khauHieuPhu = null)
        {
            _cheDo = cheDo;
            _danhSachY = danhSachY ?? Array.Empty<(LucideIconKind, Color, Color, string)>();
            _khauHieuPhu = khauHieuPhu;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bg = new LinearGradientBrush(
                       new Rectangle(0, 0, Math.Max(1, Width), Math.Max(1, Height)),
                       AppTheme.NenGradTren, AppTheme.NenGradDuoi, LinearGradientMode.Vertical))
                g.FillRectangle(bg, ClientRectangle);

            using (var bSong = new SolidBrush(Color.FromArgb(24, AppTheme.XanhPhu)))
                g.FillEllipse(bSong, -this.Px(90), Height - this.Px(240), Width + this.Px(180), this.Px(320));

            int pad = this.Px(18);
            int wNoiDung = Math.Max(80, Width - pad * 2);

            // Tang vung ve cau khau hieu len 40px de duoi chu "y" va dau sac chu "quy" KHONG bao gio bi cat
            int szTim = this.Px(18);
            int gapTim = this.Px(8);
            Size szCau = TextRenderer.MeasureText(g, CauBatBuoc, AppTheme.KhauHieuLon, Size.Empty, TextFormatFlags.NoPrefix);
            int hCau = Math.Max(this.Px(40), szCau.Height + this.Px(14));
            int yCau = Height - this.Px(16) - hCau;

            bool hienY = _danhSachY.Length > 0 && Height >= this.Px(480);
            int hDongY = this.Px(40);
            int khoiY = hienY ? (_danhSachY.Length * hDongY + this.Px(12)) : 0;

            bool hienPhu = !hienY && !string.IsNullOrEmpty(_khauHieuPhu) && Height >= this.Px(480);
            int hPhu = hienPhu ? this.Px(28) : 0;

            int topAnh = this.Px(56);
            int duoiAnh = yCau - khoiY - hPhu - this.Px(8);
            int hAnh = Math.Max(this.Px(150), duoiAnh - topAnh);
            var vungAnh = new Rectangle(pad, topAnh, wNoiDung, hAnh);

            string tenAnh = _cheDo == KieuMinhHoa.DangNhap ? "auth_login.png" : "auth_register.png";
            Image? anh = TaiNguyenAnh.Lay(tenAnh);
            if (anh != null)
            {
                TaiNguyenAnh.VeAnhMemCanh(g, tenAnh, anh, vungAnh, this.Px(32));
            }
            else
            {
                using var p = AppTheme.DuongBoGoc(vungAnh, this.Px(10));
                using var b = new SolidBrush(AppTheme.ChipXanh);
                g.FillPath(b, p);
            }

            if (hienY)
            {
                int wMaxHang = 0;
                foreach (var it in _danhSachY)
                {
                    int wIt = TextRenderer.MeasureText(g, it.Chu, AppTheme.ChuMucLon, Size.Empty, TextFormatFlags.NoPrefix).Width;
                    if (wIt > wMaxHang) wMaxHang = wIt;
                }
                int szIc = this.Px(20);
                int gapIc = this.Px(12);
                int wKhoi = Math.Min(wNoiDung, szIc + gapIc + wMaxHang + this.Px(8));
                int xKhoi = pad + Math.Max(0, (wNoiDung - wKhoi) / 2);
                int y = duoiAnh + this.Px(4);

                foreach (var it in _danhSachY)
                {
                    LucideIcons.Draw(g, it.Icon,
                        new RectangleF(xKhoi, y + (hDongY - szIc) / 2f, szIc, szIc),
                        it.Mau, 1.85f);

                    TextRenderer.DrawText(g, it.Chu, AppTheme.ChuMucLon,
                        new Rectangle(xKhoi + szIc + gapIc, y, Math.Max(40, wKhoi - szIc - gapIc), hDongY),
                        AppTheme.TieuDeMau,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

                    y += hDongY;
                }
            }
            else if (hienPhu && _khauHieuPhu != null)
            {
                TextRenderer.DrawText(g, _khauHieuPhu, AppTheme.ChuNhoDam,
                    new Rectangle(pad, duoiAnh + this.Px(4), wNoiDung, hPhu),
                    AppTheme.ChuPhu,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            int tongCau = Math.Min(wNoiDung, szTim + gapTim + szCau.Width);
            int xCau = pad + Math.Max(0, (wNoiDung - tongCau) / 2);
            LucideIcons.Draw(g, LucideIconKind.HeartPulse,
                new RectangleF(xCau, yCau + (hCau - szTim) / 2f, szTim, szTim), AppTheme.Chinh, 1.85f);
            TextRenderer.DrawText(g, CauBatBuoc, AppTheme.KhauHieuLon,
                new Rectangle(xCau + szTim + gapTim, yCau, Math.Max(40, wNoiDung - (szTim + gapTim)), hCau),
                AppTheme.Chinh,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }
    }

    // =========================================================================
    //  ChuThoang & DoanChuThoang:
    //   - Tu dong ngat dong theo tu, khong dung NoPadding de ky tu thoang, khong dinh nhau
    //   - Ho tro gian dong (line-spacing) va gian doan (paragraph-spacing) ro rang cho nguoi lon tuoi
    // =========================================================================
    internal static class ChuThoang
    {
        private const TextFormatFlags CoDo = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;

        public static List<(string Dong, bool HetDoan)> TachDong(string? vanBan, Font font, int chieuRong)
        {
            var ketQua = new List<(string, bool)>();
            if (string.IsNullOrEmpty(vanBan)) return ketQua;

            int wMax = Math.Max(40, chieuRong);
            string[] cacDoan = vanBan.Replace("\r\n", "\n").Split('\n');

            for (int p = 0; p < cacDoan.Length; p++)
            {
                string doan = cacDoan[p];
                if (string.IsNullOrWhiteSpace(doan))
                {
                    ketQua.Add((string.Empty, true));
                    continue;
                }

                // Giu thut dau dong neu la gach dau dong "• "
                bool coCham = doan.StartsWith("• ");
                string[] tu = doan.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string dongHienTai = string.Empty;

                for (int i = 0; i < tu.Length; i++)
                {
                    string thu = dongHienTai.Length == 0 ? tu[i] : dongHienTai + " " + tu[i];
                    int wThu = TextRenderer.MeasureText(thu, font, new Size(int.MaxValue, int.MaxValue), CoDo).Width;
                    if (wThu <= wMax || dongHienTai.Length == 0)
                    {
                        dongHienTai = thu;
                    }
                    else
                    {
                        ketQua.Add((dongHienTai, false));
                        dongHienTai = coCham ? "   " + tu[i] : tu[i];
                    }
                }

                if (dongHienTai.Length > 0)
                    ketQua.Add((dongHienTai, p < cacDoan.Length - 1));
            }
            return ketQua;
        }

        public static int DoCao(string? vanBan, Font font, int chieuRong, int gianDongPx, int gianDoanPx)
        {
            var cacDong = TachDong(vanBan, font, chieuRong);
            if (cacDong.Count == 0) return 0;

            int hMotDong = TextRenderer.MeasureText("Ag", font, new Size(int.MaxValue, int.MaxValue), CoDo).Height;
            int tong = 0;
            for (int i = 0; i < cacDong.Count; i++)
            {
                tong += hMotDong;
                if (i < cacDong.Count - 1)
                    tong += gianDongPx + (cacDong[i].HetDoan ? gianDoanPx : 0);
            }
            return tong + 6;
        }

        public static void Ve(Graphics g, string? vanBan, Font font, Rectangle vung, Color mau, int gianDongPx, int gianDoanPx)
        {
            var cacDong = TachDong(vanBan, font, vung.Width);
            if (cacDong.Count == 0) return;

            int hMotDong = TextRenderer.MeasureText(g, "Ag", font, new Size(int.MaxValue, int.MaxValue), CoDo).Height;
            int y = vung.Y;
            for (int i = 0; i < cacDong.Count; i++)
            {
                if (y > vung.Bottom) break;
                if (cacDong[i].Dong.Length > 0)
                {
                    TextRenderer.DrawText(g, cacDong[i].Dong, font,
                        new Rectangle(vung.X, y, vung.Width + 6, hMotDong + 4), mau, CoDo);
                }
                y += hMotDong + gianDongPx + (cacDong[i].HetDoan ? gianDoanPx : 0);
            }
        }
    }

    internal sealed class DoanChuThoang : Control
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int GianDongPx { get; set; } = 7;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int GianDoanPx { get; set; } = 10;

        public DoanChuThoang()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        public int TinhChieuCao(int chieuRong) =>
            ChuThoang.DoCao(Text, Font, chieuRong, GianDongPx, GianDoanPx);

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            ChuThoang.Ve(e.Graphics, Text, Font, ClientRectangle, ForeColor, GianDongPx, GianDoanPx);
        }
    }
}
