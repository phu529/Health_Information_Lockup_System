# Bước 1.3 — Core TCP: hướng dẫn chạy và bàn giao

Tài liệu này ghi trạng thái kiểm chứng, cách chạy lại và các điểm tích hợp của lõi TCP. Đặc tả bản tin đầy đủ nằm tại [`docs/protocol.md`](../protocol.md).

Lần kiểm tra gần nhất: **09/11/2026**. Visual Studio 2026 báo `Build succeeded` với 0 dự án lỗi và 3 dự án đã up-to-date. Khi khởi chạy thêm HealthServer từ Visual Studio, tiến trình mới dừng vì cổng TCP 5000 đã được một `HealthServer.exe` khác trong cùng thư mục dự án sử dụng (SocketException 10048); tiến trình đang nghe không bị dừng. Ảnh kết quả TcpProbe 9/9 từ lần chạy trước do người dùng cung cấp. Lần chạy mới từ môi trường kiểm tra này cho kết quả **0/9**, cả 9 kịch bản đều timeout khi chờ kết nối/phản hồi trên `127.0.0.1:5000`, mặc dù Windows báo có `HealthServer.exe` của dự án đang nghe cổng này. Cần chạy probe và server trong cùng phiên Windows để xác minh lại; chưa sửa source.

## Trạng thái theo bảng phân công

| Tiêu chí | Trạng thái hiện tại | Minh chứng / phần còn lại |
|---|---|---|
| JSON UTF-8 một dòng, kết thúc LF; tài liệu đặc tả | Đạt | [`docs/protocol.md`](../protocol.md), `src/HealthShared/LfFraming.cs` |
| Server bất đồng bộ, một tác vụ cho mỗi client, quản lý phiên đồng thời | Đạt kiểm tra chức năng | `AcceptTcpClientAsync`, `ConcurrentDictionary`, log Server |
| TcpClientHelper phía Client | Đạt kiểm tra chức năng | `src/HealthClient/Network/TcpClientHelper.cs` |
| Test #8 phân mảnh và #9 dính gói | Đạt theo minh chứng lần chạy trước; cần lặp lại | Ảnh TcpProbe người dùng cung cấp thể hiện T3 (phân mảnh) và T2 (dính gói) PASS; lần chạy mới ghi 0/9 do timeout kết nối/phản hồi |
| Build toàn bộ Solution trên Windows | Đạt trong Visual Studio | `Build succeeded`: 0 dự án mới build, 0 thất bại, 3 up-to-date. Các project HealthClient và HealthServer cũng build riêng bằng .NET CLI với 0 lỗi |
| Hai client TCP cùng kết nối và gửi/nhận | Đạt bằng TcpProbe theo minh chứng trước; cần xác minh lại trên máy | T5 trong ảnh TcpProbe: X 50/50, Y 50/50. Ảnh hai cửa sổ HealthClient hiện có chưa thể hiện hai yêu cầu JSON gửi/nhận thành công |
| Hai cửa sổ HealthClient thật cùng gửi yêu cầu, giao diện không treo | **Chưa xác minh** | Cần chạy trên máy có cấu hình SQL và tài khoản thử; ảnh hiện tại là probe TCP, chưa phải hai cửa sổ GUI |
| Pull Request để nhóm review | Đã mở PR nháp #3 | PR `feature/core-tcp` → `feature/gui-auth`; GitHub báo chưa mergeable tự động, cần nhóm review khác biệt. Chưa merge |

PR đã mở để review, nhưng còn thiếu kiểm chứng hai cửa sổ HealthClient gửi/nhận thật và cần chạy lại probe trong cùng phiên Windows với Server. Vì vậy chưa đánh dấu toàn bộ Bước 1.3 là 100%.

## Mã nguồn trong nhánh và các phần tích hợp

Commit `6675fac` trên `feature/core-tcp` bổ sung/thay đổi các nhóm file sau:

- **Giao thức dùng chung:** `src/HealthShared/LfFraming.cs`, `BaseRequest.cs`, `BaseResponse.cs`, `ProtocolConstants.cs`. Mỗi yêu cầu/phản hồi là JSON một dòng UTF-8, kết thúc LF; `RequestId` được trả lại để ghép phản hồi.
- **Server:** `src/HealthServer/Network/` có TCP listener, phiên client, bộ định tuyến và route xác thực; `Program.cs` khởi chạy lõi TCP cổng 5000. Các lớp TCP cũ được thay bằng phần triển khai mới.
- **Client:** `src/HealthClient/Network/TcpClientHelper.cs` quản lý kết nối và phản hồi; `Services/AuthServiceTcp.cs` chuyển LOGIN/REGISTER qua helper; `Program.cs` đóng kết nối khi thoát.
- **Kiểm thử:** `tools/TcpProbe/` thử phân mảnh, dính gói, nhiều client, lỗi bản tin và ngắt kết nối. Công cụ dùng PING, không truy vấn CSDL.

### Điểm cần phối hợp

| Khu vực phụ trách | Liên kết với lõi TCP | Việc cần giữ khi tích hợp |
|---|---|---|
| Xác thực (Trọng Phú) | LOGIN/REGISTER đi từ `AuthServiceTcp` qua `TcpClientHelper`, Server chuyển tiếp qua `AuthRoutes` tới `AuthRequestHandler` | Giữ nguyên tên lệnh và cấu trúc payload; không chuyển lỗi mạng thành lỗi sai mật khẩu. CSDL và chính sách xác thực vẫn thuộc module xác thực. |
| Tìm kiếm/UDP Discovery (Hoàng Anh) | Lệnh nghiệp vụ mới có thể đăng ký ở `RequestRouter`; UDP Discovery vẫn chạy độc lập ở cổng 8888 | Đăng ký handler mới qua router, không tạo listener TCP thứ hai trên cổng 5000. Client cần lấy host từ Discovery ở bước tích hợp tương ứng. |
| Cơ sở dữ liệu (thành viên phụ trách DB) | Server tạo repository xác thực khi khởi động và cần `ConnectionStrings:HealthInfoDb` trong `appsettings.json` | Cung cấp script/schema hiện hành, SQL Server và tài khoản thử qua kênh nhóm. Không commit connection string, mật khẩu hoặc dữ liệu riêng. |

Những nội dung trên mô tả điểm tích hợp, không chuyển quyền sở hữu phần việc. Mọi thay đổi code của module khác cần được thành viên phụ trách module đó review.

## Clone, cấu hình và chạy kiểm tra TCP

1. Clone repository và checkout nhánh:

   ```powershell
   git clone https://github.com/phu529/Health_Information_Lockup_System.git
   cd Health_Information_Lockup_System
   git switch --track origin/feature/core-tcp
   ```

2. Cài .NET 10 SDK. Build solution:

   ```powershell
   dotnet build Health_Information_Lockup_System.sln
   ```

3. Lấy `appsettings.json` cục bộ và thông tin SQL thử từ thành viên phụ trách DB. Đặt file trong `src/HealthServer/`; file này bị `.gitignore` có chủ ý và không được đưa lên Git. Không dùng file chứa mật khẩu thật trong ảnh hoặc log công khai.

4. Mở hai cửa sổ PowerShell tại thư mục repository. Khởi động Server trước:

   ```powershell
   dotnet run --project src\HealthServer\HealthServer.csproj
   ```

   Server cần báo đang nghe TCP cổng 5000. Sau đó chạy probe ở cửa sổ thứ hai:

   ```powershell
   dotnet run --project tools\TcpProbe\TcpProbe.csproj
   ```

   Kết quả đạt: `Tổng: 9/9 đạt`, trong đó T2 là dính gói (#9), T3 là phân mảnh (#8), T5 là hai client song song. Probe PING không đọc CSDL, nhưng tiến trình Server hiện vẫn cần có file cấu hình `appsettings.json` để khởi động.

5. Để giữ hai socket mở đủ lâu chụp ảnh số kết nối, giữ Server chạy và dùng cửa sổ thứ hai:

   ```powershell
   dotnet run --project tools\TcpProbe\TcpProbe.csproj -- --giu 2
   ```

   Probe báo đã giữ hai kết nối; Server cần hiện `đang nối: 2`. Nhấn Enter ở cửa sổ probe để đóng hai kết nối.

### Ghi chú về T9

T9 của probe gửi RST rồi xác nhận kết nối mới vẫn được phục vụ. Dòng hướng dẫn cuối của probe nhắc một câu log “mất kết nối”, nhưng lần chạy Windows có thể ghi lý do đóng khác (ví dụ “client đóng kết nối”). Vì vậy đánh giá T9 theo kết quả `PASS` và việc PING trên kết nối mới thành công; không yêu cầu khớp nguyên văn câu log.

## Mở bằng Visual Studio và kiểm tra giao diện

1. Mở `Health_Information_Lockup_System.sln`, chọn **Build > Build Solution**.
2. Chọn cấu hình khởi động nhiều project; đặt `HealthServer` và `HealthClient` thành **Start**, Server đứng trước Client. Nhấn **F5** để mở Server và một HealthClient.
3. Mở thêm HealthClient instance thứ hai bằng **Debug > Start New Instance**. Với cả hai cửa sổ, gửi thao tác đăng nhập thử được thành viên phụ trách DB cung cấp. Xác nhận Server nhận cả hai yêu cầu, cả hai giao diện vẫn phản hồi, rồi lưu ảnh có hai cửa sổ và log Server.
4. Nếu thiếu `appsettings.json`, SQL Server, schema hoặc tài khoản thử, không kết luận kiểm tra GUI đạt. Ghi rõ điều kiện thiếu và để tiêu chí GUI ở trạng thái chưa xác minh.

Visual Studio 2026 đã mở Solution và báo build thành công. Khi thử khởi chạy thêm HealthServer, cửa sổ console báo SocketException 10048 do cổng 5000 đã có tiến trình `HealthServer.exe` cùng dự án sử dụng. Không dừng tiến trình đó vì nó đã tồn tại trước lần khởi chạy này. Cần kiểm tra cửa sổ Server đang chạy, hoặc đóng đúng tiến trình của dự án rồi mới chạy lại một instance; không mở hai Server cùng cổng.

## Ảnh và log đã có

- Ảnh TcpProbe 9/9 và ảnh log Server do người chạy cung cấp được dùng riêng trong báo cáo giữa kỳ, không nằm trong commit README này.
- [`probe-run.log`](probe-run.log): log TcpProbe.
- [`server-run.log`](server-run.log): log Server từ lần kiểm chứng trước.

Ảnh hai cửa sổ HealthClient đã được dùng trong báo cáo Google Docs nhưng chưa lưu trong thư mục `docs/evidence/` của repo. Ảnh hiện có cho thấy cả hai cửa sổ đã mở; cần thêm minh chứng thao tác gửi/nhận và log tương ứng để xác nhận đủ tiêu chí. Khi chụp bổ sung, lưu tại `docs/evidence/healthclient-two-windows.png` rồi cập nhật trạng thái bảng phía trên.

## Cập nhật trạng thái Git

- Mọi thay đổi README trong lượt bàn giao chỉ được commit/push vào `feature/core-tcp`; không stage các file khác đang có thay đổi trong working tree.
- PR nháp [#3](https://github.com/phu529/Health_Information_Lockup_System/pull/3) review `feature/core-tcp` vào `feature/gui-auth`. GitHub báo chưa mergeable tự động tại lúc mở PR; nhóm cần review và xử lý khác biệt trước khi quyết định merge. PR không tự merge và không có thay đổi trực tiếp nào được đẩy vào nhánh đích.
- Chỉ stage các file evidence/README/ảnh được duyệt; không dùng `git add -A`. Không đưa `appsettings.json`, file SQL chưa được duyệt hoặc file cá nhân vào commit.
