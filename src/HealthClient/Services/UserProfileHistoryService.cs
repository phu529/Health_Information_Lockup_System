using System.Net.Sockets;
using System.Text.Json;
using HealthClient.Network;
using HealthShared;

namespace HealthClient.Services;

public sealed record KetQuaDichVu<T>(bool ThanhCong, string ThongBao, T? DuLieu = default);

public sealed class UserProfileHistoryService
{
    public Task<KetQuaDichVu<UserProfilePayload>> LayHoSoAsync(CancellationToken token = default)
        => GuiNhanAsync<UserProfilePayload>(UserCommands.GetProfile, payloadJson: null, token);

    public Task<KetQuaDichVu<UserProfilePayload>> CapNhatHoSoAsync(
        UpdateProfilePayload payload, CancellationToken token = default)
        => GuiNhanAsync<UserProfilePayload>(
            UserCommands.UpdateProfile,
            ProtocolHelper.SerializeData(payload),
            token);

    public Task<KetQuaDichVu<List<SearchHistoryItemPayload>>> LayLichSuAsync(
        int limit = 50, CancellationToken token = default)
        => GuiNhanAsync<List<SearchHistoryItemPayload>>(
            UserCommands.GetHistory,
            ProtocolHelper.SerializeData(new GetHistoryPayload(limit)),
            token);

    public async Task<KetQuaDichVu<bool>> ThemLichSuAsync(
        string tuKhoa, int? diseaseId = null, int soKetQua = 0, CancellationToken token = default)
    {
        var ketQua = await GuiNhanKhongDuLieuAsync(
            UserCommands.AddHistory,
            ProtocolHelper.SerializeData(new AddHistoryPayload(tuKhoa, diseaseId, soKetQua)),
            token);

        return new KetQuaDichVu<bool>(ketQua.ThanhCong, ketQua.ThongBao, ketQua.ThanhCong);
    }

    public Task<KetQuaDichVu<bool>> XoaTatCaLichSuAsync(CancellationToken token = default)
        => GuiNhanKhongDuLieuAsync(UserCommands.ClearHistory, payloadJson: null, token);

    private static async Task<KetQuaDichVu<T>> GuiNhanAsync<T>(
        string command, string? payloadJson, CancellationToken token)
    {
        try
        {
            BaseResponse response = await TcpClientHelper.Shared.SendAsync(command, payloadJson, token);

            if (response.Command != command)
                return new KetQuaDichVu<T>(false, "Phản hồi từ Server không hợp lệ.");

            if (response.Status != ErrorCode.Success)
            {
                string msg = string.IsNullOrWhiteSpace(response.Message)
                    ? "Server không xử lý được yêu cầu."
                    : response.Message;
                return new KetQuaDichVu<T>(false, msg);
            }

            if (string.IsNullOrWhiteSpace(response.Payload))
                return new KetQuaDichVu<T>(false, "Server trả về thiếu dữ liệu.");

            var data = ProtocolHelper.DeserializeData<T>(response.Payload);
            return data is null
                ? new KetQuaDichVu<T>(false, "Không đọc được dữ liệu phản hồi từ Server.")
                : new KetQuaDichVu<T>(true, response.Message, data);
        }
        catch (TimeoutException)
        {
            return new KetQuaDichVu<T>(false, "Hết thời gian chờ phản hồi từ Server.");
        }
        catch (Exception ex) when (ex is SocketException or IOException)
        {
            return new KetQuaDichVu<T>(false, "Không kết nối được với Server.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return new KetQuaDichVu<T>(false, "Dữ liệu gửi hoặc nhận không hợp lệ.");
        }
    }

    private static async Task<KetQuaDichVu<bool>> GuiNhanKhongDuLieuAsync(
        string command, string? payloadJson, CancellationToken token)
    {
        try
        {
            BaseResponse response = await TcpClientHelper.Shared.SendAsync(command, payloadJson, token);

            if (response.Command != command)
                return new KetQuaDichVu<bool>(false, "Phản hồi từ Server không hợp lệ.", false);

            if (response.Status != ErrorCode.Success)
            {
                string msg = string.IsNullOrWhiteSpace(response.Message)
                    ? "Server không xử lý được yêu cầu."
                    : response.Message;
                return new KetQuaDichVu<bool>(false, msg, false);
            }

            return new KetQuaDichVu<bool>(true, response.Message, true);
        }
        catch (TimeoutException)
        {
            return new KetQuaDichVu<bool>(false, "Hết thời gian chờ phản hồi từ Server.", false);
        }
        catch (Exception ex) when (ex is SocketException or IOException)
        {
            return new KetQuaDichVu<bool>(false, "Không kết nối được với Server.", false);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return new KetQuaDichVu<bool>(false, "Dữ liệu gửi hoặc nhận không hợp lệ.", false);
        }
    }
}