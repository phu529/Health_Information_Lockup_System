using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // O tich bo goc Ban 5:
    //  - O vuong 18px, bo 5px, vien 1.5px; khi tich ve dan dau tick trang 2px trong 140ms (Stopwatch)
    //  - Can tam doc o tich dung bang tam dong chu
    //  - Lien ket giu dung font dam co dinh (khong doi font khi hover, chi ve gach chan DrawLine)
    internal sealed class OTich : Control
    {
        private const int ThoiGianTickMs = 140;

        private bool _tich, _hover, _hoverLienKet;
        private float _tienDoTick;
        private Font? _fontDam;
        private readonly Stopwatch _dongHo = new();
        private readonly System.Windows.Forms.Timer _nhip = new() { Interval = 16 };
        private readonly List<(Rectangle Vung, string Chu)> _vungLienKet = new();

        public event EventHandler? CheckedChanged;
        public event Action<string>? LienKetClick;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] LienKet { get; set; } = Array.Empty<string>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Checked
        {
            get => _tich;
            set
            {
                if (_tich == value) return;
                _tich = value;
                if (_tich && Visible && IsHandleCreated && SystemInformation.UIEffectsEnabled)
                {
                    _tienDoTick = 0f;
                    _dongHo.Restart();
                    _nhip.Start();
                }
                else
                {
                    _nhip.Stop();
                    _tienDoTick = _tich ? 1f : 0f;
                }
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public OTich()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable | ControlStyles.StandardClick, true);
            TabStop = true;
            Cursor = Cursors.Hand;
            Font = AppTheme.ChuDam;
            ForeColor = AppTheme.ChuPhu;
            Height = this.Px(28);

            _nhip.Tick += (_, _) =>
            {
                float t = Math.Clamp(_dongHo.ElapsedMilliseconds / (float)ThoiGianTickMs, 0f, 1f);
                _tienDoTick = AppTheme.EaseOutCubic(t);
                if (t >= 1f) _nhip.Stop();
                Invalidate();
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nhip.Stop();
                _nhip.Dispose();
                _fontDam?.Dispose();
                _fontDam = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                _nhip.Stop();
                _tienDoTick = _tich ? 1f : 0f;
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            _fontDam?.Dispose();
            _fontDam = new Font(Font, FontStyle.Bold);
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Invalidate();
            base.OnTextChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _hoverLienKet = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool tren = _vungLienKet.Any(v => v.Vung.Contains(e.Location));
            if (tren != _hoverLienKet) { _hoverLienKet = tren; Invalidate(); }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Focus();
            foreach (var (vung, chu) in _vungLienKet)
            {
                if (vung.Contains(e.Location))
                {
                    LienKetClick?.Invoke(chu);
                    return;
                }
            }
            Checked = !Checked;
        }

        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Checked = !Checked;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        private List<(string Text, string? Link)> TachDoan()
        {
            var kq = new List<(string Text, string? Link)>();
            string con = Text;
            while (con.Length > 0)
            {
                int viTri = -1;
                string? tim = null;
                foreach (string lk in LienKet)
                {
                    int i = lk.Length == 0 ? -1 : con.IndexOf(lk, StringComparison.Ordinal);
                    if (i >= 0 && (viTri < 0 || i < viTri)) { viTri = i; tim = lk; }
                }

                string thuong = viTri < 0 ? con : con.Substring(0, viTri);
                if (thuong.Length > 0) kq.Add((thuong, null));
                if (viTri < 0 || tim == null) break;
                kq.Add((tim, tim));
                con = con.Substring(viTri + tim.Length);
            }
            return kq;
        }

        private static List<string> TachTu(string s)
        {
            var kq = new List<string>();
            int i = 0;
            while (i < s.Length)
            {
                int start = i;
                while (i < s.Length && s[i] != ' ') i++;
                while (i < s.Length && s[i] == ' ') i++;
                kq.Add(s.Substring(start, i - start));
            }
            return kq;
        }

        private void VeBox(Graphics g, int yBox, int d)
        {
            var box = new RectangleF(1f, yBox + 1f, d - 2f, d - 2f);
            using (var path = AppTheme.DuongBoGoc(box, this.Px(5)))
            {
                if (Focused && ShowFocusCues)
                {
                    using var sang = new Pen(AppTheme.VienOCoFocus, 3f);
                    g.DrawPath(sang, path);
                }

                Color nen = _tich ? AppTheme.Chinh : AppTheme.NenTrang;
                Color vien = _tich ? AppTheme.Chinh : (_hover ? AppTheme.ChinhSang : AppTheme.VienDam);
                using var coNen = new SolidBrush(nen);
                using var but = new Pen(vien, 1.5f);
                g.FillPath(coNen, path);
                g.DrawPath(but, path);
            }

            if (_tich && _tienDoTick > 0.02f)
            {
                float u = d / 18f;
                var p0 = new PointF(4.3f * u, yBox + 9.4f * u);
                var p1 = new PointF(7.6f * u, yBox + 12.6f * u);
                var p2 = new PointF(13.7f * u, yBox + 5.6f * u);

                using var tick = new Pen(Color.White, Math.Max(1.9f, 2.0f * u))
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };

                float p = Math.Clamp(_tienDoTick, 0f, 1f);
                if (p <= 0.36f)
                {
                    float s = p / 0.36f;
                    var pm = new PointF(p0.X + (p1.X - p0.X) * s, p0.Y + (p1.Y - p0.Y) * s);
                    g.DrawLine(tick, p0, pm);
                }
                else
                {
                    float s = (p - 0.36f) / 0.64f;
                    var pm = new PointF(p1.X + (p2.X - p1.X) * s, p1.Y + (p2.Y - p1.Y) * s);
                    g.DrawLines(tick, new[] { p0, p1, pm });
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Parent?.BackColor ?? AppTheme.NenTrang);

            int d = this.Px(18);
            int xStart = d + this.Px(9);
            _vungLienKet.Clear();

            // Truong hop khong co link (nhu "Nho mat khau"): ve bang TextRenderer de khop 100% dong co so
            if (LienKet.Length == 0)
            {
                Size szChu = TextRenderer.MeasureText(g, Text.Length == 0 ? "Ag" : Text, Font,
                    new Size(Math.Max(40, Width - xStart), Height), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                int yBoxSingle = Math.Max(0, (Height - d) / 2);
                VeBox(g, yBoxSingle, d);

                TextRenderer.DrawText(g, Text, Font,
                    new Rectangle(xStart, 0, Math.Max(10, Width - xStart), Height), ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                _ = szChu;
                return;
            }

            _fontDam ??= new Font(Font, FontStyle.Bold);
            using var sf = new StringFormat(StringFormat.GenericTypographic);
            sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap;

            float caoDong = TextRenderer.MeasureText(g, "Ag", Font, Size.Empty, TextFormatFlags.NoPadding).Height + this.Px(2);

            // Giu nguyen font co dinh khi hover, khong doi co/rong khi ve
            var doan = TachDoan();
            var dongVe = new List<(string Cum, string? Link, float X, int Dong, float W)>();
            float curX = xStart;
            int curDong = 0;
            float maxRight = Math.Max(xStart + 40f, Width - 2f);

            foreach (var (text, link) in doan)
            {
                if (link != null)
                {
                    float w = g.MeasureString(text, _fontDam, 2000, sf).Width;
                    if (curX > xStart && curX + w > maxRight)
                    {
                        curDong++;
                        curX = xStart;
                    }
                    dongVe.Add((text, link, curX, curDong, w));
                    curX += w;
                }
                else
                {
                    foreach (string tu in TachTu(text))
                    {
                        float w = g.MeasureString(tu, Font, 2000, sf).Width;
                        if (curX > xStart && curX + w > maxRight)
                        {
                            curDong++;
                            curX = xStart;
                        }
                        dongVe.Add((tu, null, curX, curDong, w));
                        curX += w;
                    }
                }
            }

            int soDong = curDong + 1;
            float tongCao = soDong * caoDong;
            float y0 = Math.Max(0f, (Height - tongCao) / 2f);
            int yBox = soDong == 1
                ? Math.Max(0, (Height - d) / 2)
                : Math.Max(0, (int)Math.Round(y0 + (caoDong - d) / 2f));

            VeBox(g, yBox, d);

            using var coChu = new SolidBrush(ForeColor);
            Color mauLink = _hoverLienKet ? AppTheme.ChinhSang : AppTheme.Chinh;
            using var brLink = new SolidBrush(mauLink);
            using var gachLink = new Pen(mauLink, _hoverLienKet ? 1.5f : 1.1f);

            foreach (var (cum, link, x, dong, w) in dongVe)
            {
                float yChu = y0 + dong * caoDong;
                if (link == null)
                {
                    g.DrawString(cum, Font, coChu, x, yChu, sf);
                }
                else
                {
                    g.DrawString(cum, _fontDam, brLink, x, yChu, sf);
                    float yGach = yChu + caoDong - this.Px(2);
                    g.DrawLine(gachLink, x, yGach, x + w, yGach);
                    _vungLienKet.Add((new Rectangle((int)x - 2, (int)yChu - 2, (int)Math.Ceiling(w) + 4, (int)Math.Ceiling(caoDong) + 4), link));
                }
            }
        }
    }
}
