# Bước 1.3 — Core TCP: hướng dẫn chạy và bàn giao

Tài liệu này ghi trạng thái kiểm chứng, cách chạy lại và các điểm tích hợp của lõi TCP. Đặc tả bản tin đầy đủ nằm tại [`docs/protocol.md`](../protocol.md).

Lần kiểm tra Windows gần nhất: **09/11/2026**. Build Solution thành công với 0 lỗi/12 cảnh báo; sau build, HealthServer chạy trên cổng 5000 và TcpProbe đạt 9/9.

## Trạng thái theo bảng phân công

| Tiêu chí | Trạng thái hiện tại | Minh chứng / phần còn lại |
|---|---|---|
| JSON UTF-8 một dòng, kết thúc LF; tài liệu đặc tả | Đạt | [`docs/protocol.md`](../protocol.md), `src/HealthShared/LfFraming.cs` |
| Server bất đồng bộ, một tác vụ cho mỗi client, quản lý phiên đồng thời | Đạt kiểm tra chức năng | `AcceptTcpClientAsync`, `ConcurrentDictionary`, log Server |
| TcpClientHelper phía Client | Đạt kiểm tra chức năng | `src/HealthClient/Network/TcpClientHelper.cs` |
| Test #8 phân mảnh và #9 dính gói | Đạt | TcpProbe T3 và T2; ảnh chụp được dùng riêng trong báo cáo giữa kỳ |
| Build toàn bộ Solution trên Windows | Đạt: 0 lỗi, 12 cảnh báo | Chạy `dotnet build Health_Information_Lockup_System.sln --no-restore`; cảnh báo nullability còn lại trong Shared, Client và Discovery/Heartbeat |
| Hai client TCP cùng kết nối và gửi/nhận | Đạt bằng TcpProbe | T5: X 50/50, Y 50/50; Server ghi nhận 2 kết nối. Ảnh chụp được dùng riêng trong báo cáo giữa kỳ |
| Hai cửa sổ HealthClient thật cùng gửi yêu cầu, giao diện không treo | **Chưa xác minh** | Cần chạy trên máy có cấu hình SQL và tài khoản thử; ảnh hiện tại là probe TCP, chưa phải hai cửa sổ GUI |
| Pull Request để nhóm review | **Chưa mở** | Nhánh riêng là `feature/core-tcp`; đích review dự kiến `feature/gui-auth`, không merge tự động |

Vì còn thiếu kiểm chứng hai cửa sổ HealthClient và PR, chưa đánh dấu toàn bộ Bước 1.3 là 100%.

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

Visual Studio chưa có trong môi trường kiểm chứng hiện tại. Build solution được chạy bằng .NET CLI trên Windows; chưa xác nhận thao tác khởi chạy nhiều project qua giao diện Visual Studio.

## Ảnh và log đã có

- Ảnh TcpProbe 9/9 và ảnh log Server do người chạy cung cấp được dùng riêng trong báo cáo giữa kỳ, không nằm trong commit README này.
- [`probe-run.log`](probe-run.log): log TcpProbe.
- [`server-run.log`](server-run.log): log Server từ lần kiểm chứng trước.

Ảnh HealthClient GUI hai cửa sổ chưa có. Khi chụp bổ sung, lưu tại `docs/evidence/healthclient-two-windows.png` và cập nhật trạng thái bảng phía trên sau khi xác nhận yêu cầu gửi/nhận thực sự thành công.

## Cập nhật trạng thái Git

- Mọi thay đổi của bước này chỉ được commit/push vào `feature/core-tcp`.
- Nhánh hiện dựa trên `feature/gui-auth`; tạo PR để review vào `feature/gui-auth` nhằm cô lập commit Step 1.3. PR không tự merge và không được đẩy trực tiếp lên nhánh của thành viên khác hoặc `main`.
- Chỉ stage các file evidence/README/ảnh được duyệt; không dùng `git add -A`. Không đưa `appsettings.json`, file SQL chưa được duyệt hoặc file cá nhân vào commit.
