using System.Diagnostics;
using System.Drawing;

namespace HealthClient.UI
{
    /// <summary>
    /// Mot dong chu "dong" (v35), lay y tuong tu hieu ung chu cua coolors.co: re chuot vao dong chu thi cac chu gan con tro
    /// LEN MAU (bang mau PhoMau) va LAC nhe len xuong; bo chuot ra thi chu lang dan ve mau cu va dung yen.
    ///
    /// Hieu nang (ngan sach quy tac 13):
    ///  - Khong phai control: chu cua no (Panel...) ve giup, nen khong them cua so nao.
    ///  - Mot Timer 30 ms, CHI chay khi chuot dang tren dong chu hoac chu con dang lang dan; xong la tu dung.
    ///  - Chi ve lai dung vung dong chu (Invalidate co hinh chu nhat), khong ve lai ca bang.
    ///  - Xin cho tu GioiHanHoatAnh (toi da 2 hieu ung cung luc); het cho thi chi doi mau, khong lac.
    ///  - Dem be rong tung chu (chi do lai khi doi chu / phong / DPI); luc nghi ve ca chuoi mot lan.
    ///  - Dung hay thu nho cua so, hoac Windows tat hieu ung: dung han.
    /// </summary>
    public sealed class ChuDong : IDisposable
    {
        private readonly Control _chu;
        private readonly System.Windows.Forms.Timer _bo = new() { Interval = 30 };
        private readonly Stopwatch _dongHo = Stopwatch.StartNew();

        private string _t = string.Empty;
        private Font _phong = SystemFonts.DefaultFont;
        private Rectangle _vung = Rectangle.Empty;
        private bool _giua = true;
        private bool _nenToi = true;

        private int[] _truoc = Array.Empty<int>();
        private string _truocChuoi = string.Empty;
        private Font? _truocPhong;
        private int _truocDpi;

        private float[] _muc = Array.Empty<float>();
        private bool _tren;
        private Point _chuot;
        private bool _giuCho;
        private double _tCuoi;
        private bool _lac;

        public ChuDong(Control chu)
        {
            _chu = chu;
            _bo.Tick += (s, e) => Nhip();
        }

        private int Px(int v) => (int)Math.Round(v * _chu.DeviceDpi / 96.0);

        /// <summary>Dat noi dung, phong va vung ve. Goi trong OnPaint cua control cha; chi doi trang thai khi that su thay doi.</summary>
        public void Dat(string chu, Font phong, Rectangle vung, bool giua, bool nenToi)
        {
            if (_t != chu)
            {
                _t = chu;
                _muc = new float[chu.Length];
            }

            _phong = phong;
            _vung = vung;
            _giua = giua;
            _nenToi = nenToi;
        }

        private Rectangle VungVe()
        {
            Rectangle r = _vung;
            r.Inflate(Px(4), Px(7));
            return r;
        }

        // ----- Chuot -----

        public void ChuotDi(Point p)
        {
            _chuot = p;
            bool tren = _vung.Contains(p);

            if (tren && !_tren)
            {
                _tren = true;
                Bat();
            }
            else if (!tren && _tren)
            {
                _tren = false;
            }
        }

        public void ChuotRoi() => _tren = false;

        private void Bat()
        {
            if (_bo.Enabled || _t.Length == 0 || !HoatAnh.DuocPhep)
                return;

            _lac = GioiHanHoatAnh.Xin();   // het cho: chi doi mau, khong lac
            _giuCho = _lac;
            _tCuoi = _dongHo.Elapsed.TotalSeconds;
            _bo.Start();
        }

        private void Dung()
        {
            _bo.Stop();

            if (_giuCho)
            {
                GioiHanHoatAnh.Tra();
                _giuCho = false;
            }

            for (int i = 0; i < _muc.Length; i++)
                _muc[i] = 0f;

            _chu.Invalidate(VungVe());
        }

        private void Nhip()
        {
            Form? f = _chu.FindForm();

            if (f == null || f.WindowState == FormWindowState.Minimized || Form.ActiveForm != f || _truoc.Length != _t.Length + 1)
            {
                Dung();
                return;
            }

            double giay = _dongHo.Elapsed.TotalSeconds;
            float dt = (float)Math.Clamp(giay - _tCuoi, 0, 0.1);
            _tCuoi = giay;

            float banKinh = Px(64);
            float giuaY = _vung.Y + _vung.Height / 2f;
            float lonNhat = 0f;
            int x0 = DiemDau();

            for (int i = 0; i < _t.Length; i++)
            {
                float muc = 0f;

                if (_tren && !char.IsWhiteSpace(_t[i]))
                {
                    float tamX = x0 + _truoc[i] + (_truoc[i + 1] - _truoc[i]) / 2f;
                    float dx = _chuot.X - tamX;
                    float dy = (_chuot.Y - giuaY) * 0.6f;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float u = Math.Clamp(1f - d / banKinh, 0f, 1f);
                    muc = u * u * (3f - 2f * u);
                }

                float cu = _muc[i];
                _muc[i] = muc > cu ? Math.Min(muc, cu + dt / 0.09f) : Math.Max(muc, cu - dt / 0.50f);
                lonNhat = Math.Max(lonNhat, _muc[i]);
            }

            _chu.Invalidate(VungVe());

            if (!_tren && lonNhat <= 0.003f)
                Dung();
        }

        // ----- Ve -----

        private int DiemDau() => _giua ? _vung.X + (_vung.Width - _truoc[_t.Length]) / 2 : _vung.X;

        private void DoChu(Graphics g)
        {
            if (ReferenceEquals(_truocPhong, _phong) && _truocChuoi == _t && _truocDpi == _chu.DeviceDpi && _truoc.Length == _t.Length + 1)
                return;

            var vo = new Size(int.MaxValue, int.MaxValue);
            const TextFormatFlags dd = TextFormatFlags.NoPadding;
            int gach = TextRenderer.MeasureText(g, "||", _phong, vo, dd).Width;
            _truoc = new int[_t.Length + 1];

            for (int i = 1; i <= _t.Length; i++)
                _truoc[i] = TextRenderer.MeasureText(g, "|" + _t.Substring(0, i) + "|", _phong, vo, dd).Width - gach;

            _truocPhong = _phong;
            _truocChuoi = _t;
            _truocDpi = _chu.DeviceDpi;
        }

        /// <summary>Ve dong chu. mauNen la mau luc nghi (chua len mau).</summary>
        public void Ve(Graphics g, Color mauNen)
        {
            if (_t.Length == 0 || _vung.IsEmpty)
                return;

            DoChu(g);

            const TextFormatFlags dd = TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            int x0 = DiemDau();
            int n = _t.Length;
            bool coMau = _tren;

            for (int j = 0; !coMau && j < _muc.Length; j++)
                if (_muc[j] > 0.003f)
                    coMau = true;

            if (!coMau)
            {
                TextRenderer.DrawText(g, _t, _phong, new Rectangle(x0, _vung.Y, _truoc[n] + 8, _vung.Height), mauNen, dd);
                return;
            }

            double giay = _dongHo.Elapsed.TotalSeconds;
            float bienDo = Px(3);

            for (int i = 0; i < n; i++)
            {
                if (char.IsWhiteSpace(_t[i]))
                    continue;

                float muc = _muc[i];
                Color mau = muc > 0.003f ? Theme.Tron(mauNen, PhoMau.Lay(i / (float)Math.Max(1, n - 1), _nenToi), muc) : mauNen;
                int dy = _lac ? (int)Math.Round(Math.Sin(giay * 17.0 + i * 0.85) * bienDo * muc) : 0;

                TextRenderer.DrawText(
                    g, _t[i].ToString(), _phong,
                    new Rectangle(x0 + _truoc[i], _vung.Y + dy, _truoc[i + 1] - _truoc[i] + 8, _vung.Height), mau, dd);
            }
        }

        public void Dispose()
        {
            _bo.Stop();
            _bo.Dispose();

            if (_giuCho)
            {
                GioiHanHoatAnh.Tra();
                _giuCho = false;
            }
        }
    }
}
