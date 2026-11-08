using System.Net;
using System.Net.Sockets;
using HealthShared;

namespace HealthServer.Network;

/// <summary>
/// MỘT KẾT NỐI CLIENT đang mở tới Server (Bước 1.3).
/// Giữ socket, bộ đọc LF và khóa ghi. TcpServer tạo một đối tượng cho mỗi client
/// và lưu vào ConcurrentDictionary để biết ai đang nối.
/// </summary>
internal sealed class ClientSession : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly LfFrameReader _reader;

    // Nhiều tác vụ có thể cùng gửi cho một client (hiện chỉ có phản hồi, sau này có thể có tin đẩy).
    // await bên trong nên dùng SemaphoreSlim, không dùng lock (Phụ lục C slide 16-17).
    private readonly SemaphoreSlim _khoaGhi = new(1, 1);

    /// <summary>Mã ngắn của phiên, dùng làm khóa trong danh sách và ghi log.</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8];

    public EndPoint? DiaChiClient { get; }
    public DateTime ThoiDiemNoiUtc { get; } = DateTime.UtcNow;

    public ClientSession(TcpClient client)
    {
        _client = client;
        DiaChiClient = client.Client.RemoteEndPoint;
        _stream = client.GetStream();
        _reader = new LfFrameReader(_stream);
    }

    /// <summary>Đọc một thông điệp. null = client đã đóng kết nối đàng hoàng.</summary>
    public Task<string?> DocAsync(CancellationToken token) => _reader.ReadLineAsync(token);

    /// <summary>Gửi một thông điệp JSON (tự thêm LF). An toàn khi nhiều tác vụ cùng gọi.</summary>
    public async Task GuiAsync(string json, CancellationToken token)
    {
        await _khoaGhi.WaitAsync(token);
        try
        {
            await LfFraming.WriteAsync(_stream, json, token);
        }
        finally
        {
            _khoaGhi.Release();   // luôn trả "vé", kể cả khi ghi lỗi
        }
    }

    public void Dispose()
    {
        _client.Dispose();   // đóng socket, trả lại cho hệ điều hành
        _khoaGhi.Dispose();
    }
}
