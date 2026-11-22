using System.Net;
using System.Text.Json;
using HealthShared;
using Microsoft.Data.SqlClient;

namespace HealthServer.Network;

internal static class UserProfileHistoryRoutes
{
    private static readonly HashSet<string> ValidGenders = new(StringComparer.Ordinal)
    {
        "Nam", "Nu", "Khac"
    };

    public static void Register(RequestRouter router, UserRepository repository)
    {
        router.Register(UserCommands.GetProfile, (s, r, t) => GetProfileAsync(s, r, repository));
        router.Register(UserCommands.Profile, (s, r, t) => GetProfileAsync(s, r, repository));
        router.Register(UserCommands.UpdateProfile, (s, r, t) => UpdateProfileAsync(s, r, repository));
        router.Register(UserCommands.GetHistory, (s, r, t) => GetHistoryAsync(s, r, repository));
        router.Register(UserCommands.AddHistory, (s, r, t) => AddHistoryAsync(s, r, repository));
        router.Register(UserCommands.ClearHistory, (s, r, t) => ClearHistoryAsync(s, r, repository));
    }

    private static async Task<BaseResponse> GetProfileAsync(
        ClientSession session, BaseRequest request, UserRepository repository)
    {
        if (session.UserId is not int userId)
            return ChuaDangNhap(request.Command);

        var profile = await repository.GetFullProfileAsync(userId);
        if (profile is null)
            return RequestRouter.Reply(request.Command, ErrorCode.NotFound, "Không tìm thấy hồ sơ người dùng.");

        return RequestRouter.Reply(
            request.Command,
            ErrorCode.Success,
            "Lấy thông tin hồ sơ thành công.",
            JsonSerializer.Serialize(profile));
    }

    private static async Task<BaseResponse> UpdateProfileAsync(
        ClientSession session, BaseRequest request, UserRepository repository)
    {
        if (session.UserId is not int userId)
            return ChuaDangNhap(request.Command);

        if (string.IsNullOrWhiteSpace(request.Payload))
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Thiếu dữ liệu cập nhật hồ sơ.");

        var data = ProtocolHelper.DeserializeData<UpdateProfilePayload>(request.Payload);
        if (data is null)
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Dữ liệu hồ sơ không hợp lệ.");

        string fullName = data.HoTen?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Họ tên phải từ 1 đến 100 ký tự.");

        string? email = string.IsNullOrWhiteSpace(data.Email) ? null : data.Email.Trim();
        if (email is not null && email.Length > 254)
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Email tối đa 254 ký tự.");

        string? gender = string.IsNullOrWhiteSpace(data.GioiTinh) ? null : data.GioiTinh.Trim();
        if (gender is not null && !ValidGenders.Contains(gender))
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Giới tính chỉ nhận: Nam, Nu, Khac.");

        if (data.ChieuCaoCm is <= 0 or > 300)
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Chiều cao phải lớn hơn 0 và tối đa 300 cm.");

        if (data.CanNangKg is <= 0 or > 700)
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Cân nặng phải lớn hơn 0 và tối đa 700 kg.");

        try
        {
            bool updated = await repository.UpsertFullProfileAsync(
                userId,
                fullName,
                email,
                data.NgaySinh,
                gender,
                data.ChieuCaoCm,
                data.CanNangKg,
                TrimOrNull(data.DiUng, 2000),
                TrimOrNull(data.BenhNen, 2000),
                TrimOrNull(data.GhiChu, 2000));

            if (!updated)
                return RequestRouter.Reply(request.Command, ErrorCode.NotFound, "Không tìm thấy tài khoản để cập nhật.");

            var latestProfile = await repository.GetFullProfileAsync(userId);
            return RequestRouter.Reply(
                request.Command,
                ErrorCode.Success,
                "Cập nhật hồ sơ thành công.",
                latestProfile is null ? string.Empty : JsonSerializer.Serialize(latestProfile));
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return RequestRouter.Reply(request.Command, ErrorCode.Conflict, "Email này đã được sử dụng bởi tài khoản khác.");
        }
    }

    private static async Task<BaseResponse> GetHistoryAsync(
        ClientSession session, BaseRequest request, UserRepository repository)
    {
        if (session.UserId is not int userId)
            return ChuaDangNhap(request.Command);

        int limit = 50;
        if (!string.IsNullOrWhiteSpace(request.Payload))
        {
            var query = ProtocolHelper.DeserializeData<GetHistoryPayload>(request.Payload);
            if (query?.Limit is int requestedLimit)
                limit = requestedLimit;
        }

        var history = await repository.GetSearchHistoryAsync(userId, limit);
        return RequestRouter.Reply(
            request.Command,
            ErrorCode.Success,
            "Lấy lịch sử tra cứu thành công.",
            JsonSerializer.Serialize(history));
    }

    private static async Task<BaseResponse> AddHistoryAsync(
        ClientSession session, BaseRequest request, UserRepository repository)
    {
        if (session.UserId is not int userId)
            return ChuaDangNhap(request.Command);

        if (string.IsNullOrWhiteSpace(request.Payload))
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Thiếu dữ liệu lịch sử tra cứu.");

        var item = ProtocolHelper.DeserializeData<AddHistoryPayload>(request.Payload);
        string keyword = item?.TuKhoa?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(keyword))
            return RequestRouter.Reply(request.Command, ErrorCode.BadRequest, "Từ khóa tra cứu không được để trống.");

        if (keyword.Length > 500)
            keyword = keyword[..500];

        string? clientIp = (session.DiaChiClient as IPEndPoint)?.Address.ToString();
        long newId = await repository.AddSearchHistoryAsync(
            userId, keyword, item?.DiseaseId, item?.SoKetQua ?? 0, clientIp);

        return RequestRouter.Reply(
            request.Command,
            ErrorCode.Success,
            "Đã lưu lịch sử tra cứu.",
            JsonSerializer.Serialize(new { SearchHistoryId = newId }));
    }

    private static async Task<BaseResponse> ClearHistoryAsync(
        ClientSession session, BaseRequest request, UserRepository repository)
    {
        if (session.UserId is not int userId)
            return ChuaDangNhap(request.Command);

        int deletedCount = await repository.ClearSearchHistoryAsync(userId);
        return RequestRouter.Reply(
            request.Command,
            ErrorCode.Success,
            $"Đã xóa {deletedCount} mục lịch sử tra cứu.");
    }

    private static BaseResponse ChuaDangNhap(string command) =>
        RequestRouter.Reply(command, ErrorCode.Unauthorized, "Bạn cần đăng nhập để thực hiện chức năng này.");

    private static string? TrimOrNull(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}