using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Huy hiệu dạng viên thuốc nhỏ phía trên tiêu đề: chấm màu + chữ in hoa nhỏ (kiểu nhãn
    /// "● CÔNG TY ..." của weeblab.net). Chỉ để dẫn mắt vào tiêu đề, không bấm được.
    /// Độ rộng viên tự khít theo chữ.
    /// </summary>
    public sealed class HuyHieu : Control
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color MauCham { get; set; } = Theme.Teal;

        public HuyHieu()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
            AccessibleRole = AccessibleRole.StaticText;
        }

        protected override Size DefaultSize => new Size(260, 28);

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            if (Width < 20 || Height < 14)
                return;

            float px = DeviceDpi / 96f;
            string chu = Text.ToUpperInvariant();
            Font phong = Theme.Chu(8.5F, FontStyle.Bold);

            Size doDai = TextRenderer.MeasureText(
                g, chu, phong, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            int le = (int)Math.Round(12 * px);
            int cham = (int)Math.Round(7 * px);
            int hoQuang = (int)Math.Round(8 * px);
            int rong = Math.Min(Width - 1, le + cham + hoQuang + doDai.Width + le);

            var vien = new Rectangle(0, 0, rong - 1, Height - 1);

            using (GraphicsPath duong = Theme.DuongBoGoc(vien, Height / 2))
            using (var nen = new SolidBrush(Color.White))
            using (var but = new Pen(Color.FromArgb(203, 213, 225), 1.2f))
            {
                g.FillPath(nen, duong);
                g.DrawPath(but, duong);
            }

            using (var co = new SolidBrush(MauCham))
            {
                g.FillEllipse(co, le, (Height - cham) / 2f, cham, cham);
            }

            TextRenderer.DrawText(
                g, chu, phong,
                new Rectangle(le + cham + hoQuang, 0, doDai.Width + 6, Height),
                Theme.ChuPhu,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPrefix);
        }
    }
}
