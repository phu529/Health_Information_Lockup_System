using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using HealthShared;

namespace HealthServer.Network;

/// <summary>
/// LÕI TCP ĐA KẾT NỐI CỦA SERVER (Bước 1.3).
///
/// Cách hoạt động:
///   1. Nghe ở cổng 5000 trên MỌI card mạng (IPAddress.Any) để máy khác trong LAN nối vào được.
///   2. Mỗi client nối vào thì tạo MỘT tác vụ riêng (Task per client) và quay lại nghe tiếp ngay,
///      nên client này không phải chờ client kia (Chương 3, slide 21).
///   3. Mỗi client giữ kết nối mở, gửi nhiều yêu cầu liên tiếp; mỗi yêu cầu là một dòng JSON kết thúc LF.
///   4. Các phiên đang nối nằm trong ConcurrentDictionary (an toàn khi nhiều tác vụ cùng thêm/xóa).
///   5. Client rớt (đóng cửa sổ, rút dây, im quá lâu) thì phiên bị dọn khỏi danh sách và đóng socket.
/// </summary>
internal sealed class TcpServer
{
    private readonly IPAddress _diaChiNghe;
    private readonly int _cong;
    private readonly RequestRouter _router;
    private readonly TimeSpan _thoiGianImLang;

    // Khóa = Id phiên. Dùng kiểu an toàn đa luồng vì mỗi client chạy trên một tác vụ khác nhau.
    private readonly ConcurrentDictionary<string, ClientSession> _phien = new();

    public TcpServer(RequestRouter router, int? cong = null, IPAddress? diaChiNghe = null, TimeSpan? thoiGianImLang = null)
    {
        _router = router;
        _cong = cong ?? ProtocolConstants.DefaultTcpPort;
        _diaChiNghe = diaChiNghe ?? IPAddress.Any;
        _thoiGianImLang = thoiGianImLang ?? TimeSpan.FromSeconds(ProtocolConstants.ServerIdleTimeoutSeconds);
    }

    /// <summary>Số client đang nối.</summary>
    public int SoPhienDangNoi => _phien.Count;

    /// <summary>Cổng thật đang nghe (hữu ích khi truyền cổng 0 để hệ điều hành tự chọn, dùng trong kiểm thử).</summary>
    public int CongDangNghe { get; private set; }

    /// <summary>Giờ ngắn dùng chung cho mọi dòng log của lõi TCP.</summary>
    internal static string Gio() => DateTime.Now.ToString("HH:mm:ss.fff");

    /// <summary>
    /// Bắt đầu nghe và chạy server cho tới khi token bị hủy. Task trả về hoàn thành khi đã dừng hẳn.
    /// Hàm này KHÔNG async: nếu cổng đang bị chương trình khác dùng thì ngoại lệ ném ra NGAY tại đây
    /// (không bị giấu trong Task), nên Program thấy lỗi ngay lúc khởi động.
    /// </summary>
    public Task StartAsync(CancellationToken token)
    {
        var listener = new TcpListener(_diaChiNghe, _cong);
        listener.Start();   // ném SocketException ngay nếu cổng đã bị dùng
        CongDangNghe = ((IPEndPoint)listener.LocalEndpoint).Port;
        Console.WriteLine($"{Gio()} [TCP] Đang nghe tại {listener.LocalEndpoint}");
        return ChayVongNhanAsync(listener, token);
    }

    private async Task ChayVongNhanAsync(TcpListener listener, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // Dừng ở đây (không tốn CPU) cho tới khi có client gõ cửa.
                TcpClient client = await listener.AcceptTcpClientAsync(token);

                // KHÔNG await: phục vụ client này ở tác vụ riêng, vòng lặp quay lại nghe client tiếp theo.
                _ = PhucVuClientAsync(client, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Server được yêu cầu dừng: không phải sự cố.
        }
        finally
        {
            listener.Stop();

            // Đóng mọi kết nối còn lại để các tác vụ đang chờ đọc được đánh thức và thoát.
            foreach (var phien in _phien.Values)
                phien.Dispose();

            Console.WriteLine($"{Gio()} [TCP] Đã dừng.");
        }
    }

    /// <summary>Toàn bộ đời sống của MỘT client: nhận -> xử lý -> phản hồi, lặp lại cho tới khi rời đi.</summary>
    private async Task PhucVuClientAsync(TcpClient client, CancellationToken serverToken)
    {
        // Gói nhỏ gửi đi ngay, không chờ gom (Nagle); bật keep-alive để hệ điều hành tự dò kết nối chết.
        client.NoDelay = true;
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        var phien = new ClientSession(client);
        _phien[phien.Id] = phien;
        Console.WriteLine($"{Gio()} [TCP] + {phien.Id} nối từ {phien.DiaChiClient} (đang nối: {_phien.Count})");

        string lyDo = "client đóng kết nối";
        try
        {
            while (!serverToken.IsCancellationRequested)
            {
                string? dong;

                // Hạn chờ im lặng chỉ áp cho lúc ĐỢI client gửi, không áp cho lúc xử lý yêu cầu.
                using (var hanCho = CancellationTokenSource.CreateLinkedTokenSource(serverToken))
                {
                    hanCho.CancelAfter(_thoiGianImLang);
                    dong = await phien.DocAsync(hanCho.Token);
                }

                if (dong is null) break;                 // client đóng đàng hoàng
                if (dong.Length == 0) continue;          // dòng trống: bỏ qua

                BaseResponse phanHoi = await _router.DispatchAsync(phien, dong, serverToken);
                await phien.GuiAsync(ProtocolHelper.SerializeData(phanHoi), serverToken);
            }

            if (serverToken.IsCancellationRequested) lyDo = "server dừng";
        }
        catch (OperationCanceledException)
        {
            lyDo = serverToken.IsCancellationRequested ? "server dừng" : $"im lặng quá {_thoiGianImLang.TotalSeconds:0} giây";
        }
        catch (FrameTooLargeException)
        {
            // Dòng quá dài: không còn biết ranh giới thông điệp tiếp theo, nên báo 413 rồi đóng kết nối.
            lyDo = "gửi thông điệp quá 64 KiB";
            await GuiLoiRoiDongAsync(phien, ErrorCode.PayloadTooLarge, "Thông điệp vượt quá 64 KiB.");
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            lyDo = "mất kết nối (rớt mạng hoặc client tắt đột ngột)";
        }
        catch (Exception ex)
        {
            // Chốt chặn cuối: tác vụ chạy riêng mà ném lỗi thì không ai hay biết (Phụ lục C slide 23).
            lyDo = $"lỗi không mong muốn ({ex.GetType().Name})";
        }
        finally
        {
            // Luôn dọn dẹp dù thoát bằng cách nào.
            _phien.TryRemove(phien.Id, out _);
            phien.Dispose();
            Console.WriteLine($"{Gio()} [TCP] - {phien.Id} rời đi: {lyDo} (đang nối: {_phien.Count})");
        }
    }

    private static async Task GuiLoiRoiDongAsync(ClientSession phien, ErrorCode ma, string thongBao)
    {
        try
        {
            using var hanGui = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var loi = RequestRouter.Reply(null, ma, thongBao);
            await phien.GuiAsync(ProtocolHelper.SerializeData(loi), hanGui.Token);
        }
        catch (Exception)
        {
            // Không gửi được thì thôi, kết nối sắp đóng.
        }
    }
}
