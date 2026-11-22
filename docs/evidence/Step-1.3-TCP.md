# Minh chứng Bước 1.3 — Core TCP

Ngày chạy: 08/11/2026

## Luồng kiểm tra

Hai client TCP gửi bản tin JSON `PING` (mỗi bản tin kết thúc bằng LF) tới Server ở cổng 5000. Server đọc đủ một dòng, trả `PONG` kèm đúng `RequestId` và `Payload`; client ghép phản hồi theo `RequestId`.

```text
Client X ── JSON + LF ──▶ Server :5000 ── JSON PONG + LF ──▶ Client X
Client Y ── JSON + LF ──▶ Server :5000 ── JSON PONG + LF ──▶ Client Y
```

## Kết quả

- Test #9, dính gói: 3 bản tin trong một lần ghi, nhận đủ phản hồi A, B, C đúng thứ tự — PASS.
- Test #8, phân mảnh: một bản tin UTF-8 gửi từng mảnh 3 byte, ghép lại đúng nội dung — PASS.
- Hai client đồng thời: X nhận đúng 50/50 phản hồi, Y nhận đúng 50/50; log Server ghi nhận số kết nối tăng lên 2 — PASS.
- Tải đồng thời: 20 client × 20 yêu cầu, nhận đúng 400/400 — PASS.
- Toàn bộ TcpProbe: 9/9 ca đạt.

## Cách chạy lại

Mở hai cửa sổ PowerShell tại thư mục repo:

```powershell
dotnet run --no-build --project src\HealthServer\HealthServer.csproj
dotnet run --no-build --project tools\TcpProbe\TcpProbe.csproj
```

`TcpProbe` chỉ dùng lệnh PING nên không cần cơ sở dữ liệu. Log đầy đủ của lần chạy này nằm ở `server-run.log` và `probe-run.log` trong thư mục này.
