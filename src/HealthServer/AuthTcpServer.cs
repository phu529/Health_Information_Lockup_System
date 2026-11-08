using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using HealthShared;

namespace HealthServer;

internal sealed class AuthTcpServer
{
    private readonly AuthRequestHandler _handler;

    public AuthTcpServer(AuthRequestHandler handler)
    {
        _handler = handler;
    }

    public Task StartAsync(CancellationToken token)
    {
        var listener = new TcpListener(IPAddress.Loopback, ProtocolConstants.DefaultTcpPort);
        listener.Start();

        Console.WriteLine($"[TCP] Đang nghe tại {listener.LocalEndpoint}");
        return AcceptClientsAsync(listener, token);
    }

    private async Task AcceptClientsAsync(TcpListener listener, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(token);
                _ = HandleClientAsync(client, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Server đang dừng.
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken serverToken)
    {
        using (client)
        using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(serverToken))
        {
            timeout.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                using var stream = client.GetStream();
                var requestJson = await AuthTcpFraming.ReadAsync(stream, timeout.Token);
                var response = await HandleRequestAsync(requestJson);

                await AuthTcpFraming.WriteAsync(
                    stream, ProtocolHelper.SerializeData(response), timeout.Token);
            }
            catch (OperationCanceledException)
            {
                // Hết thời gian chờ hoặc Server đang dừng.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] Lỗi xử lý kết nối: {ex.GetType().Name}");
            }
        }
    }

    private async Task<BaseResponse> HandleRequestAsync(string requestJson)
{
    string command = string.Empty;

    try
    {
        var request = ProtocolHelper.DeserializeData<BaseRequest>(requestJson);
        command = request?.Command ?? string.Empty;

        if (request is not null && !string.IsNullOrWhiteSpace(request.Payload))
        {
            switch (command)
            {
                case AuthCommands.Login:
                    var login = ProtocolHelper.DeserializeData<LoginPayload>(request.Payload);
                    return await _handler.LoginAsync(login);

                case AuthCommands.Register:
                    var register = ProtocolHelper.DeserializeData<RegisterPayload>(request.Payload);
                    return await _handler.RegisterAsync(register);
            }
        }
    }
    catch (JsonException)
    {
        // Trả BadRequest khi JSON không hợp lệ.
    }

    return new BaseResponse
    {
        Command = command,
        Status = ErrorCode.BadRequest,
        Message = "Yêu cầu xác thực không hợp lệ.",
        Payload = string.Empty
    };
}
}