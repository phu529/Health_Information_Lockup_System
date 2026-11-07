using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UI
{
    /// <summary>
    /// Cảnh báo y khoa dùng chung (v17). Giữ NGUYÊN VĂN câu cảnh báo của đề tài.
    ///
    /// Sửa lỗi bị che/cắt chữ ở v16: control tự ĐO chữ theo bề rộng hiện có rồi tự đặt chiều cao,
    /// nên câu dài xuống 2 hay 3 dòng cũng hiện đủ. Control mặc định Dock = Bottom nên luôn nằm sát đáy
    /// vùng chứa và các phần khác tự nhường chỗ (không còn đặt theo toạ độ tuyệt đối đè lên nút).
    /// Chữ vẽ bằng TextRenderer (ClearType) trên nền đặc, đậm vừa phải, tương phản khoảng 9:1.
    /// Thể hiện bằng nền hồng nhạt + viền đỏ + vạch đỏ bên trái + biểu tượng cảnh báo + chữ đậm,
    /// không chỉ dựa vào màu.
    /// </summary>
    public sealed class CanhBaoYTe : Control
    {
        public const string NoiDungMacDinh =
            "Thông tin chỉ mang tính tham khảo, không thể thay thế chẩn đoán của bác sĩ, nếu vấn đề của bạn quan trọng hãy đến các bệnh viện gần nhất";

        private static readonly Color ChuDoDam = Color.FromArgb(118, 22, 28);

        private const TextFormatFlags CoChu =
            TextFormatFlags.Left |
            TextFormatFlags.WordBreak |
            TextFormatFlags.NoPadding |
            TextFormatFlags.NoPrefix;

        private bool _dangDoCao;

        public CanhBaoYTe()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;

            Dock = DockStyle.Bottom;
            Height = 80;
            Font = Theme.Chu(9.5F, FontStyle.Bold);   // v32: 9,5 pt de cau canh bao vua 2 dong, khong de form phai cuon
            Text = NoiDungMacDinh;
            AccessibleRole = AccessibleRole.Alert;
            AccessibleName = "Cảnh báo y khoa";
        }

        private int Px(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        // Lề ngoài: ngang 24, trên 6, dưới 14 (điểm ảnh theo 96 DPI).
        private int LeNgang => Px(24);
        private int LeTren => Px(6);
        private int LeDuoi => Px(14);

        // Vùng chữ nằm sau vạch đỏ + biểu tượng (54) và chừa 12 bên phải.
        private int TrucChu => Px(56);
        private int ChuaPhai => Px(10);

        private int RongChu(int rongThe) => Math.Max(Px(80), rongThe - TrucChu - ChuaPhai);

        private int ChieuCaoCan()
        {
            // Phải KHỚP với bề rộng thẻ khi vẽ (Width - 2 * LeNgang - 1) để đo và vẽ ngắt dòng giống nhau.
            int rongThe = Math.Max(Px(160), Width - 2 * LeNgang - 1);

            Size doChu = TextRenderer.MeasureText(
                Text, Font, new Size(RongChu(rongThe), int.MaxValue), CoChu);

            int caoThe = Math.Max(Px(46), doChu.Height + Px(20));
            return LeTren + caoThe + LeDuoi;
        }

        private void CapNhatChieuCao()
        {
            if (_dangDoCao || Width <= 0)
                return;

            int cao = ChieuCaoCan();

            if (cao != Height)
            {
                _dangDoCao = true;

                try
                {
                    Height = cao;
                }
                finally
                {
                    _dangDoCao = false;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CapNhatChieuCao();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            CapNhatChieuCao();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            CapNhatChieuCao();
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            CapNhatChieuCao();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.White);

            var the = new Rectangle(
                LeNgang, LeTren,
                Width - 2 * LeNgang - 1,
                Height - LeTren - LeDuoi - 1);

            if (the.Width < Px(120) || the.Height < Px(30))
                return;

            int banKinh = Px(14);

            // Bong mem: ba lop mo chong nhau, tong nau am, khong dung vien do.
            for (int i = 3; i >= 1; i--)
            {
                var rb = new Rectangle(the.X + i, the.Y + Px(2) + i, the.Width - 2 * i, the.Height - i);

                using (GraphicsPath db = Theme.DuongBoGoc(rb, banKinh))
                using (var cb = new SolidBrush(Color.FromArgb(9, 170, 30, 40)))
                {
                    g.FillPath(cb, db);
                }
            }

            using (GraphicsPath duong = Theme.DuongBoGoc(the, banKinh))
            {
                // The kem am chuyen nhe sang vang nhat, nhin thay nhung khong gay gat.
                using (var nen = new LinearGradientBrush(
                    the, Color.FromArgb(255, 250, 250), Color.FromArgb(254, 238, 238), LinearGradientMode.Vertical))
                {
                    g.FillPath(nen, duong);
                }

                using (var vien = new Pen(Color.FromArgb(130, 226, 92, 92), 1F))
                {
                    g.DrawPath(vien, duong);
                }

                // Vach sang manh o canh tren, tao cam giac the kinh mo.
                using (var sang = new Pen(Color.FromArgb(190, 255, 255, 255), 1F))
                {
                    g.DrawLine(sang, the.X + banKinh, the.Y + 1, the.Right - banKinh, the.Y + 1);
                }
            }

            // Huy hieu tron o ben trai: quang mo + dia gradient cam am + dau "!" trang (tu ve, khong phu thuoc font).
            float k = DeviceDpi / 96F;
            float d = 30F * k;
            float bx = the.X + 16F * k;
            float by = the.Y + (the.Height - d) / 2F;

            using (var quang = new SolidBrush(Color.FromArgb(48, 255, 110, 110)))
            {
                g.FillEllipse(quang, bx - 4F * k, by - 4F * k, d + 8F * k, d + 8F * k);
            }

            var vungHuyHieu = new RectangleF(bx, by, d, d);

            using (var dia = new LinearGradientBrush(
                vungHuyHieu, Color.FromArgb(242, 92, 88), Color.FromArgb(188, 28, 44), LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(dia, vungHuyHieu);
            }

            using (var gach = new Pen(Color.White, 2.4F * k))
            {
                gach.StartCap = LineCap.Round;
                gach.EndCap = LineCap.Round;
                g.DrawLine(gach, bx + d / 2F, by + 8F * k, bx + d / 2F, by + 16F * k);
            }

            using (var cham = new SolidBrush(Color.White))
            {
                float r = 3F * k;
                g.FillEllipse(cham, bx + d / 2F - r / 2F, by + 19.5F * k, r, r);
            }

            var vungChu = new Rectangle(
                the.X + TrucChu, the.Y + Px(8),
                the.Width - TrucChu - ChuaPhai, the.Height - Px(16));

            TextRenderer.DrawText(g, Text, Font, vungChu, ChuDoDam, CoChu | TextFormatFlags.VerticalCenter);
        }
    }
}
