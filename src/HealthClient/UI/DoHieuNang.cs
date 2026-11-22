using System.Diagnostics;
using System.Drawing;

namespace HealthClient.UI
{
    /// <summary>
    /// Bang do hieu nang tich hop (v35). Bam CTRL + SHIFT + F9 o bat ky man hinh nao de bat / tat mot bang nho goc tren phai,
    /// cap nhat moi giay: CPU cua tien trinh (% tren TONG so luong logic, cung cach tinh voi Task Manager), CPU lon nhat trong
    /// 10 giay gan nhat, RAM (Working Set, MB) va so hieu ung dang chay. Chu xanh = dat tran, chu do = vuot tran
    /// (CPU &gt; 10% hoac RAM &gt; 80 MB, quy tac 13 trong nhat ky).
    ///
    /// Chi ton tai khi nguoi dung bat: khong bat thi khong Timer, khong cua so. Bang khong gianh tieu diem (WS_EX_NOACTIVATE) de
    /// khong lam cac hieu ung khac tuong la cua so mat tieu diem. Chi doc so lieu, khong cham toi logic ung dung.
    /// </summary>
    public static class DoHieuNang
    {
        private static bool _daGan;
        private static Form? _bang;

        /// <summary>Goi mot lan (an toan khi goi nhieu lan) de lang nghe phim tat.</summary>
        public static void KichHoat()
        {
            if (_daGan)
                return;

            _daGan = true;
            Application.AddMessageFilter(new BoLoc());
        }

        private static void Chuyen()
        {
            if (_bang != null && !_bang.IsDisposed)
            {
                _bang.Close();
                _bang = null;
                return;
            }

            _bang = new BangDo();
            _bang.Show();
        }

        private sealed class BoLoc : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                const int WM_KEYDOWN = 0x0100;

                if (m.Msg == WM_KEYDOWN &&
                    (Keys)(int)m.WParam == Keys.F9 &&
                    (Control.ModifierKeys & (Keys.Control | Keys.Shift)) == (Keys.Control | Keys.Shift))
                {
                    Chuyen();
                    return true;
                }

                return false;
            }
        }

        private sealed class BangDo : Form
        {
            private readonly Label _nhan = new();
            private readonly Font _phong = new("Consolas", 9.5F);
            private readonly System.Windows.Forms.Timer _bo = new() { Interval = 1000 };
            private readonly Process _tienTrinh = Process.GetCurrentProcess();
            private readonly Stopwatch _dongHo = Stopwatch.StartNew();
            private readonly Queue<double> _cpu10 = new();
            private TimeSpan _cpuTruoc;
            private double _tTruoc;

            public BangDo()
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                TopMost = true;
                ShowInTaskbar = false;
                BackColor = Color.FromArgb(17, 24, 39);
                Size = new Size(380, 82);

                Rectangle man = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
                Location = new Point(man.Right - Width - 12, man.Top + 12);

                _nhan.Dock = DockStyle.Fill;
                _nhan.Font = _phong;
                _nhan.Padding = new Padding(10, 8, 10, 8);
                Controls.Add(_nhan);

                _tienTrinh.Refresh();
                _cpuTruoc = _tienTrinh.TotalProcessorTime;
                _tTruoc = 0;

                _bo.Tick += (s, e) => CapNhat();
                _bo.Start();
                CapNhat();
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000;   // WS_EX_NOACTIVATE
                    return cp;
                }
            }

            private void CapNhat()
            {
                _tienTrinh.Refresh();
                TimeSpan cpu = _tienTrinh.TotalProcessorTime;
                double t = _dongHo.Elapsed.TotalSeconds;
                double dt = Math.Max(0.001, t - _tTruoc);
                double phanTram = (cpu - _cpuTruoc).TotalSeconds / dt / Environment.ProcessorCount * 100.0;

                _cpuTruoc = cpu;
                _tTruoc = t;

                _cpu10.Enqueue(phanTram);

                if (_cpu10.Count > 10)
                    _cpu10.Dequeue();

                double toiDa = _cpu10.Max();
                double ram = _tienTrinh.WorkingSet64 / 1048576.0;
                bool vuot = toiDa > 10.0 || ram > 80.0;

                _nhan.ForeColor = vuot ? Color.FromArgb(252, 165, 165) : Color.FromArgb(134, 239, 172);
                _nhan.Text =
                    $"CPU {phanTram,5:0.0}%   (tối đa 10 giây qua: {toiDa:0.0}%, trần 10%)\n" +
                    $"RAM {ram,5:0.0} MB   (trần 80 MB)\n" +
                    $"Hiệu ứng đang chạy: {GioiHanHoatAnh.Dang}/{GioiHanHoatAnh.ToiDa}   [Ctrl+Shift+F9: tắt]";
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _bo.Stop();
                    _bo.Dispose();
                    _tienTrinh.Dispose();
                    _phong.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
