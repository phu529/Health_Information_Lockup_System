using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// The ghi chu noi len canh o nhap (kieu MB Bank): tieu de dam, cac dong goi y co dau cham,
    /// mui ten nho tro vao bieu tuong (i). Mui ten o tren (the nam duoi o) hoac o duoi (the nam tren o).
    /// Chi ve khi hien; form tao the nay luc bam (i) lan dau, khong ton tai nguyen nao luc mo form.
    /// Bam vao the de dong.
    /// </summary>
    public class TheGhiChu : Control
    {
        private string _tieuDe = string.Empty;
        private string[] _dong = Array.Empty<string>();

        /// <summary>Toa do X (pixel that, tinh tu mep trai cua the) cua dinh mui ten.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MuiTenX { get; set; } = 400;

        /// <summary>True: mui ten o canh duoi (the nam phia tren o nhap).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MuiTenOPhiaDuoi { get; set; }

        public TheGhiChu()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            TabStop = false;
            Cursor = Cursors.Hand;
            Visible = false;
        }

        public void DatNoiDung(string tieuDe, params string[] dong)
        {
            _tieuDe = tieuDe;
            _dong = dong;
            Invalidate();
        }

        /// <summary>Chieu cao can thiet cho noi dung hien tai (pixel that).</summary>
        public int TinhChieuCao()
        {
            float px = DeviceDpi / 96f;
            Font phongTieuDe = Theme.Chu(11.5F, FontStyle.Bold);
            Font phongDong = Theme.Chu(10.5F);
            int caoDong = phongDong.Height + (int)Math.Round(4 * px);

            return (int)Math.Round(8 * px)            // mui ten
                 + (int)Math.Round(12 * px)           // le tren
                 + phongTieuDe.Height
                 + (int)Math.Round(8 * px)
                 + _dong.Length * caoDong
                 + (int)Math.Round(12 * px);          // le duoi
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.White);

            float px = DeviceDpi / 96f;
            int mui = (int)Math.Round(8 * px);

            if (Width < mui * 4 || Height < mui * 4)
                return;

            bool duoi = MuiTenOPhiaDuoi;
            var the = duoi
                ? new Rectangle(0, 0, Width - 1, Height - mui - 1)
                : new Rectangle(0, mui, Width - 1, Height - mui - 1);
            Color vien = Theme.TealNhat2;

            using (GraphicsPath duong = Theme.DuongBoGoc(the, Math.Max(2, (int)Math.Round(4 * px))))
            using (var nen = new SolidBrush(Color.White))
            using (var but = new Pen(vien, 1f))
            {
                g.FillPath(nen, duong);
                g.DrawPath(but, duong);
            }

            // Mui ten: to trang de xoa doan vien, roi ve hai canh xien
            int cx = Math.Min(Width - mui * 2, Math.Max(mui * 2, MuiTenX));
            int goc = duoi ? Height - mui - 1 : mui;
            int dinh = duoi ? Height - 1 : 0;
            int goc2 = duoi ? goc - 1 : goc + 1;
            var tamGiac = new[]
            {
                new Point(cx - mui, goc2), new Point(cx, dinh), new Point(cx + mui, goc2)
            };

            using (var nen = new SolidBrush(Color.White))
            using (var but = new Pen(vien, 1f))
            {
                g.FillPolygon(nen, tamGiac);
                g.DrawLine(but, cx - mui, goc, cx, dinh);
                g.DrawLine(but, cx, dinh, cx + mui, goc);
            }

            int le = (int)Math.Round(18 * px);
            int y = (duoi ? 0 : mui) + (int)Math.Round(12 * px);
            Font phongTieuDe = Theme.Chu(11.5F, FontStyle.Bold);
            Font phongDong = Theme.Chu(10.5F);
            int caoDong = phongDong.Height + (int)Math.Round(4 * px);

            TextRenderer.DrawText(
                g, _tieuDe, phongTieuDe,
                new Rectangle(le, y, Width - le * 2, phongTieuDe.Height + 4), Theme.Navy,
                TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            y += phongTieuDe.Height + (int)Math.Round(8 * px);

            foreach (string dong in _dong)
            {
                using (var cham = new SolidBrush(Theme.Teal))
                {
                    float d = 4f * px;
                    g.FillEllipse(cham, le + 2 * px, y + (phongDong.Height - d) / 2f, d, d);
                }

                TextRenderer.DrawText(
                    g, dong, phongDong,
                    new Rectangle(le + (int)Math.Round(16 * px), y, Width - le * 2 - (int)Math.Round(16 * px), caoDong),
                    Theme.ChuChinh,
                    TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

                y += caoDong;
            }
        }
    }
}
