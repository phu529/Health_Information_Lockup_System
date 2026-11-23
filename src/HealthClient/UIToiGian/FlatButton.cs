using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    internal enum KieuNut
    {
        Chinh,
        Vien,
        Nhat,
        Chu
    }

    // Nut Ban 8:
    //  - Them TextFormatFlags.NoPrefix de ky tu '&' khong bao gio bi bien thanh gach chan '_'
    //  - Ho tro MauChuTuyChinh (vd nut "Trang chu" mau den than dam)
    //  - Ho tro BanKinhBo tuy chinh va hover 100ms nhanh gon.
    internal class FlatButton : Button
    {
        private const int ThoiGianHoverMs = 100;

        private bool _hover, _down, _dangXuLy;
        private float _goc, _hoverT, _tuHover;
        private readonly Stopwatch _dongHoHover = new();
        private readonly System.Windows.Forms.Timer _nhipHover = new() { Interval = 16 };
        private System.Windows.Forms.Timer? _dongHo;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KieuNut Kieu { get; set; } = KieuNut.Chinh;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BanKinhBo { get; set; } = -1;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color? MauChuTuyChinh { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanTrai { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanPhai { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string KyHieu { get; set; } = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Icon { get; set; } = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LucideIconKind LucideIcon { get; set; } = LucideIconKind.None;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MuiTen { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DangXuLy
        {
            get => _dangXuLy;
            set
            {
                if (_dangXuLy == value) return;
                _dangXuLy = value;
                if (value && Visible)
                {
                    _dongHo ??= new System.Windows.Forms.Timer { Interval = 40 };
                    _dongHo.Tick -= DongHoTick;
                    _dongHo.Tick += DongHoTick;
                    _dongHo.Start();
                }
                else
                {
                    _dongHo?.Stop();
                }
                Invalidate();
            }
        }

        public FlatButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            UseMnemonic = false;
            Cursor = Cursors.Hand;
            Font = AppTheme.ChuNut;

            _nhipHover.Tick += (_, _) =>
            {
                float t = Math.Clamp(_dongHoHover.ElapsedMilliseconds / (float)ThoiGianHoverMs, 0f, 1f);
                float k = AppTheme.EaseOutCubic(t);
                float dich = _hover ? 1f : 0f;
                _hoverT = _tuHover + (dich - _tuHover) * k;
                if (t >= 1f)
                {
                    _hoverT = dich;
                    _nhipHover.Stop();
                }
                Invalidate();
            };
        }

        private void DongHoTick(object? s, EventArgs e)
        {
            _goc = (_goc + 28f) % 360f;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nhipHover.Stop();
                _nhipHover.Dispose();
                if (_dongHo != null)
                {
                    _dongHo.Stop();
                    _dongHo.Dispose();
                    _dongHo = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                _nhipHover.Stop();
                _dongHo?.Stop();
                _hoverT = _hover ? 1f : 0f;
            }
            else if (_dangXuLy)
            {
                _dongHo?.Start();
            }
        }

        private void BatHover(bool hover)
        {
            _hover = hover;
            if (!Visible || !IsHandleCreated || !SystemInformation.UIEffectsEnabled)
            {
                _nhipHover.Stop();
                _hoverT = hover ? 1f : 0f;
                Invalidate();
                return;
            }
            _tuHover = _hoverT;
            _dongHoHover.Restart();
            _nhipHover.Start();
        }

        protected override void OnMouseEnter(EventArgs e) { BatHover(true); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _down = false; BatHover(false); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color nenCha = Parent?.BackColor ?? AppTheme.Nen;
            g.Clear(nenCha);
            if (Width < 4 || Height < 4) return;

            bool anXuong = _down && Enabled;
            float coLai = (anXuong && Kieu != KieuNut.Chu) ? Math.Min(1.5f, Math.Min(Width, Height) * 0.02f) : 0f;
            float ht = Enabled ? _hoverT : 0f;

            Color nen, vien, chu;
            float vienDay = 1.25f;
            switch (Kieu)
            {
                case KieuNut.Chinh:
                    nen = !Enabled && !_dangXuLy ? AppTheme.ChinhNhat
                        : anXuong ? AppTheme.ChinhDam
                        : AppTheme.Tron(AppTheme.Chinh, AppTheme.ChinhSang, ht);
                    vien = nen;
                    chu = _dangXuLy ? Color.FromArgb(217, Color.White) : Color.White;
                    vienDay = 1.2f;
                    break;
                case KieuNut.Vien:
                    nen = !Enabled ? AppTheme.Nen
                        : anXuong ? AppTheme.NenChipHover
                        : AppTheme.Tron(AppTheme.NenTrang, AppTheme.NenHover, ht);
                    vien = !Enabled ? AppTheme.Vien : AppTheme.Tron(AppTheme.VienO, AppTheme.Chinh, ht);
                    chu = Enabled ? AppTheme.Tron(AppTheme.Chu, AppTheme.Chinh, ht) : AppTheme.VienDam;
                    vienDay = 1.25f;
                    break;
                case KieuNut.Nhat:
                    nen = anXuong ? AppTheme.NenChipHover
                        : AppTheme.Tron(AppTheme.NenChip, AppTheme.NenChipHover, ht);
                    vien = (anXuong || ht > 0.2f) && Enabled ? AppTheme.ChinhSang : AppTheme.Vien;
                    chu = Enabled ? AppTheme.TieuDeMau : AppTheme.VienDam;
                    vienDay = 1.15f;
                    break;
                default:
                    nen = nenCha;
                    vien = nenCha;
                    Color gocChu = MauChuTuyChinh ?? AppTheme.Chinh;
                    chu = !Enabled ? AppTheme.VienDam
                        : anXuong ? AppTheme.ChinhDam
                        : AppTheme.Tron(gocChu, AppTheme.ChinhSang, ht);
                    break;
            }

            if (Kieu != KieuNut.Chu)
            {
                var rect = new RectangleF(1f + coLai, 1f + coLai, Width - 2.5f - coLai * 2f, Height - 3f - coLai * 2f);
                int bo = BanKinhBo >= 0 ? BanKinhBo : AppTheme.BoGoc;
                float rBo = bo == 0 ? 0f : this.Px(bo);

                if (rBo <= 0.5f)
                {
                    using (var coNen = new SolidBrush(nen))
                        g.FillRectangle(coNen, rect);
                    using (var but = new Pen(vien, vienDay))
                        g.DrawRectangle(but, rect.X, rect.Y, rect.Width, rect.Height);
                }
                else
                {
                    using var path = AppTheme.DuongBoGoc(rect, rBo);
                    using (var coNen = new SolidBrush(nen))
                        g.FillPath(coNen, path);
                    using (var but = new Pen(vien, vienDay))
                        g.DrawPath(but, path);
                }
            }

            bool coLucide = LucideIcon != LucideIconKind.None;
            bool coGlyph = !coLucide && AppTheme.CoFontIcon && Icon.Length > 0 && AppTheme.FontIconNho != null;
            bool coIcon = coLucide || coGlyph;
            bool coDuoi = Kieu != KieuNut.Chu && (MuiTen || _dangXuLy);
            int gap = this.Px(8);
            int gs = this.Px(16);

            const TextFormatFlags coDo = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            Size szIcon = coLucide ? new Size(gs, gs)
                : coGlyph ? TextRenderer.MeasureText(g, Icon, AppTheme.FontIconNho!, Size.Empty, coDo)
                : Size.Empty;
            Size szChu = TextRenderer.MeasureText(g, Text, Font, Size.Empty, coDo);

            int tong = (coIcon ? szIcon.Width + gap : 0) + szChu.Width + (coDuoi ? gap + gs : 0);
            int x0 = CanTrai ? this.Px(2)
                : CanPhai ? Math.Max(0, Width - tong - this.Px(1))
                : Math.Max(this.Px(8), (Width - tong) / 2);
            int x = x0;
            int dy = anXuong && Kieu == KieuNut.Chu ? 1 : 0;

            if (coLucide)
            {
                var rIc = new RectangleF(x, (Height - gs) / 2f + dy, gs, gs);
                LucideIcons.Draw(g, LucideIcon, rIc, chu, 1.9f);
                x += gs + gap;
            }
            else if (coGlyph)
            {
                TextRenderer.DrawText(g, Icon, AppTheme.FontIconNho!,
                    new Rectangle(x, dy, szIcon.Width + this.Px(2), Height), chu,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                x += szIcon.Width + gap;
            }

            int wChuToiDa = Math.Max(10, Width - x - (CanPhai ? 0 : this.Px(4)) - (coDuoi ? gap + gs : 0));
            int wChu = Math.Min(szChu.Width + this.Px(2), wChuToiDa);
            TextRenderer.DrawText(g, Text, Font, new Rectangle(x, dy, wChu, Height), chu,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            if (Kieu == KieuNut.Chu && ht > 0.05f && Enabled)
            {
                float yGach = (Height + szChu.Height) / 2f + this.Px(2) + dy;
                using var pGach = new Pen(Color.FromArgb((int)(255 * ht), chu), 1.4f);
                g.DrawLine(pGach, x, yGach, x + Math.Min(szChu.Width, wChu), yGach);
            }

            x += wChu;

            if (coDuoi)
            {
                float lechPhai = (!_dangXuLy && MuiTen) ? this.Px(3) * ht : 0f;
                var vungDuoi = new RectangleF(x + gap + lechPhai, (Height - gs) / 2f + dy, gs, gs);
                if (_dangXuLy)
                {
                    using var butVong = new Pen(chu, Math.Max(1.8f, this.Px(2)))
                    { StartCap = LineCap.Round, EndCap = LineCap.Round };
                    g.DrawArc(butVong, vungDuoi.X + 1, vungDuoi.Y + 1, vungDuoi.Width - 2, vungDuoi.Height - 2, _goc, 270);
                }
                else
                {
                    LucideIcons.Draw(g, LucideIconKind.ArrowRight, vungDuoi, chu, 1.9f);
                }
            }

            if (KyHieu.Length > 0)
            {
                int xKh = this.Px(14);
                int d = this.Px(20);
                int yKh = (Height - d) / 2 + dy;
                var vung = new Rectangle(xKh, yKh, d, d);
                using var nenKh = new SolidBrush(AppTheme.NenTrang);
                using var vienKh = new Pen(AppTheme.VienDam, 1f);
                g.FillEllipse(nenKh, vung);
                g.DrawEllipse(vienKh, vung);
                TextRenderer.DrawText(g, KyHieu, AppTheme.ChuNhoDam, vung, AppTheme.Chinh,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }
        }
    }
}
