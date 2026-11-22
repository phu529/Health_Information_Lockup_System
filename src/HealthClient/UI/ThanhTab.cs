using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Thanh tab "Đăng nhập | Đăng ký" tự vẽ (v17). Thay hai nút Button và hai Panel gạch chân cũ.
    /// Gạch chân xanh LUÔN nằm dưới tab đang chọn (kể cả tab "Đăng ký"), vì nó do chính control này vẽ,
    /// không còn là một Panel rời có thể bị nhãn tiêu đề đè lên như bản v16.
    /// ViTriGach chạy 0..1 để form Đăng ký làm hiệu ứng gạch chân trượt từ tab này sang tab kia.
    /// </summary>
    public sealed class ThanhTab : Control
    {
        private static readonly string[] Nhan = { "Đăng nhập", "Đăng ký" };

        private int _chon;
        private int _re = -1;
        private float _viTriGach;
        private int _chiHien = -1;

        /// <summary>
        /// -1 (mac dinh): hien ca hai tab "Dang nhap | Dang ky". 0 hoac 1: chi hien MOT tieu de (Dang nhap hoac Dang ky)
        /// can trai theo le o nhap, gach chan xanh dung bang chieu rong chu (v25: moi man hinh chi co mot muc).
        /// </summary>
        [DefaultValue(-1)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ChiHienTab
        {
            get => _chiHien;
            set
            {
                _chiHien = value == 0 || value == 1 ? value : -1;
                Invalidate();
            }
        }

        /// <summary>Phát ra khi người dùng bấm vào tab KHÁC tab đang chọn. Tham số: 0 = Đăng nhập, 1 = Đăng ký.</summary>
        public event EventHandler<int>? TabBam;

        [DefaultValue(0)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int TabDangChon
        {
            get => _chon;
            set
            {
                _chon = value == 1 ? 1 : 0;
                _viTriGach = _chon;
                Invalidate();
            }
        }

        /// <summary>Vị trí gạch chân: 0 = dưới tab Đăng nhập, 1 = dưới tab Đăng ký (số lẻ = đang trượt).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ViTriGach
        {
            get => _viTriGach;
            set
            {
                _viTriGach = Math.Clamp(value, 0f, 1f);
                Invalidate();
            }
        }

        public ThanhTab()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
            AccessibleRole = AccessibleRole.PageTabList;
            AccessibleName = "Chọn Đăng nhập hoặc Đăng ký";
        }

        protected override Size DefaultSize => new Size(560, 60);

        private int Le => (int)Math.Round(56 * DeviceDpi / 96.0);

        private int ChiSoTaiDiem(Point p)
        {
            if (_chiHien >= 0)
                return -1;

            int rong = Width - 2 * Le;

            if (rong < 40 || p.X < Le || p.X >= Le + rong)
                return -1;

            return Math.Min(1, (p.X - Le) / (rong / 2));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int chiSo = ChiSoTaiDiem(e.Location);

            if (chiSo != _re)
            {
                _re = chiSo;
                Cursor = (Enabled && chiSo >= 0 && chiSo != _chon) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _re = -1;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int chiSo = ChiSoTaiDiem(e.Location);

            if (Enabled && chiSo >= 0 && chiSo != _chon)
                TabBam?.Invoke(this, chiSo);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            float px = DeviceDpi / 96f;
            int le = Le;
            int rong = Width - 2 * le;

            if (rong < 40 || Height < 20)
                return;

            int moiTab = rong / 2;
            int mong = Math.Max(1, (int)Math.Round(px));
            int cao = Math.Max(3, (int)Math.Round(3 * px));

            if (_chiHien >= 0)
            {
                // Che do MOT muc: tieu de can trai bang mep o nhap, duong ke mong chay het, gach xanh dung bang chu.
                using (var duongKe1 = new SolidBrush(Color.FromArgb(214, 224, 238)))
                    g.FillRectangle(duongKe1, le, Height - mong, rong, mong);

                Font phong = Theme.Chu(14F, FontStyle.Bold);
                const TextFormatFlags co1 = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
                Size kt = TextRenderer.MeasureText(Nhan[_chiHien], phong, new Size(int.MaxValue, int.MaxValue), co1);
                Color mau = Enabled ? Theme.Teal : Theme.XamVien;

                using (var gach1 = new SolidBrush(mau))
                    g.FillRectangle(gach1, le, Height - cao, kt.Width + (int)Math.Round(2 * px), cao);

                TextRenderer.DrawText(
                    g, Nhan[_chiHien], phong,
                    new Rectangle(le, 0, kt.Width + (int)Math.Round(8 * px), Height - cao), mau,
                    co1 | TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                return;
            }

            // Đường kẻ mảnh chạy suốt hai tab, rồi gạch xanh dày đè lên đúng tab đang chọn.
            using (var duongKe = new SolidBrush(Color.FromArgb(214, 224, 238)))
            {
                g.FillRectangle(duongKe, le, Height - mong, rong, mong);
            }

            int xGach = le + (int)Math.Round(moiTab * _viTriGach);

            using (var gach = new SolidBrush(Enabled ? Theme.Teal : Theme.XamVien))
            {
                g.FillRectangle(gach, xGach, Height - cao, moiTab, cao);
            }

            for (int i = 0; i < 2; i++)
            {
                var vung = new Rectangle(le + i * moiTab, 0, moiTab, Height - cao);
                bool dangChon = i == _chon;
                bool dangRe = i == _re && !dangChon && Enabled;

                if (dangRe)
                {
                    var nen = new Rectangle(vung.X + 4, vung.Y + (int)Math.Round(8 * px), vung.Width - 8, vung.Height - (int)Math.Round(8 * px));

                    using (GraphicsPath duong = Theme.DuongBoGoc(nen, 8))
                    using (var co = new SolidBrush(Theme.TealNhat))
                    {
                        g.FillPath(co, duong);
                    }
                }

                Color chu = !Enabled ? Theme.XamChu
                    : dangChon ? Theme.Teal
                    : dangRe ? Theme.Navy
                    : Theme.ChuPhu;

                TextRenderer.DrawText(
                    g, Nhan[i],
                    Theme.Chu(13.5F, dangChon ? FontStyle.Bold : FontStyle.Regular),
                    vung, chu,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.NoPrefix);
            }
        }
    }
}
