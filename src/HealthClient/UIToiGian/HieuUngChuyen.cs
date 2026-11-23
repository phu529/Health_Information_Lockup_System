using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Hieu ung chuyen man hinh / tab / buoc khoi phuc Ban 8:
    //  - Dong bo 170ms EaseOutCubic (khop voi TabSwitch 170ms), nhip 16ms (~60fps)
    //  - Bo ve ColorMatrix 2 lop nang CPU -> truot nhe 20px + che mo 1 lop nhanh, muot, khong giat lag.
    internal static class HieuUngChuyen
    {
        public const int ThoiGianMs = 170;

        public static void Chay(Control vung, Action doiNoiDung, int huong = 1, Action? khiXong = null)
        {
            foreach (LopAnh cuDangChay in vung.Controls.OfType<LopAnh>().ToList())
                cuDangChay.KetThucNgay(goiCallback: false);

            bool duDieuKien = vung.IsHandleCreated && vung.Visible && vung.Width > 40 && vung.Height > 40
                              && SystemInformation.UIEffectsEnabled;

            Bitmap? cu = null;
            if (duDieuKien)
            {
                try { cu = Chup(vung); }
                catch { cu?.Dispose(); cu = null; }
            }

            vung.SuspendLayout();
            doiNoiDung();
            vung.ResumeLayout(true);

            if (cu == null)
            {
                khiXong?.Invoke();
                return;
            }

            Bitmap? moi = null;
            try
            {
                vung.PerformLayout();
                moi = Chup(vung);

                var lop = new LopAnh(cu, moi, huong, khiXong) { Dock = DockStyle.Fill };
                cu = null;
                moi = null;
                vung.Controls.Add(lop);
                lop.BringToFront();
                lop.Bat();
            }
            catch
            {
                cu?.Dispose();
                moi?.Dispose();
                khiXong?.Invoke();
            }
        }

        private static Bitmap Chup(Control c)
        {
            var bmp = new Bitmap(Math.Max(1, c.Width), Math.Max(1, c.Height), PixelFormat.Format32bppPArgb);
            c.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
            return bmp;
        }

        private sealed class LopAnh : Control
        {
            private Bitmap? _cu, _moi;
            private readonly int _huong;
            private readonly Action? _khiXong;
            private readonly Stopwatch _dongHo = new();
            private readonly System.Windows.Forms.Timer _nhip = new() { Interval = 16 };
            private readonly ImageAttributes _ia = new();

            public LopAnh(Bitmap cu, Bitmap moi, int huong, Action? khiXong)
            {
                _cu = cu;
                _moi = moi;
                _huong = Math.Clamp(huong, -1, 1);
                _khiXong = khiXong;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                _nhip.Tick += (_, _) =>
                {
                    if (!Visible || Parent == null || !Parent.Visible)
                    {
                        KetThucNgay(goiCallback: true);
                        return;
                    }
                    if (_dongHo.ElapsedMilliseconds >= ThoiGianMs)
                    {
                        KetThucNgay(goiCallback: true);
                        return;
                    }
                    Invalidate();
                };
            }

            public void Bat()
            {
                _dongHo.Start();
                _nhip.Start();
            }

            public void KetThucNgay(bool goiCallback = true)
            {
                _nhip.Stop();
                Parent?.Controls.Remove(this);
                Dispose();
                if (goiCallback) _khiXong?.Invoke();
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                if (!Visible) _nhip.Stop();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _nhip.Stop();
                    _nhip.Dispose();
                    _ia.Dispose();
                    _cu?.Dispose();
                    _moi?.Dispose();
                    _cu = null;
                    _moi = null;
                }
                base.Dispose(disposing);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                if (_cu == null || _moi == null) return;

                float t = Math.Clamp(_dongHo.ElapsedMilliseconds / (float)ThoiGianMs, 0f, 1f);
                float k = AppTheme.EaseOutCubic(t);
                int dx = (int)MathF.Round(this.Px(22) * _huong * (1f - k));

                // Ve nen cu truoc (khong ColorMatrix), chi pha mo 1 lop duy nhat cho man hinh moi -> cuc nhe & muot
                g.DrawImageUnscaled(_cu, 0, 0);

                var m = new ColorMatrix { Matrix33 = Math.Clamp(k, 0f, 1f) };
                _ia.SetColorMatrix(m, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(_moi, new Rectangle(dx, 0, _moi.Width, _moi.Height),
                    0, 0, _moi.Width, _moi.Height, GraphicsUnit.Pixel, _ia);
            }
        }
    }
}
