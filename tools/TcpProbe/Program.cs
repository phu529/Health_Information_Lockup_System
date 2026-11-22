using System.Net.Sockets;
using System.Text;
using HealthShared;

// =====================================================================
// TCP PROBE - công cụ thử lõi TCP của Server (Bước 1.3)
//
// Cách chạy (Server phải đang chạy):
//   dotnet run --project tools\TcpProbe                 chạy các kịch bản thử, in PASS/FAIL
//   dotnet run --project tools\TcpProbe -- 127.0.0.1 5000
//   dotnet run --project tools\TcpProbe -- --giu 2      mở 2 kết nối và GIỮ MỞ (chụp ảnh log Server "đang nối: 2")
//
// Các kịch bản dùng lệnh PING (không cần cơ sở dữ liệu). PING trả lại đúng Payload đã gửi.
// =====================================================================

string host = args.FirstOrDefault(a => !a.StartsWith("--") && !int.TryParse(a, out _)) ?? "127.0.0.1";
int port = args.Select(a => int.TryParse(a, out int p) ? p : 0).FirstOrDefault(p => p > 1024);
if (port == 0) port = ProtocolConstants.DefaultTcpPort;

Console.OutputEncoding = Encoding.UTF8;

if (args.Contains("--giu"))
{
    int soKetNoi = int.TryParse(args.SkipWhile(a => a != "--giu").Skip(1).FirstOrDefault(), out int n) ? n : 2;
    await GiuKetNoiAsync(host, port, soKetNoi);
    return 0;
}

Console.WriteLine($"=== TCP PROBE -> {host}:{port} ===\n");
var ketQua = new List<(string Ten, bool Dat, string GhiChu)>();

await Chay("T1  PING cơ bản (nhận PONG, đúng RequestId)", T1_Ping);
await Chay("T2  #9 Dính gói: 3 yêu cầu trong MỘT lần ghi -> đủ 3 phản hồi, đúng thứ tự", T2_DinhGoi);
await Chay("T3  #8 Phân mảnh: 1 yêu cầu gửi từng 3 byte (cắt giữa chữ có dấu) -> ghép đúng", T3_PhanManh);
await Chay("T4  Dòng trống + kết thúc CRLF được chấp nhận", T4_DongTrongVaCrlf);
await Chay("T5  #2/#22 Hai client gửi đồng thời -> mỗi client nhận đúng phản hồi của mình", T5_HaiClient);
await Chay("T6  #21 20 client cùng lúc, mỗi client 20 yêu cầu -> không rớt, không lẫn", T6_HaiMuoiClient);
await Chay("T7  Lệnh lạ -> 404; JSON hỏng -> 400; kết nối VẪN dùng tiếp được", T7_LoiNhe);
await Chay("T8  Dòng > 64 KiB -> Server báo 413 rồi đóng kết nối", T8_QuaDai);
await Chay("T9  #14 Rớt đột ngột (RST) rồi nối lại -> Server vẫn nhận client mới", T9_RotDotNgot);

Console.WriteLine("\n--------------------------------------------------");
foreach (var (ten, dat, ghiChu) in ketQua)
    Console.WriteLine($"{(dat ? "PASS" : "FAIL")}  {ten}{(ghiChu.Length > 0 ? "  -> " + ghiChu : "")}");
int loi = ketQua.Count(k => !k.Dat);
Console.WriteLine($"\nTổng: {ketQua.Count - loi}/{ketQua.Count} đạt.");
Console.WriteLine("Với T9: mở cửa sổ Server, dòng '- ... rời đi: mất kết nối' phải xuất hiện (Server tự dọn phiên).");
return loi == 0 ? 0 : 1;

// ----------------------------------------------------------------------------
async Task Chay(string ten, Func<Task<string>> kichBan)
{
    try
    {
        string ghiChu = await kichBan();
        ketQua.Add((ten, true, ghiChu));
    }
    catch (Exception ex)
    {
        ketQua.Add((ten, false, ex.Message));
    }
}

// ---- Dụng cụ: một kết nối thô, tự đóng khung bằng tay để cố ý gửi "sai cách" ----
async Task<(TcpClient C, NetworkStream S, LfFrameReader R)> Noi()
{
    var c = new TcpClient { NoDelay = true };
    await c.ConnectAsync(host, port);
    var s = c.GetStream();
    return (c, s, new LfFrameReader(s));
}

static string TaoPing(string id, string payload) =>
    ProtocolHelper.SerializeData(new BaseRequest { Command = CoreCommands.Ping, RequestId = id, Payload = payload });

async Task<BaseResponse> DocPhanHoi(LfFrameReader r)
{
    using var han = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    string? dong = await r.ReadLineAsync(han.Token);
    if (dong is null) throw new Exception("Server đóng kết nối khi đang chờ phản hồi");
    return ProtocolHelper.DeserializeData<BaseResponse>(dong)!;
}

static void Kiem(bool dung, string loi) { if (!dung) throw new Exception(loi); }

// ---- Các kịch bản ----
async Task<string> T1_Ping()
{
    var (c, s, r) = await Noi();
    using (c)
    {
        await LfFraming.WriteAsync(s, TaoPing("abc", "xin chao"));
        var kq = await DocPhanHoi(r);
        Kiem(kq.Status == ErrorCode.Success && kq.Message == "PONG", $"Status={kq.Status} Message={kq.Message}");
        Kiem(kq.RequestId == "abc" && kq.Payload == "xin chao", "RequestId/Payload không khớp");
        return "PONG";
    }
}

async Task<string> T2_DinhGoi()
{
    var (c, s, r) = await Noi();
    using (c)
    {
        // Ghép 3 dòng thành MỘT mảng byte và ghi một lần: bên nhận có thể đọc cả 3 trong một lần.
        byte[] goi = Encoding.UTF8.GetBytes(TaoPing("A", "1") + "\n" + TaoPing("B", "2") + "\n" + TaoPing("C", "3") + "\n");
        await s.WriteAsync(goi);
        foreach (var (id, nd) in new[] { ("A", "1"), ("B", "2"), ("C", "3") })
        {
            var kq = await DocPhanHoi(r);
            Kiem(kq.RequestId == id && kq.Payload == nd, $"Mong {id}/{nd}, nhận {kq.RequestId}/{kq.Payload}");
        }
        return "A, B, C đúng thứ tự";
    }
}

async Task<string> T3_PhanManh()
{
    const string noiDung = "Đặng Thị Hạnh bị ho, sốt 38,5°C và đau họng ✓";
    var (c, s, r) = await Noi();
    using (c)
    {
        // Dùng JSON có chữ gốc (không escape) để một ký tự tiếng Việt chiếm 2-3 byte, rồi cắt từng 3 byte.
        string json = "{\"Command\":\"PING\",\"RequestId\":\"frag\",\"Payload\":\"" + noiDung + "\"}";
        byte[] byteJson = Encoding.UTF8.GetBytes(json + "\n");
        for (int i = 0; i < byteJson.Length; i += 3)
        {
            await s.WriteAsync(byteJson.AsMemory(i, Math.Min(3, byteJson.Length - i)));
            await s.FlushAsync();
            await Task.Delay(8);   // để các mảnh tới Server thành nhiều lần đọc riêng
        }
        var kq = await DocPhanHoi(r);
        Kiem(kq.RequestId == "frag", $"RequestId={kq.RequestId}");
        Kiem(kq.Payload == noiDung, $"Payload sai: '{kq.Payload}'");
        return $"{byteJson.Length} byte chia {byteJson.Length / 3 + 1} mảnh";
    }
}

async Task<string> T4_DongTrongVaCrlf()
{
    var (c, s, r) = await Noi();
    using (c)
    {
        await s.WriteAsync(Encoding.UTF8.GetBytes("\n\n" + TaoPing("crlf", "ok") + "\r\n"));
        var kq = await DocPhanHoi(r);
        Kiem(kq.RequestId == "crlf" && kq.Payload == "ok", "không nhận đúng phản hồi");
        return "bỏ qua dòng trống, nhận CRLF";
    }
}

async Task<string> T5_HaiClient()
{
    async Task<int> MotClient(string ten)
    {
        var (c, s, r) = await Noi();
        using (c)
        {
            int dung = 0;
            for (int i = 0; i < 50; i++)
            {
                string id = $"{ten}-{i}";
                await LfFraming.WriteAsync(s, TaoPing(id, "du lieu cua " + ten));
                var kq = await DocPhanHoi(r);
                if (kq.RequestId == id && kq.Payload == "du lieu cua " + ten) dung++;
            }
            return dung;
        }
    }
    var ketQuaHai = await Task.WhenAll(MotClient("X"), MotClient("Y"));
    Kiem(ketQuaHai.All(d => d == 50), $"X đúng {ketQuaHai[0]}/50, Y đúng {ketQuaHai[1]}/50");
    return "X 50/50, Y 50/50";
}

async Task<string> T6_HaiMuoiClient()
{
    async Task<int> MotClient(int so)
    {
        var (c, s, r) = await Noi();
        using (c)
        {
            int dung = 0;
            for (int i = 0; i < 20; i++)
            {
                string id = $"c{so}-{i}";
                await LfFraming.WriteAsync(s, TaoPing(id, id));
                var kq = await DocPhanHoi(r);
                if (kq.RequestId == id && kq.Payload == id) dung++;
            }
            return dung;
        }
    }
    var tatCa = await Task.WhenAll(Enumerable.Range(0, 20).Select(MotClient));
    Kiem(tatCa.All(d => d == 20), "có client nhận thiếu hoặc sai: " + string.Join(",", tatCa));
    return "20 client x 20 yêu cầu = 400/400";
}

async Task<string> T7_LoiNhe()
{
    var (c, s, r) = await Noi();
    using (c)
    {
        await LfFraming.WriteAsync(s, ProtocolHelper.SerializeData(new BaseRequest { Command = "KHONG_CO_LENH_NAY", RequestId = "x1", Payload = "" }));
        var a = await DocPhanHoi(r);
        Kiem(a.Status == ErrorCode.NotFound && a.RequestId == "x1", $"Lệnh lạ: Status={a.Status}");

        await s.WriteAsync(Encoding.UTF8.GetBytes("day khong phai json\n"));
        var b = await DocPhanHoi(r);
        Kiem(b.Status == ErrorCode.BadRequest, $"JSON hỏng: Status={b.Status}");

        await LfFraming.WriteAsync(s, TaoPing("sau-loi", "con song"));
        var d = await DocPhanHoi(r);
        Kiem(d.RequestId == "sau-loi" && d.Message == "PONG", "kết nối không dùng tiếp được sau lỗi");
        return "404, 400, rồi PING vẫn chạy";
    }
}

async Task<string> T8_QuaDai()
{
    var (c, s, r) = await Noi();
    using (c)
    {
        // 70.000 ký tự 'a' không có LF: vượt 64 KiB.
        try { await s.WriteAsync(Encoding.UTF8.GetBytes(new string('a', 70_000))); } catch (IOException) { }
        var kq = await DocPhanHoi(r);
        Kiem(kq.Status == ErrorCode.PayloadTooLarge, $"Mong 413, nhận {kq.Status}");
        string? tiep = null;
        try { using var han = new CancellationTokenSource(2000); tiep = await r.ReadLineAsync(han.Token); } catch (IOException) { }
        Kiem(tiep is null, "Server chưa đóng kết nối sau 413");
        return "413 rồi đóng";
    }
}

async Task<string> T9_RotDotNgot()
{
    var (c1, s1, r1) = await Noi();
    await LfFraming.WriteAsync(s1, TaoPing("truoc", "x"));
    await DocPhanHoi(r1);
    c1.LingerState = new LingerOption(true, 0);   // đóng kiểu RST: giống rút dây/tắt tiến trình
    c1.Dispose();
    await Task.Delay(300);

    var (c2, s2, r2) = await Noi();
    using (c2)
    {
        await LfFraming.WriteAsync(s2, TaoPing("sau", "y"));
        var kq = await DocPhanHoi(r2);
        Kiem(kq.RequestId == "sau", "client mới không được phục vụ");
        return "client mới vẫn được phục vụ";
    }
}

async Task GiuKetNoiAsync(string h, int p, int so)
{
    var dsKetNoi = new List<TcpClient>();
    for (int i = 0; i < so; i++)
    {
        var c = new TcpClient { NoDelay = true };
        await c.ConnectAsync(h, p);
        dsKetNoi.Add(c);
        Console.WriteLine($"Đã mở kết nối #{i + 1} từ {c.Client.LocalEndPoint}");
    }
    Console.WriteLine($"\nĐang giữ {so} kết nối tới {h}:{p}. Chụp ảnh log Server (phải ghi 'đang nối: {so}').");
    Console.WriteLine("Nhấn Enter để đóng các kết nối...");
    Console.ReadLine();
    foreach (var c in dsKetNoi) c.Dispose();
}
