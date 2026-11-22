namespace HealthShared;

public static class UserCommands
{
    public const string GetProfile = "GET_PROFILE";
    public const string Profile = "PROFILE";
    public const string UpdateProfile = "UPDATE_PROFILE";
    public const string GetHistory = "GET_HISTORY";
    public const string AddHistory = "ADD_HISTORY";
    public const string ClearHistory = "CLEAR_HISTORY";
}

public sealed record UserProfilePayload(
    int UserId,
    string TenDangNhap,
    string HoTen,
    string? Email,
    DateTime? NgaySinh,
    string? GioiTinh,
    decimal? ChieuCaoCm,
    decimal? CanNangKg,
    string? DiUng,
    string? BenhNen,
    string? GhiChu,
    DateTime? NgayCapNhat);

public sealed record UpdateProfilePayload(
    string HoTen,
    string? Email,
    DateTime? NgaySinh,
    string? GioiTinh,
    decimal? ChieuCaoCm,
    decimal? CanNangKg,
    string? DiUng,
    string? BenhNen,
    string? GhiChu);

public sealed record SearchHistoryItemPayload(
    long SearchHistoryId,
    string TuKhoa,
    int? DiseaseId,
    string? TenBenh,
    int SoKetQua,
    DateTime ThoiGianTim);

public sealed record GetHistoryPayload(int? Limit = 50);

public sealed record AddHistoryPayload(
    string TuKhoa,
    int? DiseaseId = null,
    int SoKetQua = 0);