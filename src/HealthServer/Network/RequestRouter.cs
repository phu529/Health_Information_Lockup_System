using System.Collections.Concurrent;
using System.Text.Json;
using HealthShared;

namespace HealthServer.Network;

/// <summary>
/// BỘ ĐỊNH TUYẾN LỆNH (Bước 1.3): nhận một dòng JSON, đọc trường Command, gọi đúng hàm xử lý.
/// Thành viên khác muốn thêm chức năng chỉ cần gọi Register("TÊN_LỆNH", hàm) - không phải sửa TcpServer.
/// Ví dụ có sẵn: PING (ở đây), LOGIN / REGISTER (xem AuthRoutes.cs).
/// </summary>
internal sealed class RequestRouter
{
    /// <summary>Hàm xử lý một lệnh: nhận phiên + yêu cầu, trả về phản hồi.</summary>
    public delegate Task<BaseResponse> Handler(
        ClientSession session, BaseRequest request, CancellationToken token);

    private readonly ConcurrentDictionary<string, Handler> _handlers =
        new(StringComparer.OrdinalIgnoreCase);

    public RequestRouter()
    {
        // PING là lệnh có sẵn của lõi: Client dùng để kiểm tra Server còn sống.
        Register(CoreCommands.Ping, (s, r, t) => Task.FromResult(
            Reply(r.Command, ErrorCode.Success, "PONG", r.Payload ?? string.Empty)));
    }

    public void Register(string command, Handler handler) => _handlers[command] = handler;

    /// <summary>
    /// Xử lý MỘT dòng JSON và LUÔN trả về một phản hồi (không ném ngoại lệ ra ngoài),
    /// để một yêu cầu hỏng không làm sập kết nối của client.
    /// </summary>
    public async Task<BaseResponse> DispatchAsync(
        ClientSession session, string requestJson, CancellationToken token)
    {
        BaseRequest? request;
        try
        {
            request = ProtocolHelper.DeserializeData<BaseRequest>(requestJson);
        }
        catch (JsonException)
        {
            return Reply(null, ErrorCode.BadRequest, "Thông điệp không phải JSON hợp lệ.");
        }

        if (request is null || string.IsNullOrWhiteSpace(request.Command))
            return Reply(request?.Command, ErrorCode.BadRequest, "Thiếu trường Command.", requestId: request?.RequestId);

        if (!_handlers.TryGetValue(request.Command, out var handler))
            return Reply(request.Command, ErrorCode.NotFound, "Lệnh không được hỗ trợ.", requestId: request.RequestId);

        BaseResponse response;
        try
        {
            response = await handler(session, request, token);
        }
        catch (OperationCanceledException)
        {
            throw;   // Server đang dừng hoặc hết giờ chờ: để TcpServer xử lý
        }
        catch (JsonException)
        {
            response = Reply(request.Command, ErrorCode.BadRequest, "Payload không hợp lệ.");
        }
        catch (Exception ex)
        {
            // Chỉ ghi TÊN loại lỗi, không ghi nội dung (có thể chứa mật khẩu, chuỗi kết nối).
            Console.WriteLine($"{TcpServer.Gio()} [ROUTER] Lệnh {request.Command} bị lỗi: {ex.GetType().Name}");
            response = Reply(request.Command, ErrorCode.InternalServerError, "Server gặp lỗi khi xử lý yêu cầu.");
        }

        // Luôn trả lại đúng RequestId và Command của yêu cầu, dù hàm xử lý có đặt hay không.
        response.RequestId = request.RequestId;
        response.Command ??= request.Command;
        return response;
    }

    /// <summary>Tạo nhanh một phản hồi (dùng cho cả hàm xử lý của thành viên khác).</summary>
    public static BaseResponse Reply(
        string? command, ErrorCode status, string message,
        string payload = "", string? requestId = null) => new()
    {
        Command = command ?? string.Empty,
        Status = status,
        Message = message,
        Payload = payload,
        RequestId = requestId
    };
}
