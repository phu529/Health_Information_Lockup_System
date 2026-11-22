using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using HealthShared;

namespace HealthServer.Network;

internal sealed class TcpServer
{
    private readonly IPAddress _diaChiNghe;
    private readonly int _cong;
    private readonly RequestRouter _router;
    private readonly TimeSpan _thoiGianImLang;

    private readonly ConcurrentDictionary<string, ClientSession> _phien = new();
    private static readonly SearchRequestHandler _searchHandler = new(new HealthRepository());

    public TcpServer(RequestRouter router, int? cong = null, IPAddress? diaChiNghe = null, TimeSpan? thoiGianImLang = null)
    {
        _router = router;
        _cong = cong ?? ProtocolConstants.DefaultTcpPort;
        _diaChiNghe = diaChiNghe ?? IPAddress.Any;
        _thoiGianImLang = thoiGianImLang ?? TimeSpan.FromSeconds(ProtocolConstants.ServerIdleTimeoutSeconds);
    }

    public int SoPhienDangNoi => _phien.Count;

    public void ThuHoiPhienTheoUser(int userId)
    {
        foreach (var phien in _phien.Values)
        {
            if (phien.UserId == userId)
            {
                phien.DangXuatPhien();
                phien.Dispose();
            }
        }
    }

    public int CongDangNghe { get; private set; }

    internal static string Gio() => DateTime.Now.ToString("HH:mm:ss.fff");

    public Task StartAsync(CancellationToken token)
    {
        var listener = new TcpListener(_diaChiNghe, _cong);
        listener.Start();
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
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                _ = PhucVuClientAsync(client, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        finally
        {
            listener.Stop();
            foreach (var phien in _phien.Values)
                phien.Dispose();

            Console.WriteLine($"{Gio()} [TCP] Đã dừng.");
        }
    }

    private async Task PhucVuClientAsync(TcpClient client, CancellationToken serverToken)
    {
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

                using (var hanCho = CancellationTokenSource.CreateLinkedTokenSource(serverToken))
                {
                    hanCho.CancelAfter(_thoiGianImLang);
                    dong = await phien.DocAsync(hanCho.Token);
                }

                if (dong is null) break;
                if (dong.Length == 0) continue;

                string? searchResponse = await TryHandleSearchAsync(dong);
                if (searchResponse is not null)
                {
                    await phien.GuiAsync(searchResponse, serverToken);
                    continue;
                }

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
            lyDo = "gửi thông điệp quá 64 KiB";
            await GuiLoiRoiDongAsync(phien, ErrorCode.PayloadTooLarge, "Thông điệp vượt quá 64 KiB.");
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            lyDo = "mất kết nối (rớt mạng hoặc client tắt đột ngột)";
        }
        catch (Exception ex)
        {
            lyDo = $"lỗi không mong muốn ({ex.GetType().Name})";
        }
        finally
        {
            _phien.TryRemove(phien.Id, out _);
            phien.Dispose();
            Console.WriteLine($"{Gio()} [TCP] - {phien.Id} rời đi: {lyDo} (đang nối: {_phien.Count})");
        }
    }

    private static async Task<string?> TryHandleSearchAsync(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Command", out var cmdProp))
            {
                string? cmd = cmdProp.GetString();
                if (cmd is "SEARCH" or "GET_DETAIL")
                {
                    return await _searchHandler.ProcessRequestAsync(json);
                }
            }
        }
        catch
        {
        }
        return null;
    }

    private static async Task GuiLoiRoiDongAsync(ClientSession phien, ErrorCode ma, string thongBao)
    {
        try
        {
            using var hanGui = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var loi = RequestRouter.Reply(null, ma, thongBao);
            await phien.GuiAsync(ProtocolHelper.SerializeData(loi), hanGui.Token);
        }
        catch
        {
        }
    }
}