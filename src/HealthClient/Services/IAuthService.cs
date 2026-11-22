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
        string? ThongBao = null,
        string? ResetToken = null);

    /// <summary>
    /// Cua ngo giua giao dien va tang mang. Form chi biet interface nay.
    /// </summary>
    public interface IAuthService
    {
        Task<KetQuaXacThuc> DangNhapAsync(
            string tenDangNhap, string matKhau,
            CancellationToken token = default);

        Task<KetQuaXacThuc> DangKyAsync(
            string tenDangNhap, string matKhau, string hoTen, string? email,
            CancellationToken token = default);

        Task<KetQuaXacThuc> QuenMatKhauAsync(
            string tenDangNhap, string email, string matKhauMoi,
            CancellationToken token = default);

        Task<KetQuaXacThuc> GuiOtpQuenMatKhauAsync(
            string email,
            CancellationToken token = default);

        Task<KetQuaXacThuc> XacThucOtpQuenMatKhauAsync(
            string email, string otpCode,
            CancellationToken token = default);

        Task<KetQuaXacThuc> DatLaiMatKhauBangTokenAsync(
            string resetToken, string matKhauMoi,
            CancellationToken token = default);
    }

    public static class AppServices
    {
        public static IAuthService Auth { get; set; } = new AuthServiceTam();
    }
}