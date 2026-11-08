namespace HealthClient.Services
{
    /// <summary>
    /// TAM THOI, chi de thu giao dien khi chua co server (xoa o Buoc 1.3).
    /// Tai khoan co san: demo / demo1234.
    /// Ten "loimang" luon tra ve LoiKetNoi de thu trang thai co loi mang.
    /// </summary>
    public sealed class AuthServiceTam : IAuthService
    {
        private const int DO_TRE_MS = 700; // gia lap cho mang, de thay trang thai "Dang gui"

        private readonly Dictionary<string, (string MatKhau, string HoTen)> _tk =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["demo"] = ("demo1234", "Người dùng thử")
            };

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
    }
}
