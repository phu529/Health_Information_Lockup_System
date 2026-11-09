using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using HealthShared;

namespace HealthClient.Network;

/// <summary>
/// KẾT NỐI TCP CỦA CLIENT TỚI SERVER (Bước 1.3). Toàn bộ là async nên giao diện không bị treo.
///
/// Cách dùng (từ form hoặc service):
///     BaseResponse kq = await TcpClientHelper.Shared.SendAsync("LOGIN", jsonPayload);
///
/// Cách hoạt động:
///   - Giữ MỘT kết nối mở dùng chung cho cả ứng dụng; chưa nối thì tự nối ở lần gửi đầu,
///     bị rớt thì lần gửi sau tự nối lại.
///   - Mỗi yêu cầu được gắn một RequestId. Một vòng lặp nền đọc phản hồi (mỗi dòng LF một phản hồi)
///     rồi ghép đúng phản hồi với đúng yêu cầu theo RequestId, nên nhiều yêu cầu gửi cùng lúc không lẫn nhau.
///   - Mọi lỗi mạng (không nối được, rớt giữa chừng, hết giờ chờ) được ném ra thành ngoại lệ
///     SocketException / IOException / TimeoutException để nơi gọi báo cho người dùng.
/// </summary>
public sealed class TcpClientHelper : IDisposable
{
    /// <summary>Đối tượng dùng chung cho cả ứng dụng (một kết nối duy nhất tới Server).</summary>
    public static TcpClientHelper Shared { get; } = new();

    /// <summary>
    /// Địa chỉ Server. Mặc định 127.0.0.1 (Client và Server cùng máy).
    /// Chạy 2 máy: đặt biến môi trường HEALTH_SERVER_HOST=IP-máy-server trước khi mở Client.
    /// (Bước 1.5 sẽ thay bằng UDP Discovery tự dò IP.)
    /// </summary>
    public string Host { get; set; } =
        Environment.GetEnvironmentVariable("HEALTH_SERVER_HOST") is { Length: > 0 } h ? h : "127.0.0.1";

    public int Port { get; set; } = ProtocolConstants.DefaultTcpPort;

    /// <summary>
    /// Báo khi kết nối bị mất ngoài ý muốn (Server tắt, rớt mạng). Tham số = lý do.
    /// CHÚ Ý: sự kiện được gọi từ luồng nền. Form muốn cập nhật giao diện phải dùng BeginInvoke.
    /// </summary>
    public event Action<string>? ConnectionLost;

    // Một kết nối = socket + luồng + bộ đọc LF + token dừng vòng đọc + bảng yêu cầu đang chờ phản hồi.
    // Bảng chờ nằm TRONG kết nối để kết nối cũ chết không làm hỏng yêu cầu của kết nối mới.
    private sealed record KetNoi(
        TcpClient Client, NetworkStream Luong, LfFrameReader DocLf, CancellationTokenSource DungDoc,
        ConcurrentDictionary<string, TaskCompletionSource<BaseResponse>> DangCho);

    private KetNoi? _ketNoi;
    private readonly SemaphoreSlim _khoaNoi = new(1, 1);   // chỉ một tác vụ được nối tại một thời điểm
    private readonly SemaphoreSlim _khoaGhi = new(1, 1);   // chỉ một tác vụ ghi vào luồng tại một thời điểm
    private bool _daHuy;

    public bool DangNoi => _ketNoi is { Client.Connected: true };

    /// <summary>
    /// Gửi một yêu cầu, chờ phản hồi tương ứng.
    /// payload là chuỗi JSON của đối tượng nghiệp vụ (xem ProtocolHelper.SerializeData), có thể null.
    /// </summary>
    /// <exception cref="TimeoutException">Quá hạn chờ phản hồi.</exception>
    /// <exception cref="IOException">Mất kết nối khi đang gửi/chờ.</exception>
    /// <exception cref="SocketException">Không nối được tới Server.</exception>
    public async Task<BaseResponse> SendAsync(
        string command, string? payload = null,
        CancellationToken token = default, TimeSpan? hanCho = null)
    {
        ObjectDisposedException.ThrowIf(_daHuy, this);

        KetNoi ketNoi = await DamBaoDaNoiAsync(token);

        var yeuCau = new BaseRequest
        {
            Command = command,
            RequestId = Guid.NewGuid().ToString("N")[..12],
            Payload = payload ?? string.Empty
        };

        var cho = new TaskCompletionSource<BaseResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        ketNoi.DangCho[yeuCau.RequestId] = cho;

        try
        {
            // Ghi qua khóa: hai yêu cầu gửi cùng lúc không được trộn byte vào nhau.
            await _khoaGhi.WaitAsync(token);
            try
            {
                await LfFraming.WriteAsync(ketNoi.Luong, ProtocolHelper.SerializeData(yeuCau), token);
            }
            catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
            {
                DongKetNoi(ketNoi, "ghi dữ liệu thất bại");
                throw new IOException("Mất kết nối tới Server.", ex);
            }
            finally
            {
                _khoaGhi.Release();
            }

            // Chờ vòng đọc nền trả phản hồi có cùng RequestId, tối đa "hanCho".
            using var gioiHan = CancellationTokenSource.CreateLinkedTokenSource(token);
            gioiHan.CancelAfter(hanCho ?? TimeSpan.FromSeconds(ProtocolConstants.ClientRequestTimeoutSeconds));
            using var dangKy = gioiHan.Token.Register(() => cho.TrySetCanceled(gioiHan.Token));

            try
            {
                return await cho.Task;
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                throw new TimeoutException("Hết thời gian chờ phản hồi từ Server.");
            }
        }
        finally
        {
            ketNoi.DangCho.TryRemove(yeuCau.RequestId, out _);
        }
    }

    /// <summary>Gửi PING, true nếu Server trả PONG kịp hạn. Dùng cho Heartbeat/kiểm tra kết nối.</summary>
    public async Task<bool> PingAsync(TimeSpan? hanCho = null, CancellationToken token = default)
    {
        try
        {
            var kq = await SendAsync(CoreCommands.Ping, null, token, hanCho ?? TimeSpan.FromSeconds(3));
            return kq.Status == ErrorCode.Success;
        }
        catch (Exception ex) when (ex is IOException or SocketException or TimeoutException)
        {
            return false;
        }
    }

    /// <summary>Đóng kết nối chủ động (gọi khi thoát ứng dụng). Có thể nối lại bằng SendAsync.</summary>
    public void NgatKetNoi()
    {
        if (_ketNoi is { } k) DongKetNoi(k, "ngắt chủ động", baoMat: false);
    }

    public void Dispose()
    {
        _daHuy = true;
        NgatKetNoi();
    }

    // ------------------------------------------------------------------
    // Bên dưới là phần nội bộ
    // ------------------------------------------------------------------

    /// <summary>Có kết nối còn sống thì dùng lại, không thì nối mới.</summary>
    private async Task<KetNoi> DamBaoDaNoiAsync(CancellationToken token)
    {
        if (_ketNoi is { Client.Connected: true } dangCo) return dangCo;

        await _khoaNoi.WaitAsync(token);
        try
        {
            // Kiểm lại: có thể tác vụ khác vừa nối xong trong lúc mình chờ khóa.
            if (_ketNoi is { Client.Connected: true } vuaNoi) return vuaNoi;

            var client = new TcpClient { NoDelay = true };
            try
            {
                using var hanNoi = CancellationTokenSource.CreateLinkedTokenSource(token);
                hanNoi.CancelAfter(TimeSpan.FromSeconds(ProtocolConstants.ClientConnectTimeoutSeconds));
                await client.ConnectAsync(Host, Port, hanNoi.Token);
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                client.Dispose();
                throw new TimeoutException("Hết thời gian chờ nối tới Server.");
            }
            catch
            {
                client.Dispose();
                throw;   // SocketException: Server chưa chạy, sai địa chỉ, tường lửa chặn...
            }

            var luong = client.GetStream();
            var ketNoi = new KetNoi(client, luong, new LfFrameReader(luong), new CancellationTokenSource(),
                new ConcurrentDictionary<string, TaskCompletionSource<BaseResponse>>());
            _ketNoi = ketNoi;

            // Vòng đọc nền: chạy suốt đời kết nối, không chặn giao diện, không cần Invoke.
            _ = VongDocAsync(ketNoi);
            return ketNoi;
        }
        finally
        {
            _khoaNoi.Release();
        }
    }

    /// <summary>Đọc phản hồi liên tục; mỗi dòng LF là một phản hồi, ghép theo RequestId.</summary>
    private async Task VongDocAsync(KetNoi ketNoi)
    {
        string lyDo = "Server đóng kết nối";
        try
        {
            while (true)
            {
                string? dong = await ketNoi.DocLf.ReadLineAsync(ketNoi.DungDoc.Token).ConfigureAwait(false);
                if (dong is null) break;            // Server đóng đàng hoàng
                if (dong.Length == 0) continue;

                BaseResponse? phanHoi;
                try { phanHoi = ProtocolHelper.DeserializeData<BaseResponse>(dong); }
                catch (JsonException) { continue; } // dòng hỏng: bỏ qua, chờ dòng sau

                if (phanHoi?.RequestId is { } id && ketNoi.DangCho.TryRemove(id, out var cho))
                    cho.TrySetResult(phanHoi);
                // Phản hồi không có RequestId (lỗi định dạng phía Server) không ghép được: yêu cầu liên quan sẽ hết giờ.
            }
        }
        catch (OperationCanceledException) { lyDo = "ngắt chủ động"; }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or FrameTooLargeException)
        {
            lyDo = "mất kết nối tới Server";
        }
        finally
        {
            DongKetNoi(ketNoi, lyDo, baoMat: lyDo != "ngắt chủ động");
        }
    }

    /// <summary>Đóng một kết nối, báo lỗi cho mọi yêu cầu đang chờ, và (nếu cần) báo cho giao diện.</summary>
    private void DongKetNoi(KetNoi ketNoi, string lyDo, bool baoMat = true)
    {
        // Chỉ tác vụ đầu tiên gỡ được kết nối này mới làm phần dọn dẹp (tránh dọn hai lần).
        if (Interlocked.CompareExchange(ref _ketNoi, null, ketNoi) != ketNoi) return;

        ketNoi.DungDoc.Cancel();
        ketNoi.Client.Dispose();
        ketNoi.DungDoc.Dispose();

        // Đánh thức mọi yêu cầu đang chờ phản hồi của kết nối đã chết.
        foreach (var (id, cho) in ketNoi.DangCho)
            if (ketNoi.DangCho.TryRemove(id, out _))
                cho.TrySetException(new IOException("Mất kết nối tới Server."));

        if (baoMat) ConnectionLost?.Invoke(lyDo);
    }
}
