using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Bieu tuong "trai tim dap" cho goc phai tren cua bia chao (v44): dia mint nhat + trai tim xanh ngoc doi mau nhe,
    /// co duong dien tim trang di qua, kieu logo y te trong anh mau. Chi ve bang GDI+ (khong anh, khong Bitmap, khong Timer).
    /// Lop chi cung cap HAM VE; nhip dap do lop bia (LopBiaChao) tinh tu Timer dien tim co san, nen khong them Timer nao.
    /// </summary>
    public static class TimDap
    {
        public const float ChuKy = 0.86f;   // giay moi nhip (~70 nhip/phut, nhip nghi nhu nguoi khoe manh)
        public const int SoNhip = 4;        // dap 4 nhip roi nghi (khong dap vo han: giu CPU = 0 luc dung yen)

        private static readonly Color DiaNhat = Color.FromArgb(228, 245, 240);
        private static readonly Color TimTren = Color.FromArgb(58, 205, 166);
        private static readonly Color TimDuoi = Color.FromArgb(14, 122, 140);
        private static readonly Color Vanh = Color.FromArgb(24, 160, 130);

        // Diem dien tim theo don vi hop trai tim: (x, y) voi y = 0.5 la duong nen.
        private static readonly float[] Song =
        {
            -0.04f, 0.50f,
            0.18f, 0.50f,
            0.26f, 0.44f,
            0.33f, 0.50f,
            0.40f, 0.50f,
            0.45f, 0.64f,
            0.52f, 0.22f,
            0.58f, 0.74f,
            0.64f, 0.50f,
            0.72f, 0.50f,
            0.80f, 0.43f,
            0.88f, 0.50f,
            1.04f, 0.50f
        };

        /// <summary>Do dap 0..1 tai thoi gian t (giay) ke tu luc bat dau: "thinh-thich" moi chu ky, het SoNhip nhip thi bang 0.</summary>
        public static float Nhip(double t)
        {
            if (t < 0 || t >= ChuKy * SoNhip)
                return 0f;

            double u = (t % ChuKy) / ChuKy;
            double thinh = Math.Exp(-Math.Pow((u - 0.05) / 0.055, 2.0));
            double thich = 0.6 * Math.Exp(-Math.Pow((u - 0.25) / 0.06, 2.0));
            return (float)Math.Max(thinh, thich);
        }

        /// <summary>Tien do 0..1 cua vong song lan toa sau tieng "thinh" cua nhip hien tai (0 neu khong co).</summary>
        public static float VongSong(double t)
        {
            if (t < 0 || t >= ChuKy * SoNhip)
                return 0f;

            double u = (t % ChuKy) / ChuKy;
            return u < 0.7 ? (float)(u / 0.7) : 0f;
        }

        /// <summary>
        /// Ve bieu tuong trong hinh vuong o. nhip (0..1) phong trai tim toi da 12%; song (0..1) la vong lan toa (0 = khong ve).
        /// Vong lan toa nam trong o mo rong them 20% moi ben, nen vung ve lai phai bao ca phan do.
        /// </summary>
        public static void Ve(Graphics g, RectangleF o, float nhip, float song)
        {
            SmoothingMode cu = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cx = o.X + o.Width / 2f;
            float cy = o.Y + o.Height / 2f;

            // Dia nen mint nhat.
            using (SolidBrush dia = new SolidBrush(DiaNhat))
                g.FillEllipse(dia, o);

            // Vong song lan toa (mo dan): chi ve khi dang dap.
            if (song > 0f && song < 1f)
            {
                float r = o.Width / 2f * (1f + 0.20f * (1f - (1f - song) * (1f - song)));
                using (Pen but = new Pen(Color.FromArgb((int)(120 * (1f - song)), Vanh), Math.Max(1.2f, o.Width * 0.025f)))
                    g.DrawEllipse(but, cx - r, cy - r, r * 2f, r * 2f);
            }

            // Trai tim.
            float s = 1f + 0.12f * nhip;
            float w = o.Width * 0.62f * s;
            float h = o.Height * 0.56f * s;
            RectangleF hop = new RectangleF(cx - w / 2f, cy - h / 2f + o.Height * 0.02f, w, h);

            using (GraphicsPath tim = DuongTim(hop))
            {
                using (LinearGradientBrush nen = new LinearGradientBrush(
                    new PointF(hop.X, hop.Y), new PointF(hop.X, hop.Bottom), TimTren, TimDuoi))
                    g.FillPath(nen, tim);

                GraphicsState st = g.Save();
                g.SetClip(tim, CombineMode.Intersect);

                // Duong dien tim trang; luc dap co them quang mem phia sau.
                PointF[] diem = new PointF[Song.Length / 2];
                for (int i = 0; i < diem.Length; i++)
                    diem[i] = new PointF(hop.X + Song[i * 2] * hop.Width, hop.Y + Song[i * 2 + 1] * hop.Height);

                float rong = Math.Max(1.3f, hop.Width * 0.075f);

                if (nhip > 0.02f)
                    using (Pen quang = Tao(Color.FromArgb((int)(90 * nhip), 255, 255, 255), rong * 2.6f))
                        g.DrawLines(quang, diem);

                using (Pen trang = Tao(Color.White, rong))
                    g.DrawLines(trang, diem);

                // Vet sang nho o thuy trai (tao chieu sau, rat nhe).
                using (Pen sang = Tao(Color.FromArgb(70, 255, 255, 255), Math.Max(1f, hop.Width * 0.05f)))
                    g.DrawArc(sang, hop.X + hop.Width * 0.10f, hop.Y + hop.Height * 0.10f, hop.Width * 0.26f, hop.Height * 0.26f, 200f, 70f);

                g.Restore(st);
            }

            g.SmoothingMode = cu;
        }

        private static Pen Tao(Color mau, float rong)
        {
            Pen b = new Pen(mau, rong);
            b.StartCap = LineCap.Round;
            b.EndCap = LineCap.Round;
            b.LineJoin = LineJoin.Round;
            return b;
        }

        // Duong vien trai tim (6 doan Bezier). Nguoi goi phai Dispose.
        private static GraphicsPath DuongTim(RectangleF r)
        {
            GraphicsPath p = new GraphicsPath();
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;

            p.AddBezier(x + w * 0.50f, y + h * 1.00f, x + w * 0.12f, y + h * 0.72f, x + w * 0.00f, y + h * 0.50f, x + w * 0.00f, y + h * 0.30f);
            p.AddBezier(x + w * 0.00f, y + h * 0.30f, x + w * 0.00f, y + h * 0.10f, x + w * 0.15f, y + h * 0.00f, x + w * 0.29f, y + h * 0.00f);
            p.AddBezier(x + w * 0.29f, y + h * 0.00f, x + w * 0.40f, y + h * 0.00f, x + w * 0.47f, y + h * 0.07f, x + w * 0.50f, y + h * 0.17f);
            p.AddBezier(x + w * 0.50f, y + h * 0.17f, x + w * 0.53f, y + h * 0.07f, x + w * 0.60f, y + h * 0.00f, x + w * 0.71f, y + h * 0.00f);
            p.AddBezier(x + w * 0.71f, y + h * 0.00f, x + w * 0.85f, y + h * 0.00f, x + w * 1.00f, y + h * 0.10f, x + w * 1.00f, y + h * 0.30f);
            p.AddBezier(x + w * 1.00f, y + h * 0.30f, x + w * 1.00f, y + h * 0.50f, x + w * 0.88f, y + h * 0.72f, x + w * 0.50f, y + h * 1.00f);
            p.CloseFigure();
            return p;
        }
    }
}
