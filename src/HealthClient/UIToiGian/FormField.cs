using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // O nhap lieu Ban 9:
    //  - Tang chieu cao nhan (28px), khung nhap (44px), dong loi do (24px) de KHONG BAO GIO cat net dau/duoi chu tieng Viet.
    internal sealed class FormField : UserControl
    {
        private readonly TextBox _o = new();
        private readonly Label _loi = new();
        private readonly System.Windows.Forms.Timer _treHoverInfo = new() { Interval = 120 };

        private string _nhanChu = string.Empty;
        private LucideIconKind _iconTrai = LucideIconKind.None;
        private string? _tieuDeGoiY;
        private string[]? _noiDungGoiY;
        private bool _batBuoc, _gon, _thuGon, _matKhau, _coFocus, _hienNutXoa;
        private bool _hoverKhung, _hoverNutPhu, _hoverInfo, _coLoi;
        private bool _truocCoNutPhu;
        private Rectangle _vungKhung;
        private Rectangle _vungNutPhu;
        private Rectangle _vungInfo;

        public event EventHandler<bool>? GoiYHoverChanged;
        public event EventHandler? GoiYClicked;

        public FormField()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = AppTheme.NenTrang;
            TabStop = false;

            _o.BorderStyle = BorderStyle.None;
            _o.Font = AppTheme.ChuNhap;
            _o.BackColor = AppTheme.NenTrang;
            _o.ForeColor = AppTheme.Chu;
            _o.Enter += (_, _) => { _coFocus = true; Invalidate(); };
            _o.Leave += (_, _) => { _coFocus = false; Invalidate(); };
            _o.TextChanged += (_, _) =>
            {
                if (_coLoi) Loi = string.Empty;
                if (CoNutPhuHien != _truocCoNutPhu) BoTri();
                Invalidate();
                OnTextChanged(EventArgs.Empty);
            };

            _loi.AutoSize = false;
            _loi.UseMnemonic = false;
            _loi.Font = AppTheme.ChuNhoDam;
            _loi.ForeColor = AppTheme.Loi;
            _loi.BackColor = Color.Transparent;
            _loi.TextAlign = ContentAlignment.MiddleLeft;

            _treHoverInfo.Tick += (_, _) =>
            {
                _treHoverInfo.Stop();
                if (!IsDisposed && Visible && _hoverInfo && CoIconInfo)
                    GoiYHoverChanged?.Invoke(this, true);
            };

            Controls.Add(_o);
            Controls.Add(_loi);

            BoTri();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _treHoverInfo.Stop();
                _treHoverInfo.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                _treHoverInfo.Stop();
                _hoverInfo = false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Nhan { get => _nhanChu; set { _nhanChu = value ?? string.Empty; Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BatBuoc { get => _batBuoc; set { _batBuoc = value; Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LucideIconKind IconTrai { get => _iconTrai; set { _iconTrai = value; BoTri(); Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BieuTuong
        {
            get => _iconTrai.ToString();
            set
            {
                _iconTrai = value switch
                {
                    AppTheme.IcNguoi => LucideIconKind.User,
                    AppTheme.IcKhoa => LucideIconKind.Lock,
                    AppTheme.IcThu => LucideIconKind.Mail,
                    AppTheme.IcTim => LucideIconKind.Search,
                    _ => LucideIconKind.None
                };
                BoTri();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool GonGang { get => _gon; set { _gon = value; BoTri(); Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ThuGon { get => _thuGon; set { _thuGon = value; BoTri(); Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HienNutXoa { get => _hienNutXoa; set { _hienNutXoa = value; BoTri(); Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PlaceholderText { get => _o.PlaceholderText; set => _o.PlaceholderText = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxLength { get => _o.MaxLength; set => _o.MaxLength = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LaMatKhau
        {
            get => _matKhau;
            set
            {
                _matKhau = value;
                _o.UseSystemPasswordChar = value;
                BoTri();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? TieuDeGoiY { get => _tieuDeGoiY; set { _tieuDeGoiY = value; BoTri(); Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[]? NoiDungGoiY { get => _noiDungGoiY; set { _noiDungGoiY = value; BoTri(); Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Loi
        {
            get => _coLoi ? _loi.Text : string.Empty;
            set
            {
                string v = value ?? string.Empty;
                _coLoi = v.Length > 0;
                _loi.Text = v;
                Invalidate();
            }
        }

        [AllowNull]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get => _o.Text;
            set => _o.Text = value ?? string.Empty;
        }

        public void Clear() => _o.Clear();
        public new bool Focus() => _o.Focus();
        public Rectangle LayVungIconGoiY() => _vungInfo.IsEmpty ? _vungKhung : _vungInfo;

        private bool CoIconInfo => _noiDungGoiY != null && _noiDungGoiY.Length > 0;
        private bool CoNutPhuHien => _matKhau || (_hienNutXoa && _o.TextLength > 0);

        private void BoTri()
        {
            int yKhung = _gon ? 0 : this.Px(_thuGon ? 26 : 28);
            int hKhung = this.Px(44);
            int gapLoi = _gon ? 0 : this.Px(3);
            int hLoi = _gon ? 0 : this.Px(24);
            int hTong = _gon ? hKhung : (yKhung + hKhung + gapLoi + hLoi);

            if (Height != hTong) { Height = hTong; return; }

            _vungKhung = new Rectangle(0, yKhung, Width, hKhung);
            _loi.Visible = !_gon;
            _loi.SetBounds(0, yKhung + hKhung + gapLoi, Width, hLoi);

            int padTrai = this.Px(12);
            int trai = _iconTrai != LucideIconKind.None ? this.Px(12) + this.Px(18) + this.Px(10) : padTrai;

            int wBtn = this.Px(28);
            int hBtn = Math.Min(hKhung - this.Px(4), this.Px(28));
            int yBtn = yKhung + (hKhung - hBtn) / 2;
            int phai = Width - this.Px(6);

            if (CoIconInfo)
            {
                _vungInfo = new Rectangle(phai - wBtn, yBtn, wBtn, hBtn);
                phai = _vungInfo.Left - this.Px(2);
            }
            else
            {
                _vungInfo = Rectangle.Empty;
            }

            _truocCoNutPhu = CoNutPhuHien;
            if (_truocCoNutPhu)
            {
                _vungNutPhu = new Rectangle(phai - wBtn, yBtn, wBtn, hBtn);
                phai = _vungNutPhu.Left - this.Px(4);
            }
            else
            {
                _vungNutPhu = Rectangle.Empty;
                if (CoIconInfo) phai -= this.Px(4);
                else phai = Width - this.Px(10);
            }

            _o.Left = trai;
            _o.Width = Math.Max(10, phai - trai);
            _o.Top = yKhung + Math.Max(this.Px(4), (hKhung - _o.Height) / 2);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            BoTri();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool trenInfo = CoIconInfo && _vungInfo.Contains(e.Location);
            bool trenPhu = CoNutPhuHien && _vungNutPhu.Contains(e.Location);
            bool trenKhung = _vungKhung.Contains(e.Location);

            if (trenKhung != _hoverKhung) { _hoverKhung = trenKhung; Invalidate(); }
            if (trenPhu != _hoverNutPhu) { _hoverNutPhu = trenPhu; Invalidate(); }
            if (trenInfo != _hoverInfo)
            {
                _hoverInfo = trenInfo;
                Invalidate();
                if (_hoverInfo)
                {
                    _treHoverInfo.Stop();
                    _treHoverInfo.Start();
                }
                else
                {
                    _treHoverInfo.Stop();
                    GoiYHoverChanged?.Invoke(this, false);
                }
            }

            Cursor = (trenInfo || trenPhu) ? Cursors.Hand
                : trenKhung ? Cursors.IBeam
                : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _treHoverInfo.Stop();
            bool doiInfo = _hoverInfo;
            if (_hoverNutPhu || _hoverKhung || _hoverInfo)
            {
                _hoverNutPhu = false;
                _hoverKhung = false;
                _hoverInfo = false;
                Invalidate();
            }
            if (doiInfo) GoiYHoverChanged?.Invoke(this, false);
            Cursor = Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            if (CoIconInfo && _vungInfo.Contains(e.Location))
            {
                _treHoverInfo.Stop();
                GoiYClicked?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (CoNutPhuHien && _vungNutPhu.Contains(e.Location))
            {
                if (_matKhau)
                {
                    _o.UseSystemPasswordChar = !_o.UseSystemPasswordChar;
                    Invalidate();
                }
                else if (_hienNutXoa && _o.TextLength > 0)
                {
                    _o.Clear();
                }

                _o.Focus();
                return;
            }

            if (_vungKhung.Contains(e.Location))
                _o.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color nenCha = Parent?.BackColor ?? BackColor;
            g.Clear(nenCha);

            // 1. Nhan + dau * do ve tay voi dem du cho dau tieng Viet
            if (!_gon && _nhanChu.Length > 0)
            {
                int hNhan = Math.Max(this.Px(22), _vungKhung.Y - this.Px(1));
                const TextFormatFlags coNhan = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
                Size szNhan = TextRenderer.MeasureText(g, _nhanChu, AppTheme.ChuNhan, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int wNhan = Math.Min(Width - this.Px(14), szNhan.Width + this.Px(6));
                TextRenderer.DrawText(g, _nhanChu, AppTheme.ChuNhan,
                    new Rectangle(0, 0, Math.Max(4, wNhan), hNhan), AppTheme.TieuDeMau, coNhan);

                if (_batBuoc)
                {
                    TextRenderer.DrawText(g, "*", AppTheme.ChuNhan,
                        new Rectangle(wNhan, 0, Math.Max(14, Width - wNhan), hNhan), AppTheme.NhanDoBatBuoc, coNhan);
                }
            }

            // 2. Khung nhap bo goc 6px sac net
            var r = new RectangleF(1.5f, _vungKhung.Y + 1.5f, Math.Max(4f, Width - 3.5f), Math.Max(4f, _vungKhung.Height - 3.5f));
            bool coLoi = _coLoi;
            Color mauVien = coLoi ? AppTheme.Loi
                : _coFocus ? AppTheme.ChinhSang
                : _hoverKhung ? AppTheme.VienDam
                : AppTheme.VienO;
            float day = (coLoi || _coFocus) ? 2f : 1.2f;

            using (var path = AppTheme.DuongBoGoc(r, this.Px(6)))
            using (var nen = new SolidBrush(AppTheme.NenTrang))
            using (var but = new Pen(mauVien, day))
            {
                g.FillPath(nen, path);
                if (_coFocus || coLoi)
                {
                    Color mauRing = coLoi ? Color.FromArgb(40, AppTheme.Loi) : AppTheme.VienOCoFocus;
                    using var sang = new Pen(mauRing, 3f);
                    g.DrawPath(sang, path);
                }
                g.DrawPath(but, path);
            }

            // 3. Icon ben trai (18px)
            if (_iconTrai != LucideIconKind.None)
            {
                int szIc = this.Px(18);
                var vungIc = new RectangleF(this.Px(12), _vungKhung.Y + (_vungKhung.Height - szIc) / 2f, szIc, szIc);
                Color mauIc = coLoi ? AppTheme.Loi : _coFocus ? AppTheme.ChinhSang : AppTheme.ChuMo;
                LucideIcons.Draw(g, _iconTrai, vungIc, mauIc, 1.9f);
            }

            // 4. Nut phu (con mat / dau 'x')
            if (CoNutPhuHien && !_vungNutPhu.IsEmpty)
            {
                if (_hoverNutPhu)
                {
                    using var pBg = AppTheme.DuongBoGoc(_vungNutPhu, this.Px(5));
                    using var bBg = new SolidBrush(AppTheme.ChipXanh);
                    g.FillPath(bBg, pBg);
                }

                Color mauPhai = _hoverNutPhu ? AppTheme.ChinhSang : AppTheme.ChuPhu;
                int szPhu = this.Px(_matKhau ? 18 : 16);
                var vungIcon = new RectangleF(
                    _vungNutPhu.X + (_vungNutPhu.Width - szPhu) / 2f,
                    _vungNutPhu.Y + (_vungNutPhu.Height - szPhu) / 2f,
                    szPhu, szPhu);

                LucideIconKind kindPhu = _matKhau
                    ? (_o.UseSystemPasswordChar ? LucideIconKind.EyeOff : LucideIconKind.Eye)
                    : LucideIconKind.Close;
                LucideIcons.Draw(g, kindPhu, vungIcon, mauPhai, 1.9f);
            }

            // 5. Nut (i) mep phai
            if (CoIconInfo && !_vungInfo.IsEmpty)
            {
                using (var pInfoBg = AppTheme.DuongBoGoc(_vungInfo, this.Px(5)))
                using (var bInfoBg = new SolidBrush(_hoverInfo ? AppTheme.ChipXanh : AppTheme.NenTrang))
                {
                    g.FillPath(bInfoBg, pInfoBg);
                }

                Color mauInfo = _hoverInfo ? AppTheme.ChinhSang : AppTheme.ChuPhu;
                int szInfo = this.Px(18);
                var vungInfo = new RectangleF(
                    _vungInfo.X + (_vungInfo.Width - szInfo) / 2f,
                    _vungInfo.Y + (_vungInfo.Height - szInfo) / 2f,
                    szInfo, szInfo);
                LucideIcons.Draw(g, LucideIconKind.Info, vungInfo, mauInfo, 1.9f);
            }
        }
    }
}
