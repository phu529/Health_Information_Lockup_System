using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Thanh dieu huong truot ngang Ban 8:
    //  - ThoiGianTruotMs = 170ms EaseOutCubic (dong bo tuyet doi voi HieuUngChuyen 170ms, khong tre)
    internal sealed class TabSwitch : Control
    {
        private const int ThoiGianTruotMs = 170;
        private const int ThoiGianHoverMs = 100;

        private readonly string[] _muc;
        private readonly float[] _hoverT;
        private readonly System.Windows.Forms.Timer _nhip = new() { Interval = 16 };
        private readonly System.Windows.Forms.Timer _nhipHover = new() { Interval = 16 };
        private readonly Stopwatch _dongHo = new();
        private readonly Stopwatch _dongHoHover = new();

        private int _hover = -1;
        private int _chiSo;
        private bool _canGiuaTheoChu;
        private bool _veDuongKeDay = true;

        private float _gachX, _gachW;
        private float _tuX, _tuW, _denX, _denW;
        private bool _daKhoiTao;

        public event Action<int>? DuocChon;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ChiSo
        {
            get => _chiSo;
            set
            {
                int moi = Math.Clamp(value, 0, Math.Max(0, _muc.Length - 1));
                if (moi == _chiSo && _daKhoiTao) return;
                _chiSo = moi;
                BatDauTruot();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGiuaTheoChu
        {
            get => _canGiuaTheoChu;
            set { _canGiuaTheoChu = value; DatNgayGach(); Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool VeDuongKeDay
        {
            get => _veDuongKeDay;
            set { _veDuongKeDay = value; Invalidate(); }
        }

        public TabSwitch(params string[] muc)
        {
            _muc = muc;
            _hoverT = new float[muc.Length];
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = AppTheme.ChuTab;
            Height = 48;
            _nhip.Tick += (_, _) => Nhip();
            _nhipHover.Tick += (_, _) => NhipHover();
        }

        public void DatChiSoKhongTruot(int chiSo)
        {
            _chiSo = Math.Clamp(chiSo, 0, Math.Max(0, _muc.Length - 1));
            _nhip.Stop();
            DatNgayGach();
            Invalidate();
        }

        public void TruotTu(int chiSoCu, int chiSoMoi)
        {
            if (_muc.Length == 0) return;
            _chiSo = Math.Clamp(chiSoCu, 0, _muc.Length - 1);
            DatNgayGach();
            ChiSo = chiSoMoi;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nhip.Stop();
                _nhip.Dispose();
                _nhipHover.Stop();
                _nhipHover.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                _nhip.Stop();
                _nhipHover.Stop();
                DatNgayGach();
                for (int i = 0; i < _hoverT.Length; i++)
                    _hoverT[i] = 0f;
            }
        }

        private Rectangle[] TinhCacO()
        {
            var kq = new Rectangle[_muc.Length];
            if (_muc.Length == 0) return kq;

            if (!_canGiuaTheoChu)
            {
                int w = Math.Max(1, Width / _muc.Length);
                for (int i = 0; i < _muc.Length; i++)
                    kq[i] = new Rectangle(i * w, 0, i == _muc.Length - 1 ? Width - i * w : w, Height);
                return kq;
            }

            int padNgang = this.Px(16);
            int gap = this.Px(10);
            int tong = 0;
            var wMuc = new int[_muc.Length];
            for (int i = 0; i < _muc.Length; i++)
            {
                wMuc[i] = TextRenderer.MeasureText(_muc[i], AppTheme.ChuTab, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width + padNgang * 2;
                tong += wMuc[i];
                if (i > 0) tong += gap;
            }

            int x = Math.Max(0, (Width - tong) / 2);
            for (int i = 0; i < _muc.Length; i++)
            {
                kq[i] = new Rectangle(x, 0, wMuc[i], Height);
                x += wMuc[i] + gap;
            }
            return kq;
        }

        private (float X, float W) MucTieuGach()
        {
            var cacO = TinhCacO();
            if (_chiSo < 0 || _chiSo >= cacO.Length) return (0, 0);
            Rectangle sel = cacO[_chiSo];
            int rong = _canGiuaTheoChu
                ? Math.Max(24, sel.Width - this.Px(6))
                : Math.Max(24, sel.Width - this.Px(12));
            return (sel.X + (sel.Width - rong) / 2f, rong);
        }

        private void DatNgayGach()
        {
            (_gachX, _gachW) = MucTieuGach();
            _daKhoiTao = Width > 0;
        }

        private void BatDauTruot()
        {
            var (dichX, dichW) = MucTieuGach();
            if (!_daKhoiTao || !IsHandleCreated || !Visible || Width <= 0 || !SystemInformation.UIEffectsEnabled)
            {
                _nhip.Stop();
                _gachX = dichX;
                _gachW = dichW;
                _daKhoiTao = Width > 0;
                return;
            }

            _tuX = _gachX; _tuW = _gachW;
            _denX = dichX; _denW = dichW;
            _dongHo.Restart();
            _nhip.Start();
        }

        private void Nhip()
        {
            float t = Math.Clamp(_dongHo.ElapsedMilliseconds / (float)ThoiGianTruotMs, 0f, 1f);
            float k = AppTheme.EaseOutCubic(t);
            _gachX = _tuX + (_denX - _tuX) * k;
            _gachW = _tuW + (_denW - _tuW) * k;
            if (t >= 1f)
            {
                _gachX = _denX;
                _gachW = _denW;
                _nhip.Stop();
            }
            Invalidate();
        }

        private void NhipHover()
        {
            if (!Visible)
            {
                _nhipHover.Stop();
                return;
            }

            float buoc = Math.Clamp(_dongHoHover.ElapsedMilliseconds / (float)ThoiGianHoverMs, 0.1f, 1f);
            _dongHoHover.Restart();

            bool tiepTuc = false;
            for (int i = 0; i < _hoverT.Length; i++)
            {
                float dich = (i == _hover && i != _chiSo) ? 1f : 0f;
                float lech = dich - _hoverT[i];
                if (Math.Abs(lech) <= buoc)
                    _hoverT[i] = dich;
                else
                {
                    _hoverT[i] += lech > 0 ? buoc : -buoc;
                    tiepTuc = true;
                }
            }

            if (!tiepTuc) _nhipHover.Stop();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!_nhip.Enabled) DatNgayGach();
        }

        private int ChiSoTai(Point pt)
        {
            var cacO = TinhCacO();
            for (int i = 0; i < cacO.Length; i++)
                if (cacO[i].Contains(pt)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int i = ChiSoTai(e.Location);
            if (i != _hover)
            {
                _hover = i;
                if (Visible && SystemInformation.UIEffectsEnabled)
                {
                    _dongHoHover.Restart();
                    _nhipHover.Start();
                }
                else
                {
                    for (int k = 0; k < _hoverT.Length; k++)
                        _hoverT[k] = (k == _hover && k != _chiSo) ? 1f : 0f;
                    Invalidate();
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (_hover != -1)
            {
                _hover = -1;
                if (Visible && SystemInformation.UIEffectsEnabled)
                {
                    _dongHoHover.Restart();
                    _nhipHover.Start();
                }
                else
                {
                    Array.Clear(_hoverT);
                    Invalidate();
                }
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int i = ChiSoTai(e.Location);
            if (i >= 0 && i != _chiSo)
            {
                ChiSo = i;
                Update();
                BeginInvoke(new Action(() => DuocChon?.Invoke(i)));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.NenTrang);
            if (!_daKhoiTao) DatNgayGach();

            if (_veDuongKeDay)
            {
                using var vien = new Pen(AppTheme.Vien);
                g.DrawLine(vien, 0, Height - 1, Width, Height - 1);
            }

            if (_gachW >= 8f)
            {
                int padY = _canGiuaTheoChu ? Math.Max(4, (Height - this.Px(36)) / 2) : this.Px(4);
                int bottomMargin = _veDuongKeDay ? this.Px(5) : this.Px(4);
                float hPill = Math.Max(this.Px(22), Height - padY - bottomMargin);
                var rPill = new RectangleF(_gachX, padY, _gachW, hPill);
                using var pPill = AppTheme.DuongBoGoc(rPill, this.Px(AppTheme.BoGoc));
                using var bgPill = new SolidBrush(AppTheme.ChipXanh);
                g.FillPath(bgPill, pPill);
            }

            var cacO = TinhCacO();
            for (int i = 0; i < cacO.Length; i++)
            {
                Rectangle o = cacO[i];
                if (i != _chiSo && _hoverT[i] > 0.02f)
                {
                    int padY = _canGiuaTheoChu ? Math.Max(4, (Height - this.Px(36)) / 2) : this.Px(4);
                    int bottomMargin = _veDuongKeDay ? this.Px(5) : this.Px(4);
                    var rHov = new RectangleF(o.X + this.Px(3), padY, Math.Max(8, o.Width - this.Px(6)), Math.Max(8, Height - padY - bottomMargin));
                    using var pHov = AppTheme.DuongBoGoc(rHov, this.Px(AppTheme.BoGoc));
                    using var bHov = new SolidBrush(Color.FromArgb((int)(120 * _hoverT[i]), AppTheme.NenHover));
                    g.FillPath(bHov, pHov);
                }

                Font fTab = i == _chiSo ? AppTheme.ChuTab : AppTheme.ChuTabPhu;
                Color chu = i == _chiSo
                    ? AppTheme.Chinh
                    : AppTheme.Tron(AppTheme.ChuPhu, AppTheme.TieuDeMau, _hoverT[i]);

                TextRenderer.DrawText(g, _muc[i], fTab, o, chu,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }

            if (_gachW >= 2f)
            {
                float day = this.Px(3);
                float yGach = Height - day - (_veDuongKeDay ? 1f : 0f);
                var rBar = new RectangleF(_gachX, yGach, _gachW, day);
                using var pBar = AppTheme.DuongBoGoc(rBar, day / 2f);
                using var bgBar = new SolidBrush(AppTheme.ChinhSang);
                g.FillPath(bgBar, pBar);
            }
        }
    }
}
