using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Khung chung cho Dang nhap va Dang ky Ban 6:
    //  - Nen gradient xanh sang dong nhat lan rong ca 2 ben (khong chia doi che man hinh)
    //  - Cot form ben phai nam trong khung the trang mem (the noi nhe), bo goc 14px, padding thoang
    //  - TaoTieuDe cao 48px, TaoMoTa cao 34px de khong bao gio bi che khuat dau tieng Viet.
    internal abstract class TrangXacThuc : UserControl, ITrang
    {
        protected readonly FrmVo Shell;
        protected readonly BrandPanel Trai = new();
        protected readonly Panel Phai = new();
        protected readonly Panel TheForm = new();
        protected readonly Panel Cot = new();
        protected readonly TabSwitch Tab = new("Đăng nhập", "Đăng ký");
        private readonly TheGoiYNoi _goiYNoi = new();
        private readonly int _chiSoTab;
        private int _tabTruoc;

        public virtual IButtonControl? NutMacDinh => null;
        public virtual FormField? OBatDau => null;

        private static void BatDoubleBuffer(Control c) =>
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(c, true);

        protected TrangXacThuc(FrmVo shell, int chiSoTab, int tabTruoc = -1, int chieuRongCot = 440)
        {
            Shell = shell;
            _chiSoTab = chiSoTab;
            _tabTruoc = tabTruoc;

            Dock = DockStyle.Fill;
            BackColor = AppTheme.NenGradTren;

            Trai.Dock = DockStyle.Left;
            Trai.Width = this.Px(376);
            Trai.QuayVeClick += (_, _) => Shell.HienKhoiDau();

            Phai.Dock = DockStyle.Fill;
            Phai.BackColor = AppTheme.NenGradTren;
            Phai.AutoScroll = true;
            BatDoubleBuffer(Phai);
            Phai.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var bg = new LinearGradientBrush(
                           new Rectangle(0, 0, Math.Max(1, Phai.Width), Math.Max(1, Phai.Height)),
                           AppTheme.NenGradTren, AppTheme.NenGradDuoi, LinearGradientMode.Vertical))
                    g.FillRectangle(bg, Phai.ClientRectangle);

                // Song mo tiep noi tu cot trai sang phai
                using (var b1 = new SolidBrush(Color.FromArgb(24, AppTheme.ChinhSang)))
                    g.FillEllipse(b1, Phai.Width - this.Px(220), -this.Px(90), this.Px(320), this.Px(320));
                using (var b2 = new SolidBrush(Color.FromArgb(22, AppTheme.XanhPhu)))
                    g.FillEllipse(b2, -this.Px(140), Phai.Height - this.Px(220), this.Px(360), this.Px(300));
            };

            TheForm.BackColor = AppTheme.NenTrang;
            BatDoubleBuffer(TheForm);
            TheForm.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, TheForm.Width - 1.5f, TheForm.Height - 1.5f);
                using var path = AppTheme.DuongBoGoc(r, this.Px(14));
                using var bg = new SolidBrush(AppTheme.NenTrang);
                using var pen = new Pen(Color.FromArgb(190, AppTheme.Vien), 1f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
            };

            Cot.BackColor = AppTheme.NenTrang;
            Cot.Width = this.Px(chieuRongCot);

            Tab.DatChiSoKhongTruot((tabTruoc >= 0 && tabTruoc != chiSoTab) ? tabTruoc : chiSoTab);
            Tab.Bounds = new Rectangle(0, 0, Cot.Width, this.Px(46));
            Tab.DuocChon += i => { if (i == 0) Shell.HienDangNhap(); else Shell.HienDangKy(); };
            Cot.Controls.Add(Tab);

            TheForm.Controls.Add(Cot);
            Phai.Controls.Add(TheForm);
            Phai.Controls.Add(_goiYNoi);
            Phai.Resize += (_, _) => { _goiYNoi.An(); CanGiua(); };
            Phai.MouseDown += (_, _) => _goiYNoi.An();
            TheForm.MouseDown += (_, _) => _goiYNoi.An();
            Cot.MouseDown += (_, _) => _goiYNoi.An();
            Phai.Scroll += (_, _) => _goiYNoi.An();

            Controls.Add(Phai);
            Controls.Add(Trai);

            Resize += (_, _) =>
            {
                Trai.Width = Math.Max(this.Px(270), Math.Min(this.Px(384), (int)(Width * 0.37)));
                _goiYNoi.An();
                CanGiua();
            };
        }

        public void HoanTatChuyenMan()
        {
            if (IsDisposed) return;
            if (_tabTruoc >= 0 && _tabTruoc != _chiSoTab)
            {
                if (Visible && IsHandleCreated)
                    Tab.TruotTu(_tabTruoc, _chiSoTab);
                else
                    Tab.DatChiSoKhongTruot(_chiSoTab);
                _tabTruoc = -1;
            }
            else
            {
                Tab.DatChiSoKhongTruot(_chiSoTab);
            }
        }

        protected void CanGiua()
        {
            int padTheX = this.Px(28);
            int padTheY = this.Px(20);
            TheForm.Size = new Size(Cot.Width + padTheX * 2, Cot.Height + padTheY * 2);
            Cot.Location = new Point(padTheX, padTheY);

            int leDoc = this.Px(14);
            int wCan = TheForm.Width + this.Px(28);
            Phai.AutoScrollMinSize = new Size(
                wCan > Phai.ClientSize.Width ? wCan : 0,
                TheForm.Height + leDoc * 2);

            TheForm.Left = Math.Max(this.Px(14), (Phai.ClientSize.Width - TheForm.Width) / 2) + Phai.AutoScrollPosition.X;
            TheForm.Top = Math.Max(leDoc, (Phai.ClientSize.Height - TheForm.Height) / 2) + Phai.AutoScrollPosition.Y;
        }

        protected void GanGoiY(FormField o, string tieuDe, params string[] dong)
        {
            o.TieuDeGoiY = tieuDe;
            o.NoiDungGoiY = dong;

            Rectangle VungIconTrongPhai()
            {
                Rectangle ic = o.LayVungIconGoiY();
                return new Rectangle(
                    TheForm.Left + Cot.Left + o.Left + ic.Left,
                    TheForm.Top + Cot.Top + o.Top + ic.Top,
                    ic.Width, ic.Height);
            }

            o.GoiYHoverChanged += (_, dangHover) =>
            {
                if (dangHover)
                {
                    if (!_goiYNoi.DangGhim)
                        _goiYNoi.HienTai(Phai, VungIconTrongPhai(), tieuDe, dong, ghim: false);
                }
                else
                {
                    _goiYNoi.HenAnNeuKhongGhim();
                }
            };

            o.GoiYClicked += (_, _) =>
            {
                if (_goiYNoi.Visible && _goiYNoi.DangGhim)
                    _goiYNoi.An();
                else
                    _goiYNoi.HienTai(Phai, VungIconTrongPhai(), tieuDe, dong, ghim: true);
            };

            o.Enter += (_, _) =>
            {
                if (_goiYNoi.DangGhim) _goiYNoi.An();
            };
        }

        protected Label TaoTieuDe(string chu, int y) => new()
        {
            AutoSize = false,
            UseMnemonic = false,
            Font = AppTheme.TieuDe,
            ForeColor = AppTheme.TieuDeMau,
            Text = chu,
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds = new Rectangle(0, y, Cot.Width, this.Px(46))
        };

        protected Label TaoMoTa(string chu, int y, int cao = 34) => new()
        {
            AutoSize = false,
            UseMnemonic = false,
            Font = AppTheme.ChuThuong,
            ForeColor = AppTheme.ChuPhu,
            Text = chu,
            TextAlign = ContentAlignment.TopLeft,
            Bounds = new Rectangle(0, y, Cot.Width, this.Px(cao))
        };

        protected void ThemDongLienKet(string chu, string lienKet, int y, Action khiBam)
        {
            var dong = new DongLienKet(chu, lienKet, khiBam)
            {
                Bounds = new Rectangle(0, y, Cot.Width, this.Px(28)),
                BackColor = AppTheme.NenTrang
            };
            Cot.Controls.Add(dong);
        }

        private sealed class DongLienKet : Control
        {
            private readonly string _chu;
            private readonly string _lienKet;
            private readonly Action _khiBam;
            private Rectangle _vungLink;
            private bool _hoverLink;

            public DongLienKet(string chu, string lienKet, Action khiBam)
            {
                _chu = chu;
                _lienKet = lienKet;
                _khiBam = khiBam;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            private void TinhVung(Graphics g, out int xChu, out int wChu, out int xLink, out int wLink)
            {
                const TextFormatFlags co = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
                wChu = TextRenderer.MeasureText(g, _chu, AppTheme.ChuThuong, Size.Empty, co).Width;
                wLink = TextRenderer.MeasureText(g, _lienKet, AppTheme.ChuDam, Size.Empty, co).Width;
                int gap = this.Px(6);
                int tong = wChu + gap + wLink;
                xChu = Math.Max(0, (Width - tong) / 2);
                xLink = xChu + wChu + gap;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                bool tren = _vungLink.Contains(e.Location);
                if (tren != _hoverLink)
                {
                    _hoverLink = tren;
                    Cursor = tren ? Cursors.Hand : Cursors.Default;
                    Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                if (_hoverLink)
                {
                    _hoverLink = false;
                    Cursor = Cursors.Default;
                    Invalidate();
                }
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                if (_vungLink.Contains(e.Location))
                    _khiBam();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Parent?.BackColor ?? AppTheme.NenTrang);

                TinhVung(g, out int xChu, out int wChu, out int xLink, out int wLink);

                const TextFormatFlags co = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                                           TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

                TextRenderer.DrawText(g, _chu, AppTheme.ChuThuong,
                    new Rectangle(xChu, 0, wChu + this.Px(2), Height), AppTheme.ChuPhu, co);

                Color mauLink = _hoverLink ? AppTheme.ChinhSang : AppTheme.Chinh;
                TextRenderer.DrawText(g, _lienKet, AppTheme.ChuDam,
                    new Rectangle(xLink, 0, wLink + this.Px(2), Height), mauLink, co);

                int hLink = TextRenderer.MeasureText(g, _lienKet, AppTheme.ChuDam, Size.Empty, TextFormatFlags.NoPadding).Height;
                _vungLink = new Rectangle(xLink - this.Px(2), 0, wLink + this.Px(6), Height);

                if (_hoverLink)
                {
                    float yGach = (Height + hLink) / 2f + this.Px(1);
                    using var p = new Pen(mauLink, 1.4f);
                    g.DrawLine(p, xLink, yGach, xLink + wLink, yGach);
                }
            }
        }
    }
}
