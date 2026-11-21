using System.Net.Sockets;
using System.Text.Json;
using HealthClient.Network;
using HealthShared;

namespace HealthClient.Services;

/// <summary>
/// Đăng nhập / đăng ký qua Server thật (thay AuthServiceTam, Bước 1.3).
/// Form chỉ biết IAuthService; lớp này đổi kết quả của Server thành MaKetQua cho giao diện.
/// Việc gửi/nhận thật nằm ở TcpClientHelper.Shared (một kết nối giữ mở, đóng khung LF).
/// KHÔNG chứa logic băm mật khẩu hay kiểm tra luật (việc của Server / Trọng Phú).
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

            // Sai tên hoặc sai mật khẩu dùng chung một thông báo (không lộ tài khoản có tồn tại hay không).
            if (command == AuthCommands.Login &&
                (response.Status == ErrorCode.Unauthorized ||
                 response.Status == ErrorCode.BadRequest))
            {
                return new KetQuaXacThuc(MaKetQua.SaiThongTin,
                    ThongBao: "Đăng nhập bị từ chối.");
            }

            //sai ten dang nhap hoac email khong khop khi dat lai mat khau
            if(command == AuthCommands.ForgotPassword &&
                (response.Status == ErrorCode.Unauthorized ||
                response.Status == ErrorCode.BadRequest ||
                response.Status == ErrorCode.NotFound))
            {
                return new KetQuaXacThuc(MaKetQua.SaiThongTin,
                    ThongBao: string.IsNullOrWhiteSpace(response.Message)
                        ? "Tên đăng nhập hoặc email không khớp."
                        : response.Message);
            }

            if (response.Status != ErrorCode.Success)
            {
                var message = string.IsNullOrWhiteSpace(response.Message)
                    ? "Server không xử lý được yêu cầu."
                    : response.Message;

                return new KetQuaXacThuc(MaKetQua.LoiKhac, ThongBao: message);
            }

            //lenh quen mat khau chi can status == success, khong can tra ve AuthSuccessPayload
            if(command == AuthCommands.ForgotPassword)
            {
                var okMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Đặt lại mật khẩu thành công."
                    : response.Message;

                return new KetQuaXacThuc(MaKetQua.ThanhCong, ThongBao: okMessage);
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
            // Lỗi MẠNG, không phải sai mật khẩu: Form báo "không kết nối được", không tính vào lần đăng nhập sai.
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
