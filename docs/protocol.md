# Đặc tả giao thức truyền thông Client <-> Server (cập nhật sau Bước 1.3)

Hệ thống tra cứu triệu chứng và thông tin sức khỏe - Nhóm 5, NT106.
Bản gốc: Huỳnh Thanh Nguyên (Bước 1.3). Bản này đối chiếu lại với mã nguồn trên nhánh `main` (commit `16fc5cd`) và **thêm các lệnh sẽ làm ở giai đoạn 2-3**.
Mã nguồn tương ứng: `src/HealthShared/` (`LfFraming.cs`, `ProtocolConstants.cs`, `ErrorCode.cs`, `AuthProtocol.cs`, `UserProtocol.cs`), `src/HealthServer/Network/`, `src/HealthClient/Network/`.

**Cách đọc cột "Trạng thái" trong tài liệu này**

| Nhãn | Nghĩa |
|---|---|
| Có trong mã | Đã có mã và đã đăng ký ở Server |
| Có mã, chưa đăng ký | Có hàm xử lý nhưng `Program.cs` chưa gọi `Register`. Phải sửa trước khi dùng |
| Kế hoạch | Chưa có mã. Tên lệnh và cấu trúc là **đề xuất**, người phụ trách được chỉnh nhưng phải cập nhật lại file này |

---

## 1. Tóm tắt nhanh

| Mục | Giá trị |
|---|---|
| Giao thức vận chuyển | TCP |
| Cổng Server | **5000** (`ProtocolConstants.DefaultTcpPort`) |
| Cổng UDP dò Server | **8888** (`Program.cs` của Server) |
| Mã hóa chữ | UTF-8 |
| Định dạng thông điệp | **Một dòng JSON, kết thúc bằng một ký tự LF (`\n`, 0x0A)** |
| Kích thước tối đa một thông điệp | 64 KiB = 65.536 byte (không tính LF), `ProtocolConstants.MaxMessageBytes` |
| Kiểu kết nối | Giữ mở, một kết nối gửi được nhiều yêu cầu liên tiếp |
| Server im lặng tối đa | 90 giây (`ServerIdleTimeoutSeconds`) |
| Client gửi PING mỗi | 30 giây (`ClientPingIntervalSeconds`) |
| Client chờ phản hồi tối đa | 10 giây (`ClientRequestTimeoutSeconds`) |
| Client chờ nối tối đa | 5 giây (`ClientConnectTimeoutSeconds`) |
| Mô hình | Client gửi yêu cầu, Server trả đúng **một** phản hồi cho mỗi yêu cầu. Ngoài ra Server có thể **chủ động đẩy tin** (mục 8) |

## 2. Vì sao phải đóng khung bằng LF

TCP là một dòng byte, không giữ ranh giới giữa các lần gửi:

- **Dính gói:** gửi 3 tin liên tiếp, bên nhận có thể nhận cả 3 trong một lần đọc.
- **Phân mảnh:** gửi 1 tin dài, bên nhận có thể nhận làm nhiều lần đọc, thậm chí bị cắt giữa một chữ có dấu.

Quy ước: **hết một dòng là hết một thông điệp.** Quy tắc cho người viết code:

1. **Gửi:** dùng `LfFraming.WriteAsync(stream, json)`. Ở Server, **mọi lần ghi phải đi qua `ClientSession.GuiAsync`** (có `SemaphoreSlim`), cấm ghi thẳng vào `NetworkStream`. Phản hồi yêu cầu và tin đẩy cùng đi qua một đường này nên không bị trộn dòng.
2. **Nhận:** dùng `LfFrameReader.ReadLineAsync()`. Mỗi kết nối một đối tượng, chỉ một vòng đọc.
3. JSON phải nằm trên **một dòng**. `ProtocolHelper.SerializeData` luôn thỏa. **Không** dùng `WriteIndented`.
4. Bên nhận chấp nhận cả CRLF và bỏ qua dòng trống.
5. `LfFraming.WriteAsync` ném `FrameTooLargeException` nếu thông điệp > 64 KiB; Server trả 413 rồi **đóng kết nối**. Vì vậy mọi dữ liệu lớn phải **chia khối** (mục 9).

## 3. Cấu trúc thông điệp

### 3.1 Yêu cầu (Client -> Server) - `BaseRequest`

```json
{"Command":"LOGIN","RequestId":"a1b2c3d4e5f6","Payload":"{\"TenDangNhap\":\"nguyenvana\",\"MatKhau\":\"...\"}"}
```

| Trường | Ý nghĩa |
|---|---|
| `Command` | Tên lệnh, không phân biệt hoa thường |
| `RequestId` | Mã do Client đặt (12 ký tự). Server trả lại nguyên vẹn |
| `Payload` | **Chuỗi JSON lồng bên trong**. Lệnh không cần dữ liệu thì để `""` |

### 3.2 Phản hồi (Server -> Client) - `BaseResponse`

```json
{"Command":"LOGIN","RequestId":"a1b2c3d4e5f6","Status":200,"Message":"Đăng nhập thành công.","Payload":"{\"HoTen\":\"Nguyen Van A\"}"}
```

| Trường | Ý nghĩa |
|---|---|
| `Command` | Lặp lại lệnh của yêu cầu |
| `RequestId` | Lặp lại mã của yêu cầu để Client ghép phản hồi |
| `Status` | Mã kết quả (mục 4) |
| `Message` | Câu mô tả ngắn, không chứa dữ liệu nhạy cảm |
| `Payload` | Dữ liệu kết quả (JSON lồng), `""` nếu không có |

### 3.3 Vì sao `Payload` là chuỗi
Khung (`Command`, `RequestId`, `Status`) cố định nên bộ định tuyến không cần biết lệnh cụ thể. Mỗi lệnh tự định nghĩa DTO trong `HealthShared`, rồi `ProtocolHelper.SerializeData` thành chuỗi. Thêm lệnh mới không phải sửa khung.

## 4. Mã trạng thái (`ErrorCode`)

| Mã | Tên | Khi nào | Trạng thái |
|---|---|---|---|
| 200 | Success | Thành công | Có trong mã |
| 400 | BadRequest | JSON hỏng, thiếu `Command`, dữ liệu không hợp lệ | Có trong mã |
| 401 | Unauthorized | Sai thông tin đăng nhập (dùng chung cho "sai tên" và "sai mật khẩu"); hoặc chưa đăng nhập khi gọi lệnh cần đăng nhập | Có trong mã |
| 404 | NotFound | `Command` không được Server hỗ trợ, hoặc không thấy dữ liệu | Có trong mã |
| 409 | Conflict | Trùng dữ liệu (tên đăng nhập đã tồn tại) | Có trong mã |
| 413 | PayloadTooLarge | Thông điệp > 64 KiB. **Server đóng kết nối sau khi trả 413** | Có trong mã |
| 423 | Locked | Tài khoản bị khóa tạm (sai mật khẩu 5 lần, khóa 15 phút) hoặc bị giới hạn tần suất lấy OTP | Có trong mã |
| 500 | InternalServerError | Lỗi phía Server | Có trong mã |
| 403 | Forbidden | Đã đăng nhập nhưng **không đủ quyền** (Test #12) | Kế hoạch: cần thêm vào `ErrorCode` |
| 429 | TooManyRequests | Gửi báo động 115 quá 1 lần/phút (`RATE_LIMITED`) | Kế hoạch: cần thêm vào `ErrorCode` |

Ghi chú: hiện `ErrorCode` chưa có 403 và 429. Hai mã này do người làm Bước 4.4 và 4.7 thêm vào `HealthShared/ErrorCode.cs`, rồi cập nhật bảng trên.

## 5. Danh sách lệnh

### 5.1 Lệnh đã có

| Lệnh | Payload yêu cầu | Phản hồi | Trạng thái |
|---|---|---|---|
| `PING` | tùy ý | 200, `Message="PONG"`, `Payload` = đúng `Payload` đã gửi | Có trong mã |
| `LOGIN` | `LoginPayload {TenDangNhap, MatKhau}` | 200 + `AuthSuccessPayload {HoTen}`; 401 sai; 423 đang bị khóa; 400 thiếu | Có trong mã |
| `REGISTER` | `RegisterPayload {TenDangNhap, MatKhau, HoTen, Email}` | 200; 400 sai luật; 409 trùng | Có trong mã |
| `FORGOT_PWD_REQUEST_OTP` | `{Email, TenDangNhap}` | Bước 1: gửi OTP qua Gmail SMTP | Có trong mã |
| `FORGOT_PWD_VERIFY_OTP` | `{Email, OtpCode}` | Bước 2: 200 + `{ResetToken}` | Có trong mã |
| `FORGOT_PWD_RESET` | `{ResetToken, MatKhauMoi}` | Bước 3: đổi mật khẩu trong 1 `SqlTransaction`, ghi `AuditLogs`, thu hồi mọi phiên cũ | Có trong mã |
| `FORGOT_PWD` | `{TenDangNhap, Email, MatKhauMoi}` | Lệnh một bước cũ, còn đăng ký ở `AuthRoutes` | Có trong mã (nên bỏ sau khi nhóm thống nhất) |
| `SEARCH`, `GET_DETAIL` | do Bước 2.3 định nghĩa | Danh sách bệnh / chi tiết | Có trong mã, nhưng `TcpServer` xử lý **trước** bộ định tuyến (`TryHandleSearchAsync`), không qua `router.Register` |
| `GET_PROFILE` (và `PROFILE`) | rỗng | Hồ sơ của phiên đang đăng nhập (`UserProfilePayload`) | **Có mã, chưa đăng ký trên `main`** |
| `UPDATE_PROFILE` | `UpdateProfilePayload` | 200 | **Có mã, chưa đăng ký trên `main`** |
| `GET_HISTORY` | `GetHistoryPayload {Limit=50}` | Danh sách `SearchHistoryItemPayload` | **Có mã, chưa đăng ký trên `main`** |
| `ADD_HISTORY` | `AddHistoryPayload {TuKhoa, DiseaseId?, SoKetQua}` | 200 | **Có mã, chưa đăng ký trên `main`** |
| `CLEAR_HISTORY` | rỗng | 200 | **Có mã, chưa đăng ký trên `main`** |

**Việc cần sửa ngay:** commit `16fc5cd` đã làm mất dòng đăng ký các lệnh hồ sơ và lịch sử khỏi `src/HealthServer/Program.cs`. Thêm lại dòng sau, ngay sau `AuthRoutes.Register(...)`:

```csharp
UserProfileHistoryRoutes.Register(router, serverRepository);
```

Các lệnh hồ sơ và lịch sử đều kiểm `session.UserId`; chưa đăng nhập thì trả 401.

### 5.2 Lệnh sẽ làm (đề xuất, người phụ trách chốt chi tiết)

Chữ in nghiêng là DTO chưa tồn tại. Mỗi DTO viết trong `HealthShared`, thêm hằng tên lệnh vào một lớp `...Commands` giống `AuthCommands`.

| Nhóm | Lệnh | Hướng | Payload gợi ý | Quyền | Phụ trách |
|---|---|---|---|---|---|
| Phiên | `LIST_SESSIONS` | C->S | rỗng; SystemAdmin có thể gửi `{UserId}` | User: chỉ của mình; SystemAdmin: mọi tài khoản | NV4 (4.7) |
| | `SESSION_LIST` / phản hồi | S->C | mảng: `SessionId, TenMay, HeDieuHanh, DiaChiIP, ThoiGianDangNhap, LaThietBiNay` | | NV4 |
| | `FORCE_LOGOUT` | C->S | `{SessionId, LyDo?}` | User: phiên của mình; SystemAdmin: mọi phiên. Thiếu quyền trả 403 | NV4 |
| | `NEW_LOGIN_ALERT` | S->C (đẩy) | `{TenMay, DiaChiIP, ThoiGian}` gửi cho các phiên khác của cùng tài khoản | | NV4 |
| | `FORCED_LOGOUT_NOTICE` | S->C (đẩy) | `{LyDo}` gửi cho phiên bị đá, rồi Server đóng kết nối | | NV4 |
| Khảo sát | `SURVEY_SUBMIT` | C->S | *SurveyPayload*: nhóm cơ quan, danh sách triệu chứng, thời gian kéo dài, mức độ/ghi chú | Đã đăng nhập | NV2 (2.6) |
| | `SURVEY_GET_LATEST` | C->S | rỗng | Đã đăng nhập | NV2 |
| Chat | `CHAT_SEND` | C->S | `{DenUserId, NoiDung, DinhKemKhaoSat: bool}`; nếu `true`, Server tự lấy phiếu khảo sát mới nhất của người gửi đính kèm | Bác sĩ do ContentAdmin đóng vai | NV2 (2.6) |
| | `CHAT_RECV` | S->C (đẩy) | `{TuUserId, NoiDung, PhieuKhaoSat?, ThoiGian}` | | NV2 |
| Đẩy | `BROADCAST_SEND` | C->S | `{Loai, TieuDe, NoiDung}` | ContentAdmin, SystemAdmin | NV2 (2.7) |
| | `BROADCAST` | S->C (đẩy) | `{Loai, TieuDe, NoiDung, ThoiGian}`; offline thì gửi bù khi đăng nhập lại | | NV2 |
| | `EMERGENCY_ALERT` | C->S | `{TrieuChung, Muc}`; tối đa 1 lần/phút/người dùng, vượt thì 429 | Đã đăng nhập | NV2 + NV4 |
| | `EMERGENCY_PUSH` | S->C (đẩy) | `{TenUser, TrieuChung, Muc, ThoiGian, ThongTinCuuHo}` **chỉ gửi cho phiên SystemAdmin** | | NV2 |
| Bác sĩ | `DOCTOR_STATUS` | C->S | `{TrangThai: "Ranh" \| "Ban"}`; Server lưu và cập nhật nhãn Rảnh/Bận cho người dùng | Chỉ ContentAdmin (vai bác sĩ); vai khác trả 403 | NV2 (2.6) |
| Quản trị | `ADMIN_CRUD_*` (ví dụ `ADMIN_CRUD_CREATE`, `_UPDATE`, `_DELETE`) | C->S | `{DoiTuong: Benh \| TrieuChung \| Thuoc \| BacSi \| FAQ, DuLieu}`; mọi lần ghi đều ghi `AuditLogs` (không ghi mật khẩu/token) | ContentAdmin, SystemAdmin; User thường trả 403 | NV4 (4.4) + NV3 (DAL) |
| Báo lỗi | `UPLOAD_BEGIN`, `UPLOAD_CHUNK`, `UPLOAD_END` | C->S | Xem mục 9 | Đã đăng nhập | NV2 (2.7) |

## 6. Vòng đời một kết nối

1. Client nối TCP tới `Host:5000` (tối đa 5 giây).
2. Client gửi nhiều yêu cầu trên cùng kết nối, ghép phản hồi theo `RequestId`. Server xử lý **tuần tự theo thứ tự nhận** trong một kết nối.
3. Client im lặng quá **90 giây** thì Server cắt. Client gửi `PING` mỗi **30 giây**.
4. Thoát đàng hoàng: Client đóng socket, Server thấy hết dòng và dọn phiên.
5. Rớt đột ngột: Server dọn phiên khi đọc/ghi báo lỗi hoặc hết 90 giây. TCP keep-alive được bật.
6. Server tắt: đóng mọi kết nối. Client báo mất kết nối; lần gửi sau tự nối lại.

### 6.1 Chính sách phiên đăng nhập (đã chốt 24-25/11)

- Một tài khoản được **đăng nhập tối đa 5 thiết bị** cùng lúc. Máy cũ **không** bị đá khi còn ≤ 5 máy.
- Khi có máy mới đăng nhập, các phiên khác của tài khoản nhận `NEW_LOGIN_ALERT`.
- Phiên chỉ bị thu hồi khi: `FORCE_LOGOUT`, `FORGOT_PWD_RESET`, hết hạn 90 giây im lặng, hoặc client đóng kết nối.
- **Hiện trạng mã:** `AuthRequestHandler.LoginAsync` đang gọi `_thuHoiPhien(user.UserId)` ngay khi đăng nhập thành công, tức là đá hết máy cũ. Phải bỏ lời gọi này ở LOGIN (NV4, Bổ sung 4.7). Giữ lời gọi ở `FORGOT_PWD_RESET`.
- **Đã chốt:** khi đăng nhập máy thứ 6, Server **từ chối** và trả `409 Conflict` kèm thông báo "Tài khoản đã đăng nhập đủ 5 thiết bị, hãy đăng xuất bớt một máy"; **không** đá phiên cũ nhất.

## 7. Lỗi và cách xử lý

| Tình huống | Server | Client |
|---|---|---|
| JSON hỏng | Trả 400, **giữ kết nối** | Bỏ qua phản hồi không ghép được; yêu cầu hết giờ |
| `Command` lạ | Trả 404 kèm `RequestId`, giữ kết nối | Đọc `Status` |
| Dòng > 64 KiB | Trả 413 rồi **đóng kết nối** | Nhận `IOException` ở lần sau |
| Lỗi trong hàm xử lý | Trả 500, ghi log tên loại lỗi, giữ kết nối | Hiện thông báo chung |
| Không nối được Server | - | `SocketException` -> `LoiKetNoi` |
| Quá 10 giây không có phản hồi | - | `TimeoutException` -> `LoiKetNoi` |
| Mất kết nối giữa chừng | Dọn phiên, ghi log | `IOException` -> `LoiKetNoi`; **không tính** vào số lần đăng nhập sai (Test #4) |

Nguyên tắc: **lỗi mạng và lỗi nghiệp vụ tách riêng.** Lỗi mạng đi bằng ngoại lệ; lỗi nghiệp vụ đi bằng `Status`.

## 8. Tin Server chủ động đẩy (realtime)

Hiện **chưa có** trong mã. Quy ước để các thành viên làm cùng một kiểu:

- Tin đẩy dùng chính khung `BaseResponse`, với `Command` là tên tin (ví dụ `BROADCAST`), `RequestId` là chuỗi rỗng `""`, `Status` 200.
- Client phải có một nhánh nhận tin có `RequestId` rỗng và chuyển cho giao diện (không đợi ghép với yêu cầu nào). Hiện `TcpClientHelper` ghép theo `RequestId`; phải bổ sung nhánh này (NV1 và NV2 thống nhất).
- Mọi lần đẩy dùng `ClientSession.GuiAsync` (mục 2).
- Khi phát cho nhiều phiên, **bắt lỗi từng phiên** (`IOException`, `ObjectDisposedException`): một máy rớt mạng không được làm cả vòng lặp dừng.

### 8.1 Hàng đợi gửi: khi nào được bỏ tin

| Tin | Cách xử lý khi hàng đợi đầy hoặc gửi lỗi |
|---|---|
| `BROADCAST`, `NEW_LOGIN_ALERT` | Được phép bỏ tin cũ (`DropOldest`), vì tin sau thay thế tin trước |
| `EMERGENCY_PUSH`, `CHAT_RECV` | **Không được bỏ im lặng.** Ghi vào CSDL trước (`ThongBao`/`TinNhan`), rồi mới đẩy. Gửi lỗi thì giữ lại để gửi bù khi người nhận online, và ghi log |

## 9. Gửi tệp báo lỗi theo khối (`UPLOAD_*`)

Vì một thông điệp tối đa 64 KiB, tệp ảnh/PDF phải cắt khối:

1. `UPLOAD_BEGIN {FileId, TenFile, TongByte, SoKhoi}`: Server kiểm tra đuôi tệp (ảnh, PDF) và trần dung lượng tổng, rồi tạo chỗ nhận. Trần tổng do nhóm chốt (ghi chú ban đầu: tối đa 5 MB).
2. `UPLOAD_CHUNK {FileId, ChiSo, DuLieuBase64}`: **mỗi khối dữ liệu thô tối đa 32 KiB.** Lý do: Base64 làm dữ liệu to ra khoảng 4/3 lần (32 KiB thành khoảng 43,7 KiB), cộng phần JSON bọc ngoài vẫn dưới 64 KiB. Nếu để khối thô 64 KiB thì thông điệp vượt trần, Server trả 413 và **đóng kết nối**.
3. `UPLOAD_END {FileId}`: Server ghép khối theo `ChiSo`, kiểm tra đủ số khối và đủ `TongByte`, rồi lưu và tạo bản ghi `BaoLoi`.

## 10. Bảo mật (hiện trạng và việc còn lại)

- Hiện dữ liệu đi **chưa mã hóa** (TCP trần), kể cả mật khẩu trong `LOGIN`/`REGISTER`. Mật khẩu được băm PBKDF2 khi lưu, nhưng trên mạng LAN vẫn đọc được bằng Wireshark.
- Bọc TLS bằng `SslStream` là việc của NV4, Bước 4.4: thay `NetworkStream` bằng `SslStream` trong `ClientSession` và `TcpClientHelper`; tầng đóng khung không đổi.
- Trước khi tạo chứng chỉ, thêm `*.pfx` và `*.key` vào `.gitignore`.
- Server không ghi mật khẩu, OTP, token hoặc `Payload` vào log; chỉ ghi tên loại lỗi.
- Phân quyền **do Server kiểm tra theo `session`**, không dựa vào việc Client ẩn nút. Thiếu quyền trả 403.

## 11. UDP dò Server (cổng 8888)

Đã có trong `Program.cs` (Server) và `DiscoveryService.cs` (Client).

| Bước | Nội dung |
|---|---|
| 1 | Client gửi UDP broadcast chuỗi `DISCOVER_HEALTH_SERVER` tới cổng 8888 |
| 2 | Server nghe cổng 8888; nhận đúng chuỗi trên thì trả về `HEALTH_SERVER_RESPONSE` cho đúng địa chỉ gửi |
| 3 | Client lấy **địa chỉ IP của gói trả lời** làm IP Server, dùng cổng TCP mặc định **5000** (gói trả lời không chứa số cổng) |

Tích hợp vào luồng kết nối (Client tự dò IP, đặt `TcpClientHelper.Shared.Host`) là Bước 1.5. Hiện muốn nối từ máy khác thì đặt biến môi trường `HEALTH_SERVER_HOST`.

## 12. Cách kiểm thử

Công cụ `tools/TcpProbe` (không nằm trong `.sln`) thử bằng lệnh `PING`, không cần CSDL:

| Kịch bản | Test Case |
|---|---|
| T2: 3 yêu cầu trong một lần ghi (dính gói) | #9 |
| T3: một yêu cầu cắt từng 3 byte, giữa chữ có dấu (phân mảnh) | #8 |
| T5: hai client đồng thời, mỗi client nhận đúng `RequestId` | #2, #22 |
| T6: 20 client x 20 yêu cầu | #21 |
| T9: rớt đột ngột rồi nối lại | #14 |

Các lệnh mới gắn với Test Case: `LOGIN` đa phiên (#7), `FORGOT_PWD_*` (#6, #24, #25), `SURVEY_SUBMIT` (#27), `CHAT_SEND` kèm khảo sát (#28), `GET_HISTORY` (#11).

## 13. Việc còn lại để file này đúng 100%

- [ ] Thêm lại `UserProfileHistoryRoutes.Register(...)` vào `Program.cs`.
- [ ] Thêm `Forbidden = 403` và `TooManyRequests = 429` vào `ErrorCode`.
- [x] Chốt chính sách khi đăng nhập máy thứ 6 (mục 6.1: trả `409 Conflict`, không đá phiên cũ).
- [ ] Sau khi cài từng lệnh ở mục 5.2, đổi nhãn "Kế hoạch" thành "Có trong mã" và sửa DTO cho đúng.
- [ ] Đổi tên `src/HealthServer/RequestRouter.cs` thành `SearchRequestHandler.cs` (file đó chứa lớp `SearchRequestHandler`, **không** gộp với `Network/RequestRouter.cs`).
