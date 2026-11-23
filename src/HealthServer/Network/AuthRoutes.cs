using HealthShared;

namespace HealthServer.Network;

internal static class AuthRoutes
{
    public static void Register(RequestRouter router, AuthRequestHandler handler)
    {
        router.Register(AuthCommands.Login, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.LoginAsync(ProtocolHelper.DeserializeData<LoginPayload>(request.Payload), session));

        router.Register(AuthCommands.Register, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.RegisterAsync(ProtocolHelper.DeserializeData<RegisterPayload>(request.Payload)));

        router.Register(AuthCommands.ForgotPassword, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.ForgotPasswordAsync(
                    ProtocolHelper.DeserializeData<ForgotPasswordPayload>(request.Payload)));

        router.Register(AuthCommands.RequestPasswordResetOtp, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.RequestPasswordResetOtpAsync(
                    ProtocolHelper.DeserializeData<RequestPasswordResetOtpPayload>(request.Payload)));

        router.Register(AuthCommands.VerifyPasswordResetOtp, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.VerifyPasswordResetOtpAsync(
                    ProtocolHelper.DeserializeData<VerifyPasswordResetOtpPayload>(request.Payload)));

        router.Register(AuthCommands.ResetPasswordWithToken, (session, request, token) =>
            string.IsNullOrWhiteSpace(request.Payload)
                ? Task.FromResult(Thieu(request.Command))
                : handler.ResetPasswordWithTokenAsync(
                    ProtocolHelper.DeserializeData<ResetPasswordWithTokenPayload>(request.Payload)));
    }

    private static BaseResponse Thieu(string command) =>
        RequestRouter.Reply(command, ErrorCode.BadRequest, "Yêu cầu xác thực không hợp lệ.");
}
