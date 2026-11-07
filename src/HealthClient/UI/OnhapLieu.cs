using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>Bieu tuong nho ve ben trai o nhap (theo mockup).</summary>
    public enum BieuTuongO
    {
        Khong,
        NguoiDung,
        KhoaMatKhau,
        ThuDienTu
    }

    /// <summary>
    /// O nhap lieu bo goc, chu 14pt, co bieu tuong ben trai. Vien doi mau khi dang nhap (teal)
    /// va khi co loi (do). CoNutHienAn them nut "con mat + chu Hien / An" ben phai o mat khau.
    /// </summary>
    public class OnhapLieu : Panel
    {
        private readonly TextBox _txt = new();
        private bool _coFocus;
        private bool _coLoi;
        private bool _coNutHienAn;
        private bool _chuotTrenNut;
        private bool _coNutXoa;
        private string _ghiChu = string.Empty;
        private int _vungChuot;   // 0 khong co, 1 nut xoa, 2 nut ghi chu (i)
        private bool _hover;
        private BieuTuongO _bieuTuong = BieuTuongO.Khong;

        public event EventHandler? NoiDungThayDoi;

        /// <summary>Nguoi dung bam vao bieu tuong (i) ben phai o nhap.</summary>
        public event EventHandler? NutGhiChuBam;

        public OnhapLieu()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            BackColor = Theme.NenTrang;
            Size = new Size(448, 46);

            _txt.BorderStyle = BorderStyle.None;
            _txt.Font = Theme.Chu(14F);
            _txt.BackColor = Theme.NenONhap;
            _txt.ForeColor = Theme.ChuChinh;
            _txt.Enter += (s, e) => { _coFocus = true; Invalidate(); };
            _txt.Leave += (s, e) => { _coFocus = false; Invalidate(); };
            _txt.TextChanged += (s, e) => { NoiDungThayDoi?.Invoke(this, EventArgs.Empty); Invalidate(); };
            _txt.MouseEnter += (s, e) => CapNhatHover();
            _txt.MouseLeave += (s, e) => CapNhatHover();

            Controls.Add(_txt);
            LayDatBoCuc();
        }

        // ----- Thuoc tinh giong TextBox de form dung quen tay -----

        [AllowNull]
        public override string Text
        {
            get => _txt.Text;
            set => _txt.Text = value ?? string.Empty;
        }

        [DefaultValue("")]
        public string PlaceholderText
        {
            get => _txt.PlaceholderText;
            set => _txt.PlaceholderText = value ?? string.Empty;
        }

        [DefaultValue(32767)]
        public int MaxLength
        {
            get => _txt.MaxLength;
            set => _txt.MaxLength = value;
        }

        [DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get => _txt.UseSystemPasswordChar;
            set
            {
                _txt.UseSystemPasswordChar = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool CoNutHienAn
        {
            get => _coNutHienAn;
            set
            {
                _coNutHienAn = value;
                LayDatBoCuc();
                Invalidate();
            }
        }

        /// <summary>Hien nut xoa nhanh (X) khi o co chu va dang duoc chon hoac re chuot.</summary>
        [DefaultValue(false)]
        public bool CoNutXoa
        {
            get => _coNutXoa;
            set
            {
                _coNutXoa = value;
                LayDatBoCuc();
                Invalidate();
            }
        }

        /// <summary>Khac rong thi hien bieu tuong (i); bam vao se phat su kien NutGhiChuBam.</summary>
        [DefaultValue("")]
        public string GhiChu
        {
            get => _ghiChu;
            set
            {
                _ghiChu = value ?? string.Empty;
                LayDatBoCuc();
                Invalidate();
            }
        }

        private bool CoNutGhiChu => _ghiChu.Length > 0;

        private bool HienNutXoa => _coNutXoa && _txt.Text.Length > 0 && (_coFocus || _hover);

        [DefaultValue(BieuTuongO.Khong)]
        public BieuTuongO BieuTuong
        {
            get => _bieuTuong;
            set
            {
                _bieuTuong = value;
                LayDatBoCuc();
                Invalidate();
            }
        }

        /// <summary>Dat true de vien chuyen sang mau do (ket hop nhan loi ben duoi).</summary>
        [DefaultValue(false)]
        public bool CoLoi
        {
            get => _coLoi;
            set
            {
                _coLoi = value;
                Invalidate();
            }
        }

        public void DatConTro() => _txt.Focus();

        public void Clear() => _txt.Clear();

        // ----- Bo cuc -----

        // Form co AutoScaleMode.Dpi se tu phong to ca control con. O nhap tu sap xep o con
        // bang LayDatBoCuc nen phai tat viec nay, neu khong o go chu bi phong to hai lan
        // va de len nut con mat.
        protected override bool ScaleChildren => false;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            LayDatBoCuc();
        }

        private int Px(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        private Rectangle VungNutHienAn() =>
            new Rectangle(Width - Px(92), 0, Px(92), Height);

        private Rectangle VungNutGhiChu()
        {
            int d = Px(22);

            // Co nut Hien/An thi dat (i) ngay ben trai nut do
            int phaiGoc = _coNutHienAn ? Width - Px(92) - Px(10) : Width - Px(14);
            return new Rectangle(phaiGoc - d, (Height - d) / 2, d, d);
        }

        /// <summary>Toa do X (pixel that, tinh tu mep trai o nhap) cua tam bieu tuong (i), de the ghi chu chi mui ten dung cho.</summary>
        public int TamNutGhiChuX()
        {
            Rectangle v = VungNutGhiChu();
            return v.X + v.Width / 2;
        }

        private Rectangle VungNutXoa()
        {
            int d = Px(20);
            int phaiGoc = CoNutGhiChu ? Width - Px(14) - Px(22) - Px(10) : Width - Px(14);
            return new Rectangle(phaiGoc - d, (Height - d) / 2, d, d);
        }

        private void LayDatBoCuc()
        {
            int trai = Px(14) + (_bieuTuong == BieuTuongO.Khong ? 0 : Px(22) + Px(10));
            int phai = _coNutHienAn ? Px(92) + Px(4) + (CoNutGhiChu ? Px(34) : 0) : Px(14) + (CoNutGhiChu ? Px(34) : 0) + (_coNutXoa ? Px(30) : 0);

            _txt.Width = Math.Max(10, Width - trai - phai);
            _txt.Location = new Point(trai, Math.Max(0, (Height - _txt.Height) / 2));
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            LayDatBoCuc();
        }

        // ----- Chuot -----

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _txt.Focus();
            base.OnMouseDown(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_coNutHienAn && VungNutHienAn().Contains(e.Location))
            {
                UseSystemPasswordChar = !UseSystemPasswordChar;
                _txt.Focus();
            }
            else if (CoNutGhiChu && VungNutGhiChu().Contains(e.Location))
            {
                NutGhiChuBam?.Invoke(this, EventArgs.Empty);
            }
            else if (HienNutXoa && VungNutXoa().Contains(e.Location))
            {
                _txt.Clear();
                _txt.Focus();
            }

            base.OnMouseClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool tren = _coNutHienAn && VungNutHienAn().Contains(e.Location);
            int vung = tren ? 0
                : CoNutGhiChu && VungNutGhiChu().Contains(e.Location) ? 2
                : HienNutXoa && VungNutXoa().Contains(e.Location) ? 1 : 0;

            if (tren != _chuotTrenNut || vung != _vungChuot)
            {
                _chuotTrenNut = tren;
                _vungChuot = vung;
                Cursor = (tren || vung != 0) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        // Re chuot vao o: vien doi sang xanh va co quang mem, de biet o nao dang duoc tro toi.
        private void CapNhatHover()
        {
            bool tren = IsHandleCreated && ClientRectangle.Contains(PointToClient(Cursor.Position));

            if (tren != _hover)
            {
                _hover = tren;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            CapNhatHover();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            CapNhatHover();
            if (_chuotTrenNut || _vungChuot != 0)
            {
                _chuotTrenNut = false;
                _vungChuot = 0;
                Cursor = Cursors.Default;
                Invalidate();
            }

            base.OnMouseLeave(e);
        }

        private Color _mauPhu = Color.Empty;

        /// <summary>v34: mau vien phu (vi du theo do manh mat khau). Color.Empty = mau mac dinh. Trang thai loi van duoc uu tien.</summary>
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color MauVienPhu
        {
            get => _mauPhu;
            set
            {
                if (_mauPhu != value)
                {
                    _mauPhu = value;
                    Invalidate();
                }
            }
        }

        // ----- Ve -----

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            var khung = new Rectangle(2, 2, Width - 5, Height - 5);
            // v29: o nhap mem hon: nen xanh rat nhat khi nghi, trang tinh khi dang go; vien manh nhe, goc bo lon.
            Color vienNghi = Color.FromArgb(203, 213, 230);
            Color mauVien = _coLoi ? Theme.Loi : !_mauPhu.IsEmpty ? _mauPhu : _coFocus ? Theme.Teal : _hover ? Theme.Tron(vienNghi, Theme.Teal, 0.55f) : vienNghi;
            float doDay = (_coLoi || _coFocus || !_mauPhu.IsEmpty) ? 2f : 1.25f;
            Color mauNenO = _coFocus ? Color.White : Theme.Tron(Theme.NenONhap, Theme.TealNhat, 0.32f);

            if (_txt.BackColor != mauNenO)
                _txt.BackColor = mauNenO;

            using (GraphicsPath duong = Theme.DuongBoGoc(khung, Px(12)))
            using (var nen = new SolidBrush(mauNenO))
            using (var but = new Pen(mauVien, doDay))
            {
                g.FillPath(nen, duong);

                // Vang sang mem quanh o dang go hoac dang bao loi, de nhin ra o can chu y.
                if (_coLoi || _coFocus || _hover)
                {
                    // Quang vang dong khi dang go / re chuot; bao loi thi quang do
                    Color mauQuang = _coLoi ? Theme.Loi : !_mauPhu.IsEmpty ? _mauPhu : Theme.Teal;

                    using (var vang = new Pen(Color.FromArgb((_coLoi || _coFocus) ? 40 : 0, mauQuang), 6f))
                    {
                        g.DrawPath(vang, duong);
                    }
                }

                g.DrawPath(but, duong);
            }

            Color mauIcon = _coLoi ? Theme.Loi : (_coFocus || _hover) ? Theme.Teal : Theme.ChuPhu;

            if (_bieuTuong != BieuTuongO.Khong)
                VeBieuTuong(g, new Rectangle(Px(14), (Height - Px(22)) / 2, Px(22), Px(22)), mauIcon);

            if (_coNutHienAn)
                VeNutHienAn(g);

            if (HienNutXoa)
                VeNutXoa(g);

            if (CoNutGhiChu)
                VeNutGhiChu(g);
        }

        private void VeNutXoa(Graphics g)
        {
            Rectangle v = VungNutXoa();
            Color nen = _vungChuot == 1 ? Theme.TealNhat2 : Theme.TealNhat;
            Color net = _vungChuot == 1 ? Theme.TealDam : Theme.ChuPhu;
            float m = v.Width * 0.30f;

            using (var co = new SolidBrush(nen))
            {
                g.FillEllipse(co, v);
            }

            using (var but = new Pen(net, 1.8f))
            {
                but.StartCap = LineCap.Round;
                but.EndCap = LineCap.Round;
                g.DrawLine(but, v.X + m, v.Y + m, v.Right - m, v.Bottom - m);
                g.DrawLine(but, v.Right - m, v.Y + m, v.X + m, v.Bottom - m);
            }
        }

        private void VeNutGhiChu(Graphics g)
        {
            Rectangle v = VungNutGhiChu();
            Color mau = _vungChuot == 2 ? Theme.TealDam : Theme.Teal;
            float cx = v.X + v.Width / 2f;

            using (var but = new Pen(mau, 1.8f))
            using (var cham = new SolidBrush(mau))
            {
                but.StartCap = LineCap.Round;
                but.EndCap = LineCap.Round;
                g.DrawEllipse(but, v.X + 1f, v.Y + 1f, v.Width - 2f, v.Height - 2f);
                g.DrawLine(but, cx, v.Y + v.Height * 0.46f, cx, v.Y + v.Height * 0.74f);
                g.FillEllipse(cham, cx - 1.7f, v.Y + v.Height * 0.22f, 3.4f, 3.4f);
            }
        }

        private void VeBieuTuong(Graphics g, Rectangle r, Color mau)
        {
            using var but = new Pen(mau, 2f);
            but.StartCap = LineCap.Round;
            but.EndCap = LineCap.Round;

            float k = r.Width / 22f;

            switch (_bieuTuong)
            {
                case BieuTuongO.NguoiDung:
                    g.DrawEllipse(but, r.X + 6 * k, r.Y + 1 * k, 10 * k, 10 * k);
                    g.DrawArc(but, r.X + 2 * k, r.Y + 13 * k, 18 * k, 16 * k, 180, 180);
                    break;

                case BieuTuongO.KhoaMatKhau:
                    g.DrawArc(but, r.X + 6 * k, r.Y + 1 * k, 10 * k, 14 * k, 180, 180);
                    using (GraphicsPath than = Theme.DuongBoGoc(
                        new Rectangle((int)(r.X + 3 * k), (int)(r.Y + 9 * k), (int)(16 * k), (int)(11 * k)),
                        Math.Max(2, (int)(3 * k))))
                    {
                        g.DrawPath(but, than);
                    }
                    using (var cham = new SolidBrush(mau))
                    {
                        g.FillEllipse(cham, r.X + 9.5f * k, r.Y + 12.5f * k, 3f * k, 3f * k);
                    }
                    break;

                case BieuTuongO.ThuDienTu:
                    g.DrawRectangle(but, r.X + 1 * k, r.Y + 4 * k, 20 * k, 14 * k);
                    g.DrawLines(but, new[]
                    {
                        new PointF(r.X + 1 * k, r.Y + 4 * k),
                        new PointF(r.X + 11 * k, r.Y + 12 * k),
                        new PointF(r.X + 21 * k, r.Y + 4 * k)
                    });
                    break;
            }
        }

        private void VeNutHienAn(Graphics g)
        {
            Rectangle v = VungNutHienAn();
            Color mau = _chuotTrenNut ? Theme.TealDam : Theme.Teal;
            float k = DeviceDpi / 96f;

            float x = v.X + 6 * k;
            float y = (Height - 20 * k) / 2f;

            using (var but = new Pen(mau, 2f))
            {
                but.StartCap = LineCap.Round;
                but.EndCap = LineCap.Round;
                g.DrawEllipse(but, x, y + 3 * k, 22 * k, 14 * k);
                using (var conNgoai = new SolidBrush(mau))
                {
                    g.FillEllipse(conNgoai, x + 8 * k, y + 6.5f * k, 7 * k, 7 * k);
                }

                // Dang an mat khau: gach cheo tren con mat
                if (UseSystemPasswordChar)
                    g.DrawLine(but, x + 2 * k, y + 18 * k, x + 20 * k, y + 1 * k);
            }

            TextRenderer.DrawText(
                g, UseSystemPasswordChar ? "Hiện" : "Ẩn", Theme.Chu(11F, FontStyle.Bold),
                new Rectangle((int)(x + 26 * k), 0, v.Right - (int)(x + 26 * k), Height), mau,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
