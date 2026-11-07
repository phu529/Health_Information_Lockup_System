namespace HealthClient.Services
{
    public enum MaKetQua
    {
        ThanhCong,
        SaiThongTin,   // sai ten dang nhap hoac mat khau (cung mot thong bao)
        TenDaTonTai,   // dang ky trung ten
        LoiKetNoi,     // khong noi duoc server / mat mang
        LoiKhac
    }

    public sealed record KetQuaXacThuc(
        MaKetQua Ma,
        string? HoTen = null,
        string? ThongBao = null);

    /// <summary>
    /// Cua ngo giua giao dien va tang mang. Form chi biet interface nay.
    /// Hien dung AuthServiceTam; sau Buoc 1.3 / 4.3 se thay bang ban gui JSON qua TCP.
    /// </summary>
    public interface IAuthService
    {
        Task<KetQuaXacThuc> DangNhapAsync(
            string tenDangNhap, string matKhau,
            CancellationToken token = default);

        Task<KetQuaXacThuc> DangKyAsync(
            string tenDangNhap, string matKhau, string hoTen, string? email,
            CancellationToken token = default);
    }

    public static class AppServices
    {
        public static IAuthService Auth { get; set; } = new AuthServiceTam();
    }
}
