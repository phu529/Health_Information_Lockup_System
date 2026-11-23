using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Bo Design Token Ban 8 (toi gian kieu Cleveland Clinic / tai lieu y khoa, sach se, khong mau me):
    //  - Font Segoe UI gon gang, de doc, ty le can doi
    //  - Bang mau tiet che: Chu den than (#1E293B), chu phu xam (#475569), 1 mau nhan Navy (#1F4B82), nen trang/xanh nhat.
    internal static class AppTheme
    {
        public static readonly bool EmailBatBuoc = true;
        public static readonly bool BatHopThoaiQuenMatKhau = true;
        public static readonly bool YeuCauDongYDieuKhoan = true;
        public static readonly bool CheDoDemoMail = false;

        // ---- Nen & be mat ----
        public static readonly Color NenTrang = Color.White;
        public static readonly Color Nen = Color.FromArgb(248, 250, 252);         // #F8FAFC
        public static readonly Color NenXanh = Color.FromArgb(238, 244, 251);     // #EEF4FB
        public static readonly Color NenGradTren = Color.FromArgb(246, 250, 255); // #F6FAFF
        public static readonly Color NenGradDuoi = Color.FromArgb(231, 241, 252); // #E7F1FC
        public static readonly Color NenSong = Color.FromArgb(228, 239, 252);
        public static readonly Color NenHover = Color.FromArgb(241, 245, 249);
        public static readonly Color NenChip = Color.FromArgb(235, 242, 250);
        public static readonly Color NenChipHover = Color.FromArgb(221, 233, 247);
        public static readonly Color ChipXanh = Color.FromArgb(235, 243, 252);    // #EBF3FC
        public static readonly Color ChipMint = Color.FromArgb(232, 246, 243);    // #E8F6F3
        public static readonly Color ChipCam = Color.FromArgb(255, 243, 230);

        // ---- Vien ----
        public static readonly Color Vien = Color.FromArgb(222, 229, 238);        // #DEE5EE
        public static readonly Color VienO = Color.FromArgb(196, 208, 222);
        public static readonly Color VienDam = Color.FromArgb(156, 173, 194);

        // ---- Chu (den than tu nhien, de doc) ----
        public static readonly Color TieuDeMau = Color.FromArgb(24, 38, 58);      // #18263A
        public static readonly Color Chu = Color.FromArgb(30, 41, 59);            // #1E293B
        public static readonly Color ChuPhu = Color.FromArgb(71, 85, 105);        // #475569
        public static readonly Color ChuMo = Color.FromArgb(148, 163, 184);       // #94A3B8

        // ---- Mau nhan chinh ----
        public static readonly Color Chinh = Color.FromArgb(31, 75, 130);         // #1F4B82
        public static readonly Color ChinhSang = Color.FromArgb(43, 108, 192);    // #2B6CC0
        public static readonly Color ChinhDam = Color.FromArgb(22, 56, 99);
        public static readonly Color ChinhNhat = Color.FromArgb(150, 178, 216);
        public static readonly Color XanhPhu = Color.FromArgb(112, 163, 215);
        public static readonly Color VienOCoFocus = Color.FromArgb(88, 43, 108, 192);

        public static readonly Color Loi = Color.FromArgb(185, 28, 28);           // #B91C1C
        public static readonly Color NhanDoBatBuoc = Loi;

        // ---- Mau nhan phu & trang thai ----
        public static readonly Color Phu = Color.FromArgb(15, 118, 110);          // #0F766E
        public static readonly Color PhuDam = Color.FromArgb(13, 98, 92);
        public static readonly Color PhuNhat = Color.FromArgb(226, 245, 243);
        public static readonly Color ThanhCong = Color.FromArgb(22, 163, 74);
        public static readonly Color ManhYeu = Color.FromArgb(220, 53, 69);
        public static readonly Color Cam = Color.FromArgb(217, 119, 6);
        public static readonly Color CamDam = Color.FromArgb(180, 83, 9);
        public static readonly Color Vang = Color.FromArgb(202, 138, 4);
        public static readonly Color NenThanhManh = Color.FromArgb(226, 232, 240);

        // ---- Canh bao y khoa (dang tinh) ----
        public static readonly Color CanhBaoDo1 = Color.FromArgb(183, 28, 28);
        public static readonly Color CanhBaoDo2 = Color.FromArgb(220, 53, 69);

        // ---- Hop thong bao "che do demo" ----
        public static readonly Color DemoNen = Color.FromArgb(255, 250, 235);
        public static readonly Color DemoVien = Color.FromArgb(238, 214, 142);
        public static readonly Color DemoChu = Color.FromArgb(118, 82, 8);

        // ---- Mau minh hoa du phong ----
        public static readonly Color MH1 = Color.FromArgb(223, 236, 252);
        public static readonly Color MH2 = Color.FromArgb(188, 213, 244);
        public static readonly Color MH3 = Color.FromArgb(136, 178, 228);
        public static readonly Color MH4 = Color.FromArgb(72, 130, 202);
        public static readonly Color LaCay1 = Color.FromArgb(174, 212, 246);
        public static readonly Color LaCay2 = Color.FromArgb(142, 198, 236);
        public static readonly Color LaNgoc = Color.FromArgb(144, 186, 186);
        public static readonly Color OngNghe = Color.FromArgb(24, 62, 118);
        public static readonly Color BongDo = Color.FromArgb(22, 30, 41, 59);

        public const int BoGoc = 6;
        public const int BoGocThe = 8;

        // Font sach se, chuan he thong Windows, khong qua tron/nhua
        private static readonly string FontChinh = "Segoe UI";

        private static Font TaoFont(float co, bool semibold = false, bool bold = false)
        {
            if (semibold && !bold)
                return new Font("Segoe UI Semibold", co);
            return new Font(FontChinh, co, bold ? FontStyle.Bold : FontStyle.Regular);
        }

        public static readonly Font ChuNho = TaoFont(9.5F);
        public static readonly Font ChuNhoDam = TaoFont(9.5F, semibold: true);
        public static readonly Font ChuNhan = TaoFont(10F, semibold: true);
        public static readonly Font ChuThuong = TaoFont(10F);
        public static readonly Font ChuNhap = TaoFont(10.5F);
        public static readonly Font ChuDam = TaoFont(10.5F, semibold: true);
        public static readonly Font ChuNut = TaoFont(10F, semibold: true);
        public static readonly Font ChuTab = TaoFont(10.5F, bold: true);
        public static readonly Font ChuTabPhu = TaoFont(10.5F, semibold: true);
        public static readonly Font ChuLon = TaoFont(10.5F);
        public static readonly Font ChuMuc = TaoFont(11.5F, semibold: true);
        public static readonly Font ChuMucLon = TaoFont(12.5F, semibold: true);
        public static readonly Font ChuTheNho = TaoFont(10.5F, semibold: true);
        public static readonly Font KhauHieu = TaoFont(10.5F, semibold: true);
        public static readonly Font KhauHieuLon = TaoFont(12F, semibold: true);
        public static readonly Font KhauHieuNghieng = new Font(FontChinh, 10.5F, FontStyle.Italic | FontStyle.Bold);
        public static readonly Font TieuDe = TaoFont(20F, semibold: true);
        public static readonly Font TieuDeLon = TaoFont(24F, bold: true);
        public static readonly Font ChuCanhBao = TaoFont(10F, semibold: true);

        // Bo font doc lon, thoang, than thien cho nguoi lon tuoi (Tab Gioi thieu & Ho tro)
        public static readonly Font ChuDoc = TaoFont(11.5F);
        public static readonly Font ChuDocDam = TaoFont(11.5F, semibold: true);
        public static readonly Font ChuDocLoc = TaoFont(10.5F, semibold: true);
        public static readonly Font ChuDocMoDau = TaoFont(12F);
        public static readonly Font ChuDocMuc = TaoFont(13F, semibold: true);
        public static readonly Font ChuDocMucLon = TaoFont(14.5F, bold: true);

        public static readonly bool CoFontIcon =
            FontFamily.Families.Any(f => f.Name == "Segoe MDL2 Assets");
        public static readonly Font? FontIcon = CoFontIcon ? new Font("Segoe MDL2 Assets", 10.5F) : null;
        public static readonly Font? FontIconNho = CoFontIcon ? new Font("Segoe MDL2 Assets", 9F) : null;
        public static readonly Font? FontIconLon = CoFontIcon ? new Font("Segoe MDL2 Assets", 15F) : null;

        public const string IcNguoi = "\uE77B";
        public const string IcKhoa = "\uE72E";
        public const string IcTim = "\uE721";
        public const string IcHien = "\uE7B3";
        public const string IcAn = "\uED1A";
        public const string IcThu = "\uE715";
        public const string IcXoa = "\uE711";
        public const string IcThongTin = "\uE946";
        public const string IcQuayLai = "\uE72B";
        public const string IcTick = "\uE73E";
        public const string IcSach = "\uE82D";
        public const string IcKhien = "\uEA18";
        public const string IcTimMach = "\uEB51";
        public const string IcHoi = "\uE897";
        public const string IcMuiTen = "\uE72A";
        public const string IcCanhBao = "\uE7BA";

        public static int Px(this Control c, int v) =>
            (int)Math.Round(v * c.DeviceDpi / 96.0);

        public static Color Tron(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                a.A + (int)((b.A - a.A) * t), a.R + (int)((b.R - a.R) * t),
                a.G + (int)((b.G - a.G) * t), a.B + (int)((b.B - a.B) * t));
        }

        public static float EaseOut(float t) => EaseOutCubic(t);

        public static float EaseOutCubic(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            float u = 1f - t;
            return 1f - u * u * u;
        }

        public static float EaseInOutCubic(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static GraphicsPath DuongBoGoc(RectangleF r, float banKinh)
        {
            float d = Math.Max(1f, Math.Min(banKinh * 2f, Math.Min(r.Width, r.Height)));
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
