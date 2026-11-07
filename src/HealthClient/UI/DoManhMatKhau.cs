using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Thanh do manh mat khau + danh sach 5 tieu chi (v34)
    /// mot thanh muc (do - cam - xanh), nhan Yeu / Trung binh / Manh, va 5 tieu chi (8 ky tu, A-Z, a-z, 123, @#$)
    /// moi tieu chi la vong rong, dat thi thanh vong xanh co dau tich.
    ///
    /// CHI LA GOI Y HIEN THI: luat chap nhan mat khau that van o FrmDangKy (8 den 64 ky tu, co chu va so) va se do
    /// InputValidator cua Trong Phu kiem lai o Server. Control nay khong dung Timer, khong tao Bitmap; chi ve lai khi
    /// noi dung mat khau doi (nhe, khong anh huong CPU luc dung yen).
    /// Khong co thuoc tinh public nao (tranh WFO1000); dung phuong thuc CapNhat.
    /// </summary>
    public sealed class DoManhMatKhau : Control
    {
        private static readonly string[] NhanTieuChi = { "8 ký tự", "A-Z", "a-z", "123", "@#$" };

        private static readonly Color MauYeu = Color.FromArgb(214, 66, 58);
        private static readonly Color MauVua = Color.FromArgb(217, 119, 6);
        private static readonly Color MauManh = Color.FromArgb(22, 163, 74);

        private readonly bool[] _dat = new bool[5];
        private int _muc;

        public DoManhMatKhau()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
            AccessibleRole = AccessibleRole.StaticText;
            AccessibleName = "Độ mạnh mật khẩu";
        }

        protected override Size DefaultSize => new Size(448, 54);

        /// <summary>Phan tich mat khau, ve lai, va tra ve mau vien goi y cho o nhap (Color.Empty khi chua nhap gi).</summary>
        public Color CapNhat(string matKhau)
        {
            matKhau ??= string.Empty;

            _dat[0] = matKhau.Length >= 8;
            _dat[1] = matKhau.Any(char.IsUpper);
            _dat[2] = matKhau.Any(char.IsLower);
            _dat[3] = matKhau.Any(char.IsDigit);
            _dat[4] = matKhau.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

            _muc = _dat.Count(d => d);

            AccessibleDescription = _muc == 0
                ? "Chưa nhập mật khẩu"
                : $"{TenMuc()}, đạt {_muc} trên 5 tiêu chí";

            Invalidate();
            return _muc == 0 ? Color.Empty : MauMuc();
        }

        private string TenMuc() => _muc <= 2 ? "Yếu" : _muc <= 4 ? "Trung bình" : "Mạnh";

        private Color MauMuc() => _muc <= 2 ? MauYeu : _muc <= 4 ? MauVua : MauManh;

        private static GraphicsPath DuongVien(RectangleF r)
        {
            var p = new GraphicsPath();
            float d = Math.Min(r.Height, r.Width);
            p.AddArc(r.X, r.Y, d, d, 90, 180);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 180);
            p.CloseFigure();
            return p;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            if (Width < 120 || Height < 30)
                return;

            // v40: ti le ve = min(DPI, chieu cao thuc / 54). Neu control KHONG duoc co gian theo DPI (chieu cao van 54 diem anh
            // trong khi DPI = 150%) thi van ve vua khit trong chieu cao do, khong con bi cat mat hang 5 tieu chi.
            float dpi = DeviceDpi / 96f;
            float px = Math.Min(dpi, Height / 54f);
            float fs = px / dpi;
            const TextFormatFlags dd = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

            // Hang 1: nhan ben trai, ten muc ben phai (co mau).
            int cao1 = (int)(18 * px);
            TextRenderer.DrawText(g, "Độ mạnh mật khẩu", Theme.Chu(9F * fs), new Rectangle(0, 0, Width / 2, cao1), Theme.ChuPhu, dd | TextFormatFlags.Left);

            if (_muc > 0)
            {
                TextRenderer.DrawText(
                    g, TenMuc(), Theme.Chu(9F * fs, FontStyle.Bold),
                    new Rectangle(Width / 2, 0, Width - Width / 2, cao1), MauMuc(), dd | TextFormatFlags.Right);
            }

            // Hang 2: thanh muc.
            float yThanh = cao1 + 3 * px;
            float caoThanh = 5 * px;
            var nenThanh = new RectangleF(0, yThanh, Width - 1, caoThanh);

            using (GraphicsPath dn = DuongVien(nenThanh))
            using (var co = new SolidBrush(Color.FromArgb(229, 234, 242)))
                g.FillPath(co, dn);

            if (_muc > 0)
            {
                var day = new RectangleF(0, yThanh, Math.Max(caoThanh, (Width - 1) * _muc / 5f), caoThanh);

                using (GraphicsPath dd2 = DuongVien(day))
                using (var co = new SolidBrush(MauMuc()))
                    g.FillPath(co, dd2);
            }

            // Hang 3: 5 tieu chi, chia deu chieu ngang.
            float yHang = yThanh + caoThanh + 8 * px;
            float rongO = Width / 5f;
            float d = 13 * px;

            for (int i = 0; i < 5; i++)
            {
                float x = i * rongO;
                var vong = new RectangleF(x + 1, yHang, d, d);

                if (_dat[i])
                {
                    using (var co = new SolidBrush(MauManh))
                        g.FillEllipse(co, vong);

                    using (var tich = new Pen(Color.White, 1.6f * px))
                    {
                        tich.StartCap = LineCap.Round;
                        tich.EndCap = LineCap.Round;
                        tich.LineJoin = LineJoin.Round;
                        g.DrawLines(tich, new[]
                        {
                            new PointF(vong.X + d * 0.26f, vong.Y + d * 0.54f),
                            new PointF(vong.X + d * 0.44f, vong.Y + d * 0.72f),
                            new PointF(vong.X + d * 0.76f, vong.Y + d * 0.32f)
                        });
                    }
                }
                else
                {
                    using (var but = new Pen(Color.FromArgb(172, 183, 200), 1.4f))
                        g.DrawEllipse(but, vong.X + 0.5f, vong.Y + 0.5f, d - 1, d - 1);
                }

                Color mauChu = _dat[i] ? Theme.ChuChinh : Color.FromArgb(112, 122, 138);

                TextRenderer.DrawText(
                    g, NhanTieuChi[i], Theme.Chu(9F * fs),
                    new Rectangle((int)(x + d + 6 * px), (int)yHang - 2, (int)(rongO - d - 8 * px), (int)d + 4),
                    mauChu, dd | TextFormatFlags.Left);
            }
        }
    }
}
