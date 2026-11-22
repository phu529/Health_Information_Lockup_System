using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Hop chon to hon hop chon mac dinh cua Windows (24px), de nguoi cao tuoi nhin va bam de hon.
    /// </summary>
    public class HopChon : CheckBox
    {
        private bool _hover;

        public HopChon()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            Cursor = Cursors.Hand;
            Font = Theme.Chu(12F);
            ForeColor = Theme.ChuChinh;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            int o = (int)Math.Round(24 * DeviceDpi / 96.0);
            var hop = new Rectangle(2, (Height - o) / 2, o, o);

            using (GraphicsPath duong = Theme.DuongBoGoc(hop, 5))
            using (var nen = new SolidBrush(Checked ? (_hover ? Theme.TealDam : Theme.Teal) : (_hover ? Theme.TealNhat : Color.White)))
            using (var vien = new Pen(Checked ? Theme.Teal : _hover ? Theme.Teal : Theme.XamVien, _hover ? 2.6f : 2f))
            {
                g.FillPath(nen, duong);
                g.DrawPath(vien, duong);
            }

            if (Checked)
            {
                using var dau = new Pen(Color.White, 3f);
                dau.StartCap = LineCap.Round;
                dau.EndCap = LineCap.Round;
                dau.LineJoin = LineJoin.Round;

                g.DrawLines(dau, new[]
                {
                    new PointF(hop.X + o * 0.22f, hop.Y + o * 0.52f),
                    new PointF(hop.X + o * 0.42f, hop.Y + o * 0.72f),
                    new PointF(hop.X + o * 0.78f, hop.Y + o * 0.30f)
                });
            }

            if (Focused && ShowFocusCues)
            {
                var ngoai = Rectangle.Inflate(hop, 3, 3);
                using (GraphicsPath duong = Theme.DuongBoGoc(ngoai, 7))
                using (var vien = new Pen(Theme.TealDam, 2f))
                {
                    g.DrawPath(vien, duong);
                }
            }

            int leChu = hop.Right + 10;
            TextRenderer.DrawText(
                g, Text, Font,
                new Rectangle(leChu, 0, Math.Max(10, Width - leChu), Height), ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            Invalidate();
            base.OnCheckedChanged(e);
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
