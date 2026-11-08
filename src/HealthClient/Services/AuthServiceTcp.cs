using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using HealthShared;

namespace HealthClient.Services;

public sealed class AuthServiceTcp : IAuthService
{
    public Task<KetQuaXacThuc> DangNhapAsync(
        string tenDangNhap, string matKhau, CancellationToken token = default)
    {
        return SendAsync(AuthCommands.Login, new LoginPayload(tenDangNhap, matKhau), token);
    }

    public Task<KetQuaXacThuc> DangKyAsync(
        string tenDangNhap, string matKhau, string hoTen, string? email,
        CancellationToken token = default)
    {
        return SendAsync(
            AuthCommands.Register,
            new RegisterPayload(tenDangNhap, matKhau, hoTen, email),
            token);
    }

    private static async Task<KetQuaXacThuc> SendAsync<TPayload>(
        string command, TPayload payload, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            var request = new BaseRequest
            {
                Command = command,
                Payload = ProtocolHelper.SerializeData(payload)
            };

            using var client = new TcpClient();
            await client.ConnectAsync(
                IPAddress.Loopback, ProtocolConstants.DefaultTcpPort, timeout.Token);

            using var stream = client.GetStream();
            await AuthTcpFraming.WriteAsync(
                stream, ProtocolHelper.SerializeData(request), timeout.Token);

            var json = await AuthTcpFraming.ReadAsync(stream, timeout.Token);
            var response = ProtocolHelper.DeserializeData<BaseResponse>(json);

            if (response is null || response.Command != command)
            {
                return new KetQuaXacThuc(MaKetQua.LoiKhac,
                    ThongBao: "Phản hồi từ Server không hợp lệ.");
            }

            if (command == AuthCommands.Login &&
                (response.Status == ErrorCode.Unauthorized ||
                 response.Status == ErrorCode.BadRequest))
            {
                return new KetQuaXacThuc(MaKetQua.SaiThongTin,
                    ThongBao: "Đăng nhập bị từ chối.");
            }

            if (response.Status != ErrorCode.Success)
            {
                var message = string.IsNullOrWhiteSpace(response.Message)
                    ? "Server không xử lý được yêu cầu."
                    : response.Message;

                return new KetQuaXacThuc(MaKetQua.LoiKhac, ThongBao: message);
            }

            if (string.IsNullOrWhiteSpace(response.Payload))
            {
                return new KetQuaXacThuc(MaKetQua.LoiKhac,
                    ThongBao: "Phản hồi thiếu thông tin tài khoản.");
            }

            var result = ProtocolHelper.DeserializeData<AuthSuccessPayload>(response.Payload);
            return result is null
                ? new KetQuaXacThuc(MaKetQua.LoiKhac,
                    ThongBao: "Phản hồi thiếu thông tin tài khoản.")
                : new KetQuaXacThuc(MaKetQua.ThanhCong, HoTen: result.HoTen);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            return new KetQuaXacThuc(MaKetQua.LoiKetNoi,
                ThongBao: "Hết thời gian chờ Server.");
        }
        catch (Exception ex) when (
            ex is JsonException or InvalidDataException or InvalidOperationException)
        {
            return new KetQuaXacThuc(MaKetQua.LoiKhac,
                ThongBao: "Yêu cầu hoặc phản hồi không hợp lệ.");
        }
        catch (Exception ex) when (ex is SocketException or IOException)
        {
            return new KetQuaXacThuc(MaKetQua.LoiKetNoi,
                ThongBao: "Không kết nối được với Server.");
        }
    }
}