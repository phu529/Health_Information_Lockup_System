using HealthShared;

namespace HealthServer.Network;

/// <summary>
/// Nối hai lệnh LOGIN và REGISTER (do AuthRequestHandler xử lý, việc của Trọng Phú ở Bước 4.3)
/// vào bộ định tuyến. File này chỉ làm phần "cầu nối": đọc Payload thành đối tượng rồi gọi hàm có sẵn,
/// KHÔNG chứa logic mật khẩu hay truy vấn CSDL.
/// </summary>
internal static class AuthRoutes
{
    public static void Register(RequestRouter router, AuthRequestHandler handler)
    {
        router.Register(AuthCommands.Login, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.LoginAsync(ProtocolHelper.DeserializeData<LoginPayload>(request.Payload)));

        router.Register(AuthCommands.Register, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.RegisterAsync(ProtocolHelper.DeserializeData<RegisterPayload>(request.Payload)));
    }

    private static BaseResponse Thieu(string command) =>
        RequestRouter.Reply(command, ErrorCode.BadRequest, "Yêu cầu xác thực không hợp lệ.");
}
