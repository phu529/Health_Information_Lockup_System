using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // The goi y noi bat len khi hover/bam icon (i) tren o nhap (Ban 8):
    //  - Gan Region cat dung theo duong vien bong bong + mui ten -> XOA SACH cac goc trang du thua de len nut ben duoi.
    internal sealed class TheGoiYNoi : Control
    {
        private string _tieuDe = string.Empty;
        private string[] _dong = Array.Empty<string>();
        private readonly System.Windows.Forms.Timer _henAn = new() { Interval = 180 };
        private bool _dangGhim;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MuiTenX { get; set; } = 240;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MuiTenOPhiaDuoi { get; set; }

        public bool DangGhim => _dangGhim;

        public TheGoiYNoi()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Visible = false;
            Cursor = Cursors.Hand;

            _henAn.Tick += (_, _) =>
            {
                _henAn.Stop();
                if (!_dangGhim) Visible = false;
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _henAn.Stop();
                _henAn.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible) _henAn.Stop();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _henAn.Stop();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_dangGhim) _henAn.Start();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            An();
        }

        public void HienTai(Control vungCha, Rectangle vungIconTrongCha, string tieuDe, string[] dong, bool ghim)
        {
            _henAn.Stop();
            _tieuDe = tieuDe;
            _dong = dong;
            _dangGhim = ghim;

            int leAnToan = this.Px(10);
            int w = Math.Min(this.Px(344), Math.Max(this.Px(220), vungCha.ClientSize.Width - leAnToan * 2));
            Width = w;
            int h = Math.Min(TinhChieuCao(w), Math.Max(this.Px(80), vungCha.ClientSize.Height - leAnToan * 2));

            int muiXTrongCha = vungIconTrongCha.Left + vungIconTrongCha.Width / 2;
            int maxX = Math.Max(leAnToan, vungCha.ClientSize.Width - w - leAnToan);
            int x = Math.Clamp(muiXTrongCha - w + this.Px(24), leAnToan, maxX);

            int maxY = Math.Max(leAnToan, vungCha.ClientSize.Height - h - leAnToan);
            bool datTren = vungIconTrongCha.Bottom + this.Px(4) + h > vungCha.ClientSize.Height - leAnToan
                           && vungIconTrongCha.Top - this.Px(4) - h >= leAnToan;
            int yLyTuong = datTren
                ? vungIconTrongCha.Top - this.Px(4) - h
                : vungIconTrongCha.Bottom + this.Px(4);
            int y = Math.Clamp(yLyTuong, leAnToan, maxY);

            MuiTenOPhiaDuoi = datTren;
            MuiTenX = Math.Clamp(muiXTrongCha - x, this.Px(22), w - this.Px(22));
            Bounds = new Rectangle(x, y, w, h);
            CapNhatRegion();
            Visible = true;
            BringToFront();
            Invalidate();
        }

        public void HenAnNeuKhongGhim()
        {
            if (!_dangGhim && Visible) _henAn.Start();
        }

        public void An()
        {
            _henAn.Stop();
            _dangGhim = false;
            Visible = false;
        }

        private int TinhChieuCao(int rong)
        {
            int le = this.Px(16);
            int mui = this.Px(8);
            int wChu = Math.Max(60, rong - le * 2 - this.Px(14));
            Size szTd = TextRenderer.MeasureText(_tieuDe, AppTheme.ChuDam,
                new Size(Math.Max(40, rong - le * 2 - this.Px(22)), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            int tongDong = 0;
            foreach (string d in _dong)
            {
                Size sz = TextRenderer.MeasureText(d, AppTheme.ChuNho, new Size(wChu, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                tongDong += sz.Height + this.Px(6);
            }

            return mui + this.Px(16) + szTd.Height + this.Px(12) + tongDong + this.Px(10);
        }

        private GraphicsPath TaoDuongBongBong(out RectangleF box, out int mui)
        {
            mui = this.Px(8);
            bool duoi = MuiTenOPhiaDuoi;
            box = new RectangleF(0.5f, duoi ? 0.5f : mui + 0.5f, Math.Max(4f, Width - 1.5f), Math.Max(4f, Height - mui - 1.5f));
            float r = Math.Min(this.Px(8), Math.Min(box.Width, box.Height) / 2f);
            float d = r * 2f;
            float cx = Math.Clamp(MuiTenX, box.X + r + mui + 2f, box.Right - r - mui - 2f);

            var p = new GraphicsPath();
            if (!duoi)
            {
                // Top-left arc -> top edge with upward triangle pointer -> top-right arc -> bottom-right -> bottom-left
                p.AddArc(box.X, box.Y, d, d, 180, 90);
                p.AddLine(box.X + r, box.Y, cx - mui, box.Y);
                p.AddLine(cx - mui, box.Y, cx, 0.5f);
                p.AddLine(cx, 0.5f, cx + mui, box.Y);
                p.AddLine(cx + mui, box.Y, box.Right - r, box.Y);
                p.AddArc(box.Right - d, box.Y, d, d, 270, 90);
                p.AddArc(box.Right - d, box.Bottom - d, d, d, 0, 90);
                p.AddArc(box.X, box.Bottom - d, d, d, 90, 90);
            }
            else
            {
                // Top-left -> top-right -> bottom-right -> bottom edge with downward triangle pointer -> bottom-left
                p.AddArc(box.X, box.Y, d, d, 180, 90);
                p.AddArc(box.Right - d, box.Y, d, d, 270, 90);
                p.AddArc(box.Right - d, box.Bottom - d, d, d, 0, 90);
                p.AddLine(box.Right - r, box.Bottom, cx + mui, box.Bottom);
                p.AddLine(cx + mui, box.Bottom, cx, Height - 0.5f);
                p.AddLine(cx, Height - 0.5f, cx - mui, box.Bottom);
                p.AddLine(cx - mui, box.Bottom, box.X + r, box.Bottom);
                p.AddArc(box.X, box.Bottom - d, d, d, 90, 90);
            }
            p.CloseFigure();
            return p;
        }

        private void CapNhatRegion()
        {
            if (Width < 16 || Height < 16) return;
            using var p = TaoDuongBongBong(out _, out _);
            using var pExpand = (GraphicsPath)p.Clone();
            using var pen = new Pen(Color.Black, 1.5f);
            pExpand.Widen(pen);
            var reg = new Region(p);
            reg.Union(pExpand);
            Region? cu = Region;
            Region = reg;
            cu?.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CapNhatRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Width < 24 || Height < 24) return;

            using var p = TaoDuongBongBong(out RectangleF box, out int mui);
            using var nen = new SolidBrush(AppTheme.NenTrang);
            using var vien = new Pen(AppTheme.Chinh, 1.3f);

            g.FillPath(nen, p);

            int le = this.Px(14);
            Size szTd = TextRenderer.MeasureText(g, _tieuDe, AppTheme.ChuDam,
                new Size(Math.Max(40, Width - le * 2 - this.Px(22)), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int hHead = szTd.Height + this.Px(16);

            GraphicsState st = g.Save();
            g.SetClip(p, CombineMode.Intersect);
            using (var bgHead = new SolidBrush(AppTheme.ChipXanh))
                g.FillRectangle(bgHead, 0, 0, Width, box.Y + hHead);
            using (var sep = new Pen(AppTheme.Vien, 1f))
                g.DrawLine(sep, box.X, box.Y + hHead, box.Right, box.Y + hHead);
            g.Restore(st);

            g.DrawPath(vien, p);

            // Tieu de + icon Info
            int yTd = (int)box.Y + this.Px(8);
            int szIc = this.Px(15);
            LucideIcons.Draw(g, LucideIconKind.Info,
                new RectangleF(le, yTd + Math.Max(0, (szTd.Height - szIc) / 2f), szIc, szIc),
                AppTheme.Chinh, 1.9f);
            TextRenderer.DrawText(g, _tieuDe, AppTheme.ChuDam,
                new Rectangle(le + szIc + this.Px(7), yTd, Math.Max(40, Width - le * 2 - szIc - this.Px(7)), szTd.Height + 2),
                AppTheme.TieuDeMau, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Cac quy tac goi y
            int y = (int)box.Y + hHead + this.Px(8);
            int wChu = Math.Max(40, Width - le * 2 - this.Px(14));
            foreach (string d in _dong)
            {
                float dCham = this.Px(4);
                using (var cham = new SolidBrush(AppTheme.Chinh))
                    g.FillEllipse(cham, le + this.Px(2), y + this.Px(7), dCham, dCham);

                Size sz = TextRenderer.MeasureText(g, d, AppTheme.ChuNho, new Size(wChu, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, d, AppTheme.ChuNho,
                    new Rectangle(le + this.Px(14), y, wChu, sz.Height + 2), AppTheme.Chu,
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                y += sz.Height + this.Px(6);
            }
        }
    }
}
