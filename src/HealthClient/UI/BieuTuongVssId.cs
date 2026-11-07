using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Bieu tuong VssID / Bao hiem xa hoi Viet Nam (v32), VE LAI bang code theo logo tren trang
    /// baohiemxahoi.gov.vn: nen tron xanh; bong hoa 5 canh chi co vien trang (giua rong, thay nen xanh);
    /// hinh nguoi chu Y: hai tay cong gio len, mot cham tron lam dau, than thang xuong duoi.
    /// Day la ban ve lai de dung lam bieu tuong nut, KHONG phai tep logo chinh thuc va khong co dong chu tren vanh.
    /// Hinh duoc dung thu bang bo ve ngoai (so voi anh logo that) truoc khi chuyen sang C#.
    /// </summary>
    public static class BieuTuongVssId
    {
        public static void Ve(Graphics g, RectangleF o)
        {
            SmoothingMode cu = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float r = o.Width / 2f;
            float cx = o.X + r;
            float cy = o.Y + o.Height / 2f;

            // 1) Nen tron xanh, doi mau nhe tu tren xuong.
            using (var nen = new LinearGradientBrush(
                o, Color.FromArgb(74, 156, 216), Color.FromArgb(34, 108, 180), LinearGradientMode.Vertical))
            {
                g.FillEllipse(nen, o);
            }

            // 2) Bong hoa 5 canh: ban kinh theo goc = rf * (0,56 + 0,44 * |cos(2,5 * goc)|^0,45).
            //    Canh dau huong len; giua hai canh la mot khia nhon.
            const int diem = 150;
            var hoa = new PointF[diem];
            float rf = r * 0.80f;
            float tamHoaY = cy - r * 0.08f;

            for (int i = 0; i < diem; i++)
            {
                double goc = 2.0 * Math.PI * i / diem;
                double bk = rf * (0.56 + 0.44 * Math.Pow(Math.Abs(Math.Cos(2.5 * goc)), 0.45));
                hoa[i] = new PointF(cx + (float)(Math.Sin(goc) * bk), tamHoaY - (float)(Math.Cos(goc) * bk));
            }

            using (var vien = new Pen(Color.White, Math.Max(1.3f, r * 0.075f)))
            {
                vien.LineJoin = LineJoin.Round;
                g.DrawPolygon(vien, hoa);
            }

            // 3) Hinh nguoi: than, hai tay cong, dau.
            float nx = cx;
            float ny = cy - r * 0.02f;

            using (var than = new Pen(Color.White, Math.Max(1.4f, r * 0.13f)))
            {
                g.DrawLine(than, nx, ny, nx, cy + r * 0.74f);
            }

            using (var tay = new Pen(Color.White, Math.Max(1.3f, r * 0.12f)))
            {
                tay.StartCap = LineCap.Round;
                tay.EndCap = LineCap.Round;

                for (int dau = -1; dau <= 1; dau += 2)
                {
                    g.DrawBezier(
                        tay,
                        new PointF(nx, ny),
                        new PointF(nx + dau * r * 0.03f, ny - r * 0.16f),
                        new PointF(nx + dau * r * 0.16f, ny - r * 0.24f),
                        new PointF(nx + dau * r * 0.29f, ny - r * 0.38f));
                }
            }

            float bkDau = Math.Max(1.2f, r * 0.095f);
            g.FillEllipse(Brushes.White, cx - bkDau, cy - r * 0.26f - bkDau, bkDau * 2f, bkDau * 2f);

            g.SmoothingMode = cu;
        }
    }
}
