using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Thanh do manh mat khau Ban 5 (cao co dinh 52px, khong de o ke tiep):
    //  - 4 doan bo tron to dan muot trong 200ms (Stopwatch + EaseOutCubic)
    //  - Nhan doi mau: Yeu -> Trung binh -> Kha -> Manh
    //  - Ben duoi 5 chip tieu chi (8 ky tu, A-Z, a-z, 0-9, @#$): dat doi nen ChipMint + icon Check.
    internal sealed class ThanhDoManh : Control
    {
        private const int ThoiGianThanhMs = 200;

        private static readonly string[] TenTieuChi = { "8 ký tự", "A–Z", "a–z", "0–9", "@#$" };
        private static readonly string[] TenMuc = { "", "Yếu", "Trung bình", "Khá", "Mạnh" };

        private readonly bool[] _dat = new bool[5];
        private readonly System.Windows.Forms.Timer _dongHo = new() { Interval = 16 };
        private readonly Stopwatch _sw = new();
        private int _muc;
        private float _tuHien, _hien;

        public ThanhDoManh()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = this.Px(52);
            _dongHo.Tick += (_, _) =>
            {
                if (!Visible)
                {
                    _dongHo.Stop();
                    _hien = _muc;
                    return;
                }
                float t = Math.Clamp(_sw.ElapsedMilliseconds / (float)ThoiGianThanhMs, 0f, 1f);
                float k = AppTheme.EaseOutCubic(t);
                _hien = _tuHien + (_muc - _tuHien) * k;
                if (t >= 1f)
                {
                    _hien = _muc;
                    _dongHo.Stop();
                }
                Invalidate();
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dongHo.Stop();
                _dongHo.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                _dongHo.Stop();
                _hien = _muc;
            }
        }

        public void Dat(string? matKhau)
        {
            string mk = matKhau ?? string.Empty;
            _dat[0] = mk.Length >= 8;
            _dat[1] = mk.Any(char.IsUpper);
            _dat[2] = mk.Any(char.IsLower);
            _dat[3] = mk.Any(char.IsDigit);
            _dat[4] = mk.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

            int dem = mk.Length == 0 ? 0 : _dat.Count(b => b);
            int mucMoi = dem switch
            {
                0 => 0,
                1 => 1,
                2 => 2,
                3 => 3,
                _ => 4
            };
            if (mucMoi != _muc)
            {
                _muc = mucMoi;
                if (Visible && IsHandleCreated && SystemInformation.UIEffectsEnabled)
                {
                    _tuHien = _hien;
                    _sw.Restart();
                    _dongHo.Start();
                }
                else
                {
                    _hien = _muc;
                }
            }
            Invalidate();
        }

        private static Color MauMuc(int muc) => muc switch
        {
            1 => AppTheme.ManhYeu,
            2 => AppTheme.Cam,
            3 => AppTheme.Vang,
            4 => AppTheme.ThanhCong,
            _ => AppTheme.Vien
        };

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.NenTrang);

            const TextFormatFlags co = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;

            // Dong 1: Nhan + Muc do
            int h1 = this.Px(16);
            TextRenderer.DrawText(g, "Độ mạnh mật khẩu", AppTheme.ChuNho, new Rectangle(0, 0, Width / 2, h1),
                AppTheme.ChuPhu, co | TextFormatFlags.Left);
            if (_muc > 0)
                TextRenderer.DrawText(g, TenMuc[_muc], AppTheme.ChuNhoDam,
                    new Rectangle(Width / 2, 0, Width - Width / 2, h1), MauMuc(_muc), co | TextFormatFlags.Right);

            // Dong 2: Thanh 4 doan bo tron to dan
            int yThanh = this.Px(19);
            int hThanh = this.Px(6);
            int gapDoan = this.Px(5);
            float wDoan = Math.Max(8f, (Width - 1f - gapDoan * 3f) / 4f);
            Color mau = MauMuc(_muc);

            for (int i = 0; i < 4; i++)
            {
                float xD = i * (wDoan + gapDoan);
                var rD = new RectangleF(xD, yThanh, wDoan, hThanh);
                using var pD = AppTheme.DuongBoGoc(rD, hThanh / 2f);
                using (var bgD = new SolidBrush(AppTheme.NenThanhManh))
                    g.FillPath(bgD, pD);

                float fill = Math.Clamp(_hien - i, 0f, 1f);
                if (fill > 0.02f)
                {
                    GraphicsState st = g.Save();
                    g.SetClip(pD, CombineMode.Intersect);
                    using var coD = new SolidBrush(mau);
                    g.FillRectangle(coD, rD.X, rD.Y, rD.Width * fill, rD.Height);
                    g.Restore(st);
                }
            }

            // Dong 3: 5 chip tieu chi (>= 9pt)
            int yChip = this.Px(29);
            int hChip = this.Px(21);
            int gapChip = this.Px(5);
            int n = TenTieuChi.Length;
            float wChip = Math.Max(20f, (Width - 1f - gapChip * (n - 1)) / n);
            int szCheck = this.Px(11);

            for (int i = 0; i < n; i++)
            {
                float xC = i * (wChip + gapChip);
                var rC = new RectangleF(xC, yChip, wChip, hChip);
                bool dat = _dat[i];

                using var pC = AppTheme.DuongBoGoc(rC, this.Px(6));
                using (var bgC = new SolidBrush(dat ? AppTheme.ChipMint : AppTheme.Nen))
                    g.FillPath(bgC, pC);
                using (var vienC = new Pen(dat ? AppTheme.Phu : AppTheme.Vien, 1f))
                    g.DrawPath(vienC, pC);

                if (dat)
                {
                    LucideIcons.Draw(g, LucideIconKind.Check,
                        new RectangleF(rC.X + this.Px(6), rC.Y + (rC.Height - szCheck) / 2f, szCheck, szCheck),
                        AppTheme.PhuDam, 2.3f);
                }

                int xText = dat ? (int)rC.X + this.Px(19) : (int)rC.X + this.Px(4);
                int wText = Math.Max(10, (int)rC.Right - xText - this.Px(3));
                TextRenderer.DrawText(g, TenTieuChi[i], dat ? AppTheme.ChuNhoDam : AppTheme.ChuNho,
                    new Rectangle(xText, (int)rC.Y, wText, (int)rC.Height),
                    dat ? AppTheme.PhuDam : AppTheme.ChuPhu,
                    co | (dat ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter));
            }
        }
    }
}
