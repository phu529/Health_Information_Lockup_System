# Hệ thống tra cứu triệu chứng và thông tin sức khỏe (NT106)

Đồ án môn Lập trình mạng căn bản. Client C# WinForms, Server C# TCP (cổng 5000), CSDL SQL Server.
Thông tin chỉ mang tính tham khảo, không thay thế chẩn đoán của bác sĩ.

Nhánh `main` chỉ chứa sản phẩm cuối. Mọi công việc nằm trên nhánh `feature/*`.
Nhánh `feature/core-tcp` = Bước 1.3 (lõi TCP đa kết nối, đóng khung LF).

## 1. Cấu trúc thư mục

| Đường dẫn | Chức năng |
|---|---|
| `src/HealthShared/` | Thư viện dùng chung của Client và Server: bản tin, mã lỗi, hằng số, đóng khung LF |
| `src/HealthServer/` | Server dòng lệnh: lõi TCP, định tuyến lệnh, xác thực, truy cập SQL Server, nghe UDP dò Server |
| `src/HealthClient/` | Ứng dụng WinForms: form đăng nhập/đăng ký, kết nối TCP, dịch vụ xác thực, thành phần giao diện |
| `database/` | Script SQL tạo CSDL `HealthInfoDB` |
| `docs/` | Đặc tả giao thức, đề tài, minh chứng kiểm thử |
| `tools/TcpProbe/` | Công cụ kiểm thử lõi TCP, chạy riêng, không nằm trong `.sln` |

## 2. src/HealthShared

| File | Chức năng |
|---|---|
| `BaseRequest.cs`, `BaseResponse.cs` | Khung bản tin: `Command`, `RequestId`, `Payload` (yêu cầu); thêm `Status`, `Message` (phản hồi) |
| `ErrorCode.cs` | Mã trạng thái: 200, 400, 401, 404, 409, 413, 500 |
| `ProtocolConstants.cs` | Cổng 5000, trần bản tin 64 KiB, Server cắt sau 90 giây im lặng, Client chờ phản hồi 10 giây, nối 5 giây, chu kỳ ping 30 giây, lệnh `PING` |
| `ProtocolHelper.cs` | Chuyển đối tượng sang JSON và ngược lại |
| `LfFraming.cs` | Ghi và đọc bản tin một dòng kết thúc LF; xử lý phân mảnh, dính gói, chặn dòng quá 64 KiB |
| `AuthProtocol.cs` | Tên lệnh `LOGIN`, `REGISTER` và dữ liệu kèm theo |

## 3. src/HealthServer

| File | Chức năng |
|---|---|
| `Program.cs` | Điểm khởi động: dựng bộ định tuyến, chạy TCP cổng 5000, nghe UDP cổng 8888. Có các tham số thử CSDL: `--test-db`, `--test-user`, `--test-register`, `--test-password`, `--test-profile`, `--test-update-profile` |
| `Network/TcpServer.cs` | Nhận kết nối bất đồng bộ, mỗi client một tác vụ, lưu danh sách phiên bằng `ConcurrentDictionary`, cắt phiên im lặng quá 90 giây |
| `Network/ClientSession.cs` | Một kết nối client đang mở: socket, bộ đọc LF, khóa ghi |
| `Network/RequestRouter.cs` | Đọc `Command`, gọi đúng hàm xử lý; có sẵn lệnh `PING`; lệnh lạ trả 404, JSON sai trả 400, hàm xử lý ném lỗi trả 500. Thêm lệnh mới bằng `router.Register("TÊN_LỆNH", hàm)` |
| `Network/AuthRoutes.cs` | Cầu nối đưa `LOGIN` và `REGISTER` vào bộ định tuyến, không chứa logic mật khẩu |
| `AuthRequestHandler.cs` | Xử lý nghiệp vụ đăng nhập và đăng ký |
| `UserRepository.cs` | Truy vấn người dùng: tìm tài khoản, tạo tài khoản, ghi kết quả đăng nhập, đọc và cập nhật hồ sơ |
| `DatabaseHelper.cs` | Tạo kết nối SQL Server từ `appsettings.json` |

## 4. src/HealthClient

| Đường dẫn | Chức năng |
|---|---|
| `Program.cs` | Điểm khởi động: gắn `AuthServiceTcp`, mở `FrmDangNhap`, đóng kết nối TCP khi thoát |
| `FrmDangNhap`, `FrmDangKy` | Form đăng nhập và đăng ký (kiểm tra dữ liệu tại chỗ, gọi `IAuthService`) |
| `FrmTrangChu` | Màn hình tạm sau đăng nhập; giao diện tra cứu đầy đủ làm ở Bước 1.4 |
| `Network/TcpClientHelper.cs` | Kết nối TCP dùng chung, gửi lệnh và ghép phản hồi theo `RequestId`, tự nối lại khi mất kết nối, có hạn chờ |
| `Services/IAuthService.cs` | Cổng giữa giao diện và tầng mạng; form chỉ biết interface này |
| `Services/AuthServiceTcp.cs` | Gửi `LOGIN`, `REGISTER` qua Server thật và đổi kết quả thành `MaKetQua` cho form |
| `Services/AuthServiceTam.cs` | Bản giả dùng khi chưa có Server; `Program.cs` không còn dùng |
| `Services/CauHinhCucBo.cs` | Lưu tên đăng nhập và mật khẩu "nhớ" trên máy, mật khẩu mã hóa bằng Windows DPAPI |
| `DiscoveryService.cs` | Gửi UDP broadcast dò IP Server (cổng 8888) |
| `HeartbeatClient.cs` | Vòng PING/PONG và tự nối lại; hàm `SendPingAsync` hiện luôn trả `true`, chưa gọi Server thật |
| `Form1` | Form thử UDP dò Server và Heartbeat; không phải form khởi động, chưa nối vào luồng đăng nhập |
| `UI/` | Thành phần giao diện dùng chung: `Theme`, nút, ô nhập, thanh tab, cảnh báo y khoa `CanhBaoYTe`, thanh độ mạnh mật khẩu, hiệu ứng chuyển cảnh và chuyển trang, bộ đo CPU/RAM (Ctrl+Shift+F9). Chỉ là giao diện, không chứa logic mạng |

## 5. Giao thức TCP (tóm tắt, đầy đủ ở `docs/protocol.md`)

- Cổng 5000. Một kết nối giữ mở cho nhiều yêu cầu.
- Mỗi bản tin là một dòng JSON UTF-8 kết thúc LF, tối đa 64 KiB.
- Client đặt `RequestId`, Server trả lại nguyên vẹn để ghép phản hồi.
- Lệnh hiện có: `PING`, `LOGIN`, `REGISTER`.
- Chưa có mã hóa TLS; mật khẩu đi dạng chữ thường trong `LOGIN` và `REGISTER` cho tới khi làm Bước 4.4.

## 6. database và docs

| Đường dẫn | Chức năng |
|---|---|
| `database/HealthInfoDB.sql` | Tạo CSDL `HealthInfoDB` gồm 11 bảng (Users, Categories, Symptoms, Diseases, DiseaseSymptoms, Medicines, DiseaseMedicines, SearchHistory, HealthProfiles, AuditLogs, Sessions) và thủ tục `usp_RecordLoginResult`, `usp_BackupHealthInfoDB` |
| `docs/protocol.md` | Đặc tả giao thức: cấu trúc bản tin, mã trạng thái, danh sách lệnh, vòng đời kết nối, cách thêm lệnh mới |
| `docs/evidence/` | Minh chứng Bước 1.3: `README.md` (trạng thái và cách chạy lại), `server-run.log`, `probe-run.log` |
| `docs/Ghi-chu-bo-sung-mat-khau-Step-1.2.md` | Ghi chú bổ sung của Bước 1.2 |

## 7. tools/TcpProbe

Chạy 9 kịch bản vào Server đang chạy bằng lệnh `PING`, không cần SQL Server:
PING cơ bản, dính gói, phân mảnh, dòng trống và CRLF, hai client đồng thời, 20 client, lệnh lạ và JSON hỏng, dòng quá 64 KiB, rớt đột ngột.
Tham số `-- --giu 2` mở 2 kết nối và giữ mở để chụp log Server.

## 8. Cách chạy

1. Chạy `database/HealthInfoDB.sql` trong SQL Server Management Studio.
2. Tạo `src/HealthServer/appsettings.json` (file bị `.gitignore` chặn, không đưa lên Git) với khóa `ConnectionStrings:HealthInfoDb` trỏ tới SQL Server của máy.
   Thiếu file này thì Server không khởi động được.
3. Mở `Health_Information_Lockup_System.sln` bằng Visual Studio (.NET 10), Build Solution phải 0 lỗi.
4. Chạy Server: `dotnet run --project src\HealthServer`. Log phải có `Đang nghe tại 0.0.0.0:5000`.
5. Chạy Client: Start `HealthClient`; mở thêm cửa sổ bằng Debug > Start New Instance.
6. Kiểm tra lõi TCP không cần giao diện: với Server đang chạy, chạy `dotnet run --project tools\TcpProbe`. Kết quả đúng: `Tổng: 9/9 đạt`.

Đăng nhập và đăng ký qua giao diện cần SQL Server đang chạy và `appsettings.json` hợp lệ.
