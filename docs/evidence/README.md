# Bước 1.3 — Core TCP (Server/Client nói chuyện bằng JSON)

**Trạng thái:** đã đủ tiêu chí kỹ thuật, đang chờ nhóm review PR #3.
Đặc tả bản tin: [`docs/protocol.md`](../protocol.md)

## Làm được gì
- Server mở cổng **5000**, mỗi client một tác vụ riêng, nhiều client cùng lúc không đụng nhau.
- Mỗi yêu cầu/phản hồi là **1 dòng JSON UTF-8 kết thúc bằng LF** → không bị phân mảnh, không bị dính gói.
- Client dùng `TcpClientHelper` để gửi/nhận. Đăng nhập/Đăng ký của Bước 1.2 đã đi qua đây.

## Kết quả kiểm tra
| Yêu cầu trong bảng phân công | Kết quả | Minh chứng |
|---|---|---|
| Có `docs/protocol.md` | Đạt | file `docs/protocol.md` |
| Build Solution 0 lỗi | Đạt | Visual Studio: Build succeeded |
| Test #8 (phân mảnh), #9 (dính gói) | Đạt | TcpProbe 9/9 PASS (T3 = #8, T2 = #9) |
| 2 Client kết nối cùng lúc, GUI không treo | Đạt | ảnh 2 cửa sổ + log Server "đang nối: 2" |
| PR để nhóm review | Đã mở | PR #3: `feature/core-tcp` → `feature/gui-auth` |

## Cách chạy thử
1. Cần .NET 10 SDK và file `src/HealthServer/appsettings.json` (xin bạn phụ trách DB; **không commit** file này).
2. Cửa sổ 1 (chạy Server trước): `dotnet run --project src\HealthServer\HealthServer.csproj`
3. Cửa sổ 2 (chạy thử): `dotnet run --project tools\TcpProbe\TcpProbe.csproj` → cuối cùng phải thấy `Tổng: 9/9 đạt`.
4. Thử GUI: trong Visual Studio đặt HealthServer + HealthClient cùng **Start**, rồi **Debug > Start New Instance** để mở thêm một Client.

## Lưu ý khi dùng chung
- **Chỉ mở 1 Server.** Báo lỗi `10048` nghĩa là cổng 5000 đã có Server khác đang chạy, đừng bật thêm.
- Server tự ngắt client **im lặng quá 90 giây**.
- Test T9 chỉ cần PASS; câu log khi ngắt kết nối có thể khác chữ, không sao.

## Các thành viên khác cần làm gì
| Ai | Việc |
|---|---|
| Trọng Phú (xác thực) | Giữ nguyên tên lệnh `LOGIN`/`REGISTER` và cấu trúc payload; lỗi mạng đừng báo thành "sai mật khẩu". |
| Hoàng Anh (tìm kiếm/UDP) | Thêm lệnh mới bằng `router.Register(...)` trong `RequestRouter`; **không mở listener thứ hai ở cổng 5000**. |
| Bạn phụ trách DB | Gửi script/schema, SQL Server và tài khoản thử qua nhóm; không đưa mật khẩu lên Git. |

## File trong thư mục này
`probe-run.log`, `server-run.log` (log chạy thử), `healthclient-two-windows.png` (ảnh 2 Client), `tcpprobe-9-9.png` (ảnh 9/9).
