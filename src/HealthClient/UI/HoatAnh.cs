using System.Diagnostics;

namespace HealthClient.UI
{
    /// <summary>
    /// Bo hoat anh nhe dung chung: mot Timer cua WinForms (chay tren luong giao dien, 16 ms/lan, ~60 khung/giay),
    /// tu dung khi xong. Khong dung luong rieng, khong cham toi logic gui/nhan du lieu.
    /// Tu tat khi Windows tat hieu ung hoat anh (Settings > Accessibility > Visual effects)
    /// hoac khi dang chay qua Remote Desktop, de may cu va nguoi nhay cam chuyen dong van dung tot.
    /// </summary>
    public static class HoatAnh
    {
        public static bool DuocPhep =>
            SystemInformation.UIEffectsEnabled && !SystemInformation.TerminalServerSession;

        // v37: gioi han so hieu ung TRANG TRI chay cung luc (de khong nghen luong giao dien). Moi control co Timer hieu ung
        // phai goi TryBat() truoc khi Start va Tat() khi Stop; het cho thi bo qua hieu ung do (khong xep hang).
        public const int ToiDa = 2;
        private static int _dangChay;

        public static bool TryBat()
        {
            if (_dangChay >= ToiDa)
                return false;

            _dangChay++;
            return true;
        }

        public static void Tat()
        {
            if (_dangChay > 0)
                _dangChay--;
        }

        /// <summary>Nhanh luc dau, cham dan ve cuoi (cubic ease-out). t va ket qua deu tu 0 den 1.</summary>
        public static float RaCham(float t)
        {
            float k = 1f - Math.Clamp(t, 0f, 1f);
            return 1f - k * k * k;
        }

        /// <summary>
        /// Phan hoi ngay khi bam, ha canh rat em (quart ease-out). Dung cho chuyen canh bia chao (v22):
        /// ease-in-out cu bat dau cham nen co cam giac "tre" sau cu bam chuot.
        /// </summary>
        public static float RaEm(float t)
        {
            float k = 1f - Math.Clamp(t, 0f, 1f);
            return 1f - k * k * k * k;
        }

        /// <summary>Cham o hai dau, nhanh o giua (cubic ease-in-out): hop voi chuyen canh toan man hinh.</summary>
        public static float VaoRaMem(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t < 0.5f ? 4f * t * t * t : 1f - (float)Math.Pow(-2.0 * t + 2.0, 3.0) / 2f;
        }

        /// <summary>
        /// Chay hoat anh trong thoiGianMs. buoc nhan tien do 0..1 (da lam muot).
        /// Neu hieu ung bi tat thi nhay thang toi 1 va goi xong ngay.
        /// </summary>
        public static LuotChay? Chay(
            Control chu, int thoiGianMs, Action<float> buoc, Action? xong = null, Func<float, float>? lamMuot = null)
        {
            if (!DuocPhep || thoiGianMs <= 0)
            {
                buoc(1f);
                xong?.Invoke();
                return null;
            }

            return new LuotChay(chu, thoiGianMs, buoc, xong, lamMuot ?? RaCham);
        }
    }

    public sealed class LuotChay : IDisposable
    {
        private readonly System.Windows.Forms.Timer _bo = new() { Interval = 16 };
        private readonly Stopwatch _dongHo = Stopwatch.StartNew();
        private readonly Control _chu;
        private readonly int _thoiGianMs;
        private readonly Action<float> _buoc;
        private readonly Action? _xong;
        private readonly Func<float, float> _lamMuot;
        private bool _daDung;

        internal LuotChay(Control chu, int thoiGianMs, Action<float> buoc, Action? xong, Func<float, float> lamMuot)
        {
            _chu = chu;
            _thoiGianMs = thoiGianMs;
            _buoc = buoc;
            _xong = xong;
            _lamMuot = lamMuot;

            _bo.Tick += (s, e) => Nhip();
            _bo.Start();
        }

        private void Nhip()
        {
            if (_chu.IsDisposed)
            {
                Dung();
                return;
            }

            float t = Math.Min(1f, (float)_dongHo.ElapsedMilliseconds / _thoiGianMs);
            _buoc(_lamMuot(t));

            if (t >= 1f)
            {
                Dung();
                _xong?.Invoke();
            }
        }

        public void Dung()
        {
            if (_daDung)
                return;

            _daDung = true;
            _bo.Stop();
            _bo.Dispose();
        }

        public void Dispose() => Dung();
    }
}
