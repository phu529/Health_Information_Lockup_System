using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Nut "Quay lai" (v26) o goc tren trai bang thuong hieu cua form Dang nhap va Dang ky.
    ///
    /// Thiet ke "kinh mo" kin dao: vien thuoc nho (cao 32 px), nen trang trong suot rat nhe, vien manh 1 px,
    /// chi co mui ten nho + chu "Quay lai" mau trang. Nho va nam sat goc nen KHONG che logo y te va
    /// khong tranh cua so voi tieu de, nhung van du tuong phan de nhin ra (chu trang tren nen xanh dam).
    /// Re chuot: nen va vien sang len, co quang cyan rat nhe, mui ten nhich sang trai.
    /// Bam: nen dam hon, vien thuoc lun xuong 1 px.
    ///
    /// Vung bam du lon (cao 44 px gom le). Kich thuoc tinh lai theo DPI va do rong that cua chu.
    /// Su kien Bam tra ve diem bam theo toa do MAN HINH. Khong cham toi logic dang nhap.
    /// </summary>
    public sealed class NutQuayLai : Control
    {
        private const string NhanNut = "Quay lại";

        private static readonly Font PhongChu = Theme.Chu(9.5F, FontStyle.Bold);

        private const TextFormatFlags CoDo =
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        private bool _re;
        private bool _nhan;
        private readonly ToolTip _goiY = new();

        /// <summary>Phat ra khi bam chuot trai va tha ngay trong nut. Tham so: diem bam theo toa do man hinh.</summary>
        public event EventHandler<Point>? Bam;

        public NutQuayLai()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            SetStyle(ControlStyles.Selectable, false);

            BackColor = Color.Transparent;
            TabStop = false;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PushButton;
            AccessibleName = "Quay lại";
            AccessibleDescription = "Quay lại màn hình chào";

            _goiY.SetToolTip(this, "Về màn hình chào (Alt + ←)");
        }

        protected override Size DefaultSize => new Size(112, 44);

        private int Px(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        private int LeNgoai => Px(6);
        private int CaoVien => Px(32);
        private int LeNhan => Px(20);

        // Bo cuc ben trong vien thuoc: [le][mui ten][khoang][chu][le]
        private int LeTrai => Px(11);
        private int RongMuiTen => Px(7);
        private int KhoangChu => Px(8);
        private int LePhai => Px(14);

        /// <summary>Gan nut vao goc tren trai cua cha va tu can chinh khi cha doi kich thuoc hoac DPI.</summary>
        public void GanVaoGoc(Control cha)
        {
            cha.Layout += (s, e) => CanChinh();
            HandleCreated += (s, e) => CanChinh();
            CanChinh();
        }

        private int RongChu()
            => TextRenderer.MeasureText(NhanNut, PhongChu, new Size(int.MaxValue, int.MaxValue), CoDo).Width;

        private void CanChinh()
        {
            int rongVien = LeTrai + RongMuiTen + KhoangChu + RongChu() + LePhai;
            int rong = rongVien + 2 * LeNgoai;
            int cao = CaoVien + 2 * LeNgoai;

            var moi = new Rectangle(LeNhan - LeNgoai, LeNhan - LeNgoai, rong, cao);

            if (Bounds != moi)
                Bounds = moi;
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            CanChinh();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _re = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _re = false;
            _nhan = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                _nhan = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            bool daNhan = _nhan;
            _nhan = false;
            Invalidate();

            if (daNhan && e.Button == MouseButtons.Left && Enabled && ClientRectangle.Contains(e.Location))
                Bam?.Invoke(this, PointToScreen(e.Location));
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        // Hinh vien thuoc: hai nua hinh tron o hai dau.
        private static GraphicsPath DuongVien(RectangleF r)
        {
            var p = new GraphicsPath();
            float d = r.Height;
            p.AddArc(r.X, r.Y, d, d, 90, 180);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 180);
            p.CloseFigure();
            return p;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width < Px(60) || Height < Px(24))
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int le = LeNgoai;
            int chim = _nhan ? Px(1) : 0;
            var vien = new RectangleF(le, le + chim, Width - 2 * le - 1, CaoVien - 1);

            // 1) Quang cyan rat nhe khi re chuot.
            if (_re && Enabled && !_nhan)
            {
                for (int i = 3; i >= 1; i--)
                {
                    RectangleF q = vien;
                    q.Inflate(i * 1.2f, i * 1.2f);

                    using (GraphicsPath dp = DuongVien(q))
                    using (var co = new SolidBrush(Color.FromArgb(14, 150, 225, 255)))
                        g.FillPath(co, dp);
                }
            }

            // 2) Than vien thuoc: kinh mo trong suot.
            int aNen = !Enabled ? 12 : _nhan ? 70 : _re ? 52 : 28;
            int aVien = !Enabled ? 40 : _re ? 190 : 100;

            using (GraphicsPath dv = DuongVien(vien))
            {
                using (var nen = new SolidBrush(Color.FromArgb(aNen, 255, 255, 255)))
                    g.FillPath(nen, dv);

                using (var but = new Pen(Color.FromArgb(aVien, 255, 255, 255), 1.1f))
                    g.DrawPath(but, dv);
            }

            // 3) Mui ten nho; re chuot thi nhich sang trai.
            Color mau = Enabled ? Color.White : Color.FromArgb(160, 185, 225);
            float nhich = _re && Enabled ? -Px(2) : 0f;
            float cx = vien.X + LeTrai + Px(4) + nhich;
            float cy = vien.Y + vien.Height / 2f;
            float ngang = Px(3);
            float cao = Px(5);

            using (var mui = new Pen(mau, 2.0f))
            {
                mui.StartCap = LineCap.Round;
                mui.EndCap = LineCap.Round;
                mui.LineJoin = LineJoin.Round;

                g.DrawLines(mui, new[]
                {
                    new PointF(cx + ngang, cy - cao),
                    new PointF(cx - ngang, cy),
                    new PointF(cx + ngang, cy + cao)
                });
            }

            // 4) Chu "Quay lai".
            int xChu = (int)Math.Round(vien.X) + LeTrai + RongMuiTen + KhoangChu;
            var vungChu = new Rectangle(
                xChu, (int)Math.Round(vien.Y),
                Math.Max(10, (int)Math.Round(vien.Right) - xChu),
                (int)Math.Round(vien.Height));

            TextRenderer.DrawText(g, NhanNut, PhongChu, vungChu, mau, CoDo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _goiY.Dispose();

            base.Dispose(disposing);
        }
    }
}
