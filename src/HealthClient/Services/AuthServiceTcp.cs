using System.Net.Sockets;
using System.Text.Json;
using HealthClient.Network;
using HealthShared;

namespace HealthClient.Services;

/// <summary>
/// Đăng nhập / đăng ký / quên mật khẩu qua Server thật qua TcpClientHelper.Shared.
/// </summary>
public sealed class AuthServiceTcp : IAuthService
{
    public Task<KetQuaXacThuc> DangNhapAsync(
        string tenDangNhap, string matKhau, CancellationToken token = default)
    {
        return GuiAsync(AuthCommands.Login, new LoginPayload(tenDangNhap, matKhau), token);
    }

    public Task<KetQuaXacThuc> DangKyAsync(
        string tenDangNhap, string matKhau, string hoTen, string? email,
        CancellationToken token = default)
    {
        return GuiAsync(
            AuthCommands.Register,
            new RegisterPayload(tenDangNhap, matKhau, hoTen, email),
            token);
    }

    public Task<KetQuaXacThuc> QuenMatKhauAsync(
        string tenDangNhap, string email, string matKhauMoi,
        CancellationToken token = default)
    {
        return GuiAsync(
            AuthCommands.ForgotPassword,
            new ForgotPasswordPayload(tenDangNhap, email, matKhauMoi),
            token);
    }

    public Task<KetQuaXacThuc> GuiOtpQuenMatKhauAsync(
        string email, CancellationToken token = default)
    {
        return GuiAsync(
            AuthCommands.RequestPasswordResetOtp,
            new RequestPasswordResetOtpPayload(email),
            token);
    }

    public Task<KetQuaXacThuc> XacThucOtpQuenMatKhauAsync(
        string email, string otpCode, CancellationToken token = default)
    {
        return GuiAsync(
            AuthCommands.VerifyPasswordResetOtp,
            new VerifyPasswordResetOtpPayload(email, otpCode),
            token);
    }

    public Task<KetQuaXacThuc> DatLaiMatKhauBangTokenAsync(
        string resetToken, string matKhauMoi, CancellationToken token = default)
    {
        return GuiAsync(
            AuthCommands.ResetPasswordWithToken,
            new ResetPasswordWithTokenPayload(resetToken, matKhauMoi),
            token);
    }

    private static async Task<KetQuaXacThuc> GuiAsync<TPayload>(
        string command, TPayload payload, CancellationToken token)
    {
        try
        {
            BaseResponse response = await TcpClientHelper.Shared.SendAsync(
                command, ProtocolHelper.SerializeData(payload), token);

            if (response.Command != command)
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

            if ((command == AuthCommands.ForgotPassword ||
                 command == AuthCommands.VerifyPasswordResetOtp ||
                 command == AuthCommands.ResetPasswordWithToken) &&
                (response.Status == ErrorCode.Unauthorized ||
                 response.Status == ErrorCode.BadRequest ||
                 response.Status == ErrorCode.NotFound))
            {
                return new KetQuaXacThuc(MaKetQua.SaiThongTin,
                    ThongBao: string.IsNullOrWhiteSpace(response.Message)
                        ? "Thông tin xác thực không hợp lệ."
                        : response.Message);
            }

            if (response.Status != ErrorCode.Success)
            {
                var message = string.IsNullOrWhiteSpace(response.Message)
                    ? "Server không xử lý được yêu cầu."
                    : response.Message;

                return new KetQuaXacThuc(MaKetQua.LoiKhac, ThongBao: message);
            }

            if (command == AuthCommands.ForgotPassword ||
                command == AuthCommands.RequestPasswordResetOtp ||
                command == AuthCommands.ResetPasswordWithToken)
            {
                var okMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Thao tác thành công."
                    : response.Message;

                return new KetQuaXacThuc(MaKetQua.ThanhCong, ThongBao: okMessage);
            }

            if (command == AuthCommands.VerifyPasswordResetOtp)
            {
                if (string.IsNullOrWhiteSpace(response.Payload))
                {
                    return new KetQuaXacThuc(MaKetQua.LoiKhac,
                        ThongBao: "Phản hồi thiếu ResetToken.");
                }

                var otpResult = ProtocolHelper.DeserializeData<VerifyPasswordResetOtpResponsePayload>(response.Payload);
                return otpResult is null || string.IsNullOrWhiteSpace(otpResult.ResetToken)
                    ? new KetQuaXacThuc(MaKetQua.LoiKhac, ThongBao: "Không đọc được ResetToken.")
                    : new KetQuaXacThuc(MaKetQua.ThanhCong, ThongBao: response.Message, ResetToken: otpResult.ResetToken);
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
        catch (TimeoutException)
        {
            return new KetQuaXacThuc(MaKetQua.LoiKetNoi,
                ThongBao: "Hết thời gian chờ Server.");
        }
        catch (Exception ex) when (ex is SocketException or IOException)
        {
            return new KetQuaXacThuc(MaKetQua.LoiKetNoi,
                ThongBao: "Không kết nối được với Server.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return new KetQuaXacThuc(MaKetQua.LoiKhac,
                ThongBao: "Yêu cầu hoặc phản hồi không hợp lệ.");
        }
    }
}