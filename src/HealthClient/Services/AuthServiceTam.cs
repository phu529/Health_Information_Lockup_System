namespace HealthClient.Services
{
    /// <summary>
    /// TAM THOI, chi de thu giao dien khi chua co server.
    /// Tai khoan co san: demo / demo1234. OTP gia lap: 123456.
    /// </summary>
    public sealed class AuthServiceTam : IAuthService
    {
        private const int DO_TRE_MS = 700;

        private readonly Dictionary<string, (string MatKhau, string HoTen)> _tk =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["demo"] = ("demo1234", "Người dùng thử")
            };

        private int _mockOtpFailures;

        public async Task<KetQuaXacThuc> DangNhapAsync(
            string tenDangNhap, string matKhau, CancellationToken token = default)
        {
            await Task.Delay(DO_TRE_MS, token);

            if (tenDangNhap.Equals("loimang", StringComparison.OrdinalIgnoreCase))
                return new KetQuaXacThuc(MaKetQua.LoiKetNoi);

            if (_tk.TryGetValue(tenDangNhap, out var t) && t.MatKhau == matKhau)
                return new KetQuaXacThuc(MaKetQua.ThanhCong, t.HoTen);

            return new KetQuaXacThuc(MaKetQua.SaiThongTin);
        }

        public async Task<KetQuaXacThuc> DangKyAsync(
            string tenDangNhap, string matKhau, string hoTen, string? email,
            CancellationToken token = default)
        {
            await Task.Delay(DO_TRE_MS, token);

            if (tenDangNhap.Equals("loimang", StringComparison.OrdinalIgnoreCase))
                return new KetQuaXacThuc(MaKetQua.LoiKetNoi);

            if (_tk.ContainsKey(tenDangNhap))
                return new KetQuaXacThuc(MaKetQua.TenDaTonTai);

            _tk[tenDangNhap] = (matKhau, hoTen);
            return new KetQuaXacThuc(MaKetQua.ThanhCong, hoTen);
        }

        public async Task<KetQuaXacThuc> QuenMatKhauAsync(
            string tenDangNhap, string email, string matKhauMoi,
            CancellationToken token = default)
        {
            await Task.Delay(DO_TRE_MS, token);

            if (tenDangNhap.Equals("loimang", StringComparison.OrdinalIgnoreCase))
                return new KetQuaXacThuc(MaKetQua.LoiKetNoi);

            if (!_tk.TryGetValue(tenDangNhap, out var t))
                return new KetQuaXacThuc(MaKetQua.SaiThongTin, ThongBao: "Tên đăng nhập hoặc email không khớp.");

            _tk[tenDangNhap] = (matKhauMoi, t.HoTen);
            return new KetQuaXacThuc(MaKetQua.ThanhCong, t.HoTen, "Đặt lại mật khẩu thành công.");
        }

        public async Task<KetQuaXacThuc> GuiOtpQuenMatKhauAsync(
            string email, CancellationToken token = default)
        {
            await Task.Delay(DO_TRE_MS, token);

            if (email.StartsWith("loimang", StringComparison.OrdinalIgnoreCase))
                return new KetQuaXacThuc(MaKetQua.LoiKetNoi, ThongBao: "Không kết nối được với Server.");

            _mockOtpFailures = 0;
            return new KetQuaXacThuc(MaKetQua.ThanhCong,
                ThongBao: "Nếu email tồn tại và đang hoạt động, mã OTP 6 số đã được gửi (mã thử: 123456).");
        }

        public async Task<KetQuaXacThuc> XacThucOtpQuenMatKhauAsync(
            string email, string otpCode, CancellationToken token = default)
        {
            await Task.Delay(DO_TRE_MS, token);

            if (email.StartsWith("loimang", StringComparison.OrdinalIgnoreCase))
                return new KetQuaXacThuc(MaKetQua.LoiKetNoi, ThongBao: "Không kết nối được với Server.");

            if (_mockOtpFailures >= 5)
                return new KetQuaXacThuc(MaKetQua.LoiKhac, ThongBao: "Bạn đã nhập sai OTP quá 5 lần. Vui lòng yêu cầu mã mới.");

            if (otpCode.Trim() != "123456")
            {
                _mockOtpFailures++;
                return new KetQuaXacThuc(MaKetQua.SaiThongTin,
                    ThongBao: $"Mã OTP không đúng. Còn {5 - _mockOtpFailures} lần thử.");
            }

            return new KetQuaXacThuc(MaKetQua.ThanhCong,
                ThongBao: "Xác thực OTP thành công.",
                ResetToken: "MOCK_RESET_TOKEN");
        }

        public async Task<KetQuaXacThuc> DatLaiMatKhauBangTokenAsync(
            string resetToken, string matKhauMoi, CancellationToken token = default)
        {
            await Task.Delay(DO_TRE_MS, token);

            if (resetToken != "MOCK_RESET_TOKEN")
                return new KetQuaXacThuc(MaKetQua.SaiThongTin, ThongBao: "Phiên đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

            if (_tk.TryGetValue("demo", out var t))
                _tk["demo"] = (matKhauMoi, t.HoTen);

            return new KetQuaXacThuc(MaKetQua.ThanhCong, ThongBao: "Đặt lại mật khẩu thành công.");
        }
    }
}