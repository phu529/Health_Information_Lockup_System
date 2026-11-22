# Đặc tả giao thức truyền thông Client <-> Server (Bước 1.3)

Hệ thống tra cứu triệu chứng và thông tin sức khỏe - Nhóm 5, NT106.
Người viết: Huỳnh Thanh Nguyên. Mã nguồn tương ứng: `src/HealthShared/LfFraming.cs`, `src/HealthServer/Network/`, `src/HealthClient/Network/`.

## 1. Tóm tắt nhanh

| Mục | Giá trị |
|---|---|
| Giao thức vận chuyển | TCP |
| Cổng Server | **5000** (hằng `ProtocolConstants.DefaultTcpPort`) |
| Cổng UDP dò Server | 8888 (Bước 2.2, Hoàng Anh; xem mục 9) |
| Mã hóa chữ | UTF-8 |
| Định dạng thông điệp | **Một dòng JSON, kết thúc bằng một ký tự LF (`\n`, 0x0A)** |
| Kích thước tối đa một thông điệp | 64 KiB = 65.536 byte (không tính LF) |
| Kiểu kết nối | Giữ mở, một kết nối gửi được nhiều yêu cầu liên tiếp |
| Mô hình | Client gửi yêu cầu, Server trả đúng **một** phản hồi cho mỗi yêu cầu |
| Số client đồng thời | Không giới hạn cứng; đã thử 20 client x 20 yêu cầu (Test Case #21) |

## 2. Vì sao phải đóng khung bằng LF

TCP là một dòng byte, không giữ ranh giới giữa các lần gửi:

- **Dính gói:** gửi 3 tin liên tiếp, bên nhận có thể nhận cả 3 trong một lần đọc.
- **Phân mảnh:** gửi 1 tin dài, bên nhận có thể nhận làm nhiều lần đọc, thậm chí bị cắt giữa một chữ có dấu (một chữ tiếng Việt chiếm 2-3 byte UTF-8).

Quy ước của nhóm: **hết một dòng là hết một thông điệp**. Bên nhận gom byte vào bộ đệm cho tới khi gặp LF thì cắt ra một thông điệp, phần còn lại giữ cho lần sau. Việc này do lớp `LfFrameReader` làm; Client và Server dùng chung nên luôn khớp nhau.

Quy tắc cho người viết code:

1. **Gửi:** luôn dùng `LfFraming.WriteAsync(stream, json)`. Không tự `stream.Write` byte thô.
2. **Nhận:** luôn dùng `LfFrameReader.ReadLineAsync()`. Mỗi kết nối một đối tượng `LfFrameReader`.
3. JSON phải nằm trên **một dòng**. `ProtocolHelper.SerializeData` luôn thỏa (xuống dòng trong chuỗi được escape thành `\n`). **Không** dùng `WriteIndented`.
4. Bên nhận chấp nhận cả CRLF (bỏ CR đứng trước LF) và bỏ qua dòng trống.

## 3. Cấu trúc thông điệp

### 3.1 Yêu cầu (Client -> Server) - lớp `BaseRequest`

```json
{"Command":"LOGIN","RequestId":"a1b2c3d4e5f6","Payload":"{\"TenDangNhap\":\"nguyenvana\",\"MatKhau\":\"...\"}"}
```

| Trường | Kiểu | Ý nghĩa |
|---|---|---|
| `Command` | string, bắt buộc | Tên lệnh, **không phân biệt hoa thường** (mục 5) |
| `RequestId` | string, nên có | Mã do Client đặt (`TcpClientHelper` tự sinh 12 ký tự). Server trả lại nguyên vẹn |
| `Payload` | string | Dữ liệu của lệnh, là **một chuỗi JSON lồng bên trong** (xem 3.3). Lệnh không cần dữ liệu thì để `""` |

### 3.2 Phản hồi (Server -> Client) - lớp `BaseResponse`

```json
{"Command":"LOGIN","RequestId":"a1b2c3d4e5f6","Status":200,"Message":"Đăng nhập thành công.","Payload":"{\"HoTen\":\"Nguyen Van A\"}"}
```

| Trường | Ý nghĩa |
|---|---|
| `Command` | Lặp lại lệnh của yêu cầu |
| `RequestId` | Lặp lại mã của yêu cầu. Client dùng để ghép phản hồi với yêu cầu (Test Case #2, #22) |
| `Status` | Mã kết quả (mục 4) |
| `Message` | Câu mô tả ngắn cho người đọc/log. Không chứa dữ liệu nhạy cảm |
| `Payload` | Dữ liệu kết quả (JSON lồng), `""` nếu không có |

### 3.3 Vì sao `Payload` là chuỗi

Phần khung (`Command`, `RequestId`, `Status`) cố định cho mọi lệnh nên bộ định tuyến đọc được mà không cần biết lệnh cụ thể. Mỗi lệnh tự định nghĩa lớp DTO của mình (Hoàng Anh, Bước 2.1) rồi `ProtocolHelper.SerializeData` thành chuỗi đặt vào `Payload`. Thêm lệnh mới không phải sửa khung.

## 4. Mã trạng thái (`ErrorCode`)

| Mã | Tên | Khi nào |
|---|---|---|
| 200 | Success | Thành công |
| 400 | BadRequest | JSON hỏng, thiếu `Command`, `Payload` sai định dạng, dữ liệu không hợp lệ |
| 401 | Unauthorized | Sai thông tin đăng nhập (dùng chung cho "sai tên" và "sai mật khẩu") |
| 404 | NotFound | `Command` không được Server hỗ trợ |
| 409 | Conflict | Trùng dữ liệu (ví dụ tên đăng nhập đã tồn tại) |
| 413 | PayloadTooLarge | Thông điệp vượt 64 KiB. **Server đóng kết nối sau khi trả 413** |
| 500 | InternalServerError | Lỗi phía Server khi xử lý |

## 5. Danh sách lệnh

| Lệnh | Payload yêu cầu | Phản hồi | Phụ trách | Trạng thái |
|---|---|---|---|---|
| `PING` | tùy ý (hoặc rỗng) | 200, `Message="PONG"`, `Payload` = đúng `Payload` đã gửi | Lõi TCP (Nguyên) | Xong, đã thử |
| `LOGIN` | `LoginPayload {TenDangNhap, MatKhau}` | 200 + `AuthSuccessPayload {HoTen}`; 401 nếu sai; 400 nếu thiếu | Auth (Trọng Phú, Bước 4.3) | Đã nối vào lõi |
| `REGISTER` | `RegisterPayload {TenDangNhap, MatKhau, HoTen, Email?}` | 200 + `AuthSuccessPayload`; 400 sai luật; 409 trùng | Auth (Trọng Phú, Bước 4.1/4.3) | Đã nối vào lõi |
| `SEARCH`, `GET_DETAIL`, `GET_HISTORY`, `PROFILE`... | do Hoàng Anh định nghĩa | - | Hoàng Anh (Bước 2.3, 2.4) | Chưa có, đăng ký bằng `router.Register` |

Ví dụ yêu cầu `PING` có dấu tiếng Việt và phản hồi:

```
>>> {"Command":"PING","RequestId":"p1","Payload":"Xin chào"}\n
<<< {"Command":"PING","RequestId":"p1","Status":200,"Message":"PONG","Payload":"Xin chào"}\n
```

### Thêm một lệnh mới (cho thành viên khác)

Ở `HealthServer/Program.cs`, sau dòng tạo `router`:

```csharp
router.Register("SEARCH", async (session, request, token) =>
{
    var yc = ProtocolHelper.DeserializeData<SearchRequestDto>(request.Payload);
    var ketQua = await timKiem.TimAsync(yc, token);          // gọi Repository
    return RequestRouter.Reply(request.Command, ErrorCode.Success, "OK",
                               ProtocolHelper.SerializeData(ketQua));
});
```

Không phải sửa `TcpServer`. Bộ định tuyến tự điền `RequestId` vào phản hồi và tự bắt lỗi (trả 500, không làm sập kết nối).

## 6. Vòng đời một kết nối

1. Client nối TCP tới `Host:5000` (tối đa 5 giây). `TcpClientHelper` tự nối ở lần gửi đầu tiên.
2. Client gửi nhiều yêu cầu trên cùng kết nối. Có thể gửi yêu cầu tiếp khi chưa nhận phản hồi trước; Client ghép theo `RequestId`. Server xử lý **tuần tự theo thứ tự nhận** trong cùng một kết nối, nên phản hồi cũng về đúng thứ tự.
3. Client im lặng quá **90 giây** thì Server tự cắt. Client nên gửi `PING` mỗi **30 giây** để giữ kết nối (Heartbeat, Hoàng Anh).
4. Thoát đàng hoàng: Client đóng socket. Server thấy hết dòng, dọn phiên.
5. Rớt đột ngột (tắt tiến trình, rút dây): Server dọn phiên khi lệnh đọc/ghi báo lỗi hoặc khi hết 90 giây im lặng. TCP keep-alive được bật để hệ điều hành dò thêm.
6. Server tắt: đóng mọi kết nối. Client thấy hết dòng, báo `ConnectionLost`; yêu cầu đang chờ nhận `IOException`; lần gửi sau tự nối lại.

## 7. Lỗi và cách xử lý

| Tình huống | Server | Client |
|---|---|---|
| JSON hỏng | Trả 400 (không có `RequestId` vì không đọc được), **giữ kết nối** | Bỏ qua phản hồi không ghép được; yêu cầu hết giờ |
| `Command` lạ | Trả 404 kèm `RequestId`, giữ kết nối | Đọc `Status` |
| Dòng > 64 KiB | Trả 413 rồi **đóng kết nối** (không còn biết ranh giới) | Nhận `IOException` ở lần sau |
| Lỗi trong hàm xử lý | Trả 500, ghi log tên loại lỗi, giữ kết nối | Hiện thông báo chung |
| Không nối được Server | - | `SocketException` -> `MaKetQua.LoiKetNoi` |
| Quá 10 giây không có phản hồi | - | `TimeoutException` -> `MaKetQua.LoiKetNoi` |
| Mất kết nối giữa chừng | Dọn phiên, ghi log | `IOException` -> `LoiKetNoi`; **không tính** vào số lần đăng nhập sai (Test Case #4) |

Nguyên tắc chung: **lỗi mạng và lỗi nghiệp vụ tách riêng.** Lỗi mạng đi bằng ngoại lệ (`SocketException`, `IOException`, `TimeoutException`); lỗi nghiệp vụ đi bằng `Status` trong phản hồi.

## 8. Bảo mật (hiện trạng và việc còn lại)

- Hiện tại dữ liệu đi **chưa mã hóa** (TCP trần), kể cả mật khẩu trong `LOGIN`/`REGISTER`. Mật khẩu được băm PBKDF2 khi lưu trong CSDL, nhưng khi đi trên mạng LAN vẫn đọc được bằng Wireshark.
- Bọc TLS bằng `SslStream` là việc của **Trọng Phú, Bước 4.4**. Thiết kế đã chừa chỗ: chỉ cần thay `NetworkStream` bằng `SslStream` trong `ClientSession` và `TcpClientHelper`; tầng đóng khung LF không đổi.
- Server không ghi mật khẩu/Payload vào log, chỉ ghi tên loại lỗi.

## 9. Việc liên quan chưa thuộc Bước 1.3

- **UDP Discovery** (cổng 8888, Hoàng Anh Bước 2.2): đã có mã riêng. Tích hợp vào luồng kết nối (Client tự dò IP) là **Bước 1.5**. Đến lúc đó, Client đặt `TcpClientHelper.Shared.Host` bằng IP dò được. Hiện tại muốn nối từ máy khác thì đặt biến môi trường `HEALTH_SERVER_HOST`.
- **Heartbeat thật:** `HeartbeatClient.SendPingAsync` hiện còn là hàm giả. Có thể thay bằng `TcpClientHelper.Shared.PingAsync()`.
- **Phiên đăng nhập, khóa sai 5 lần, RBAC:** Trọng Phú (Bước 4.3, 4.4). `ClientSession` hiện chưa lưu người dùng đã đăng nhập.
- **Tin Server chủ động đẩy** (không có yêu cầu trước): chưa có. Nếu cần, thêm hàm phát tin duyệt `_phien` trong `TcpServer`.

## 10. Cách kiểm thử

Công cụ `tools/TcpProbe` (không nằm trong `.sln`) thử bằng lệnh `PING`, không cần CSDL:

| Kịch bản | Test Case |
|---|---|
| T2: 3 yêu cầu trong một lần ghi (dính gói) | #9 |
| T3: một yêu cầu cắt từng 3 byte, giữa chữ có dấu (phân mảnh) | #8 |
| T5: hai client đồng thời, mỗi client nhận đúng `RequestId` | #2, #22 |
| T6: 20 client x 20 yêu cầu | #21 |
| T9: rớt đột ngột rồi nối lại | #14 |
| T1, T4, T7, T8: PING cơ bản, CRLF/dòng trống, 404/400 giữ kết nối, 413 | - |

Lệnh chạy xem phần đầu `tools/TcpProbe/Program.cs`.
