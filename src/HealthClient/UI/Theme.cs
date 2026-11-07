using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Bo mau, co chu va ham ve dung chung cho moi form.
    /// Thiet ke uu tien nguoi cao tuoi va tre nho: chu lon, tuong phan cao,
    /// nut to, goc bo mem, khong dung mau la dau hieu duy nhat.
    /// </summary>
    public static class Theme
    {
        // BANG MAU v11 (y te hien dai, tham khao MB Bank): xanh duong #1747C4, xanh ngoc nhat (cyan), trang.
        // Ten bien (Teal...) giu nguyen de khong phai sua code cac form; gia tri nay la XANH DUONG CHINH.
        // Tuong phan: chu trang tren xanh #1747C4 ~7,7:1; chu navy #0B1F33 tren nen trang ~16:1.
        public static readonly Color Teal = Color.FromArgb(23, 71, 196);
        public static readonly Color TealDam = Color.FromArgb(12, 38, 128);
        public static readonly Color TealNhat = Color.FromArgb(232, 240, 253);
        public static readonly Color TealNhat2 = Color.FromArgb(205, 222, 248);
        // Hong san ho: chi dung cho trai tim va diem nhan am ap, tao cam giac than thien
        public static readonly Color HongTim = Color.FromArgb(232, 92, 112);
        public static readonly Color HongTimDam = Color.FromArgb(196, 52, 84);
        public static readonly Color Navy = Color.FromArgb(11, 31, 51);

        // Vang dong #C6A15B: nen nut chinh, duong gach, vien. Chu tren nut vang luon la navy.
        public static readonly Color Vang = Color.FromArgb(198, 161, 91);
        public static readonly Color VangSang = Color.FromArgb(226, 196, 128);

        // Xanh ngoc nhat (cyan) kieu MB: duong dien tim, diem nhan tren nen xanh dam
        public static readonly Color Cyan = Color.FromArgb(106, 213, 255);

        // Chu: tren nen trang deu dat ty le tuong phan tu 4.5:1 tro len
        public static readonly Color ChuChinh = Color.FromArgb(31, 41, 55);
        public static readonly Color ChuPhu = Color.FromArgb(75, 85, 99);
        public static readonly Color Loi = Color.FromArgb(180, 35, 24);

        // O nhap, vien, nut khoa
        public static readonly Color NenONhap = Color.FromArgb(255, 255, 255);
        // Nen kem #F7F3EA (thay cho trang tinh): diu mat hon, dem lai cam giac sang trong
        public static readonly Color NenTrang = Color.FromArgb(250, 252, 255);
        // Vang dam: dung tren nen kem (thanh tab, duong ke), tuong phan ~3.6:1 du cho thanh chi bao
        public static readonly Color VangDam = Color.FromArgb(160, 121, 40);
        public static readonly Color XamVien = Color.FromArgb(122, 138, 164);
        public static readonly Color XamNhat = Color.FromArgb(226, 229, 232);
        public static readonly Color XamChu = Color.FromArgb(107, 114, 128);

        private static readonly Dictionary<(float, FontStyle), Font> _chu = new();

        /// <summary>Phong chu Segoe UI, lay tu bo nho dem de khong tao lai nhieu lan.</summary>
        public static Font Chu(float pt, FontStyle kieu = FontStyle.Regular)
        {
            lock (_chu)
            {
                if (!_chu.TryGetValue((pt, kieu), out Font? f))
                {
                    f = new Font("Segoe UI", pt, kieu, GraphicsUnit.Point);
                    _chu[(pt, kieu)] = f;
                }

                return f;
            }
        }

        private static readonly Dictionary<(float, FontStyle), Font> _chuSerif = new();

        /// <summary>
        /// v13: Segoe UI dam cho TIEU DE (phong co chan Georgia kho doc nen da bo). Giu ten ham de khong phai sua cho goi.
        /// Chu noi dung, nhan, o nhap van la Segoe UI cho de doc. Georgia co san tren Windows.
        /// </summary>
        public static Font ChuSerif(float pt, FontStyle kieu = FontStyle.Bold)
        {
            lock (_chuSerif)
            {
                if (!_chuSerif.TryGetValue((pt, kieu), out Font? f))
                {
                    f = new Font("Segoe UI", pt, kieu, GraphicsUnit.Point);
                    _chuSerif[(pt, kieu)] = f;
                }

                return f;
            }
        }

        /// <summary>Hinh chu nhat bo goc, dung de ve nut, o nhap, the.</summary>
        public static GraphicsPath DuongBoGoc(Rectangle r, int banKinh)
        {
            int d = Math.Max(2, banKinh * 2);
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        /// <summary>Tron hai mau theo ty le t (0 = a, 1 = b).</summary>
        public static Color Tron(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        /// <summary>Lam toi mot mau theo ty le (0 den 1), dung cho trang thai re chuot / nhan chuot.</summary>
        public static Color LamToi(Color c, float tyLe)
        {
            float k = 1f - tyLe;
            return Color.FromArgb(c.A, (int)(c.R * k), (int)(c.G * k), (int)(c.B * k));
        }
    }
}
