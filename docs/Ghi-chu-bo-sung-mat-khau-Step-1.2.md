# Ghi chú bổ sung mật khẩu trong giao diện Client – Bước 1.2

## Phạm vi bổ sung

Ở Bước 1.2, bổ sung một số chức năng liên quan đến mật khẩu ở phía giao diện Client, phối hợp cùng Trọng Phú. Mục đích là giúp người dùng dễ theo dõi độ mạnh mật khẩu và thuận tiện hơn khi sử dụng chức năng đăng nhập, đăng ký.

## 1. Thanh đánh giá độ mạnh mật khẩu – `DoManhMatKhau.cs`

Thanh đánh giá hiển thị 5 tiêu chí:

- Mật khẩu có tối thiểu 8 ký tự.
- Có chữ hoa.
- Có chữ thường.
- Có chữ số.
- Có ký tự đặc biệt.

Đây là các tiêu chí đánh giá và khuyến nghị để người dùng cải thiện mật khẩu. Không phải cả 5 tiêu chí đều là điều kiện bắt buộc để đăng ký.

## 2. Quy tắc kiểm tra mật khẩu khi đăng ký – `FrmDangKy.cs`

Logic đăng ký hiện tại yêu cầu:

- Mật khẩu dài từ 8 đến 64 ký tự.
- Có ít nhất một chữ cái.
- Có ít nhất một chữ số.

Chữ hoa, chữ thường và ký tự đặc biệt là các tiêu chí khuyến nghị để tăng độ mạnh mật khẩu, không phải điều kiện bắt buộc của biểu mẫu đăng ký.

## 3. Chức năng ghi nhớ mật khẩu – `CauHinhCucBo.cs`

Bổ sung chức năng ghi nhớ mật khẩu trên máy người dùng. Dữ liệu lưu cục bộ được bảo vệ bằng Windows DPAPI.

DPAPI là cơ chế mã hóa và bảo vệ dữ liệu của Windows, không phải phương pháp băm mật khẩu một chiều. Vì vậy, dữ liệu đã mã hóa có thể được giải mã lại trong môi trường máy tính và tài khoản Windows phù hợp.

Đây là tiện ích bổ sung phía Client, ngoài các thành phần giao diện cơ bản.

## 4. Phân định phạm vi công việc

Các chức năng nêu trên là phần bổ sung ở phía Client, được thực hiện với sự phối hợp cùng Trọng Phú để hỗ trợ giao diện và trải nghiệm người dùng.

Phần bổ sung này không thay thế, chỉnh sửa hoặc can thiệp vào phần triển khai bảo mật mật khẩu phía Server do Trọng Phú phụ trách. Phần xử lý bảo mật phía Server vẫn thuộc phạm vi công việc riêng của Trọng Phú.

Cần phân biệt rõ:

- **Thanh đánh giá độ mạnh mật khẩu:** kiểm tra các tiêu chí và đưa ra gợi ý cho người dùng.
- **Quy tắc đăng ký:** xác định điều kiện mật khẩu được biểu mẫu chấp nhận.
- **Windows DPAPI:** mã hóa và bảo vệ dữ liệu ghi nhớ mật khẩu được lưu cục bộ.
- **Bảo mật mật khẩu phía Server:** phần triển khai riêng ở Server, không bị thay thế bởi các tiện ích Client nói trên.

Ghi chú này nhằm giúp các thành viên hiểu đúng phạm vi bổ sung ở Bước 1.2 và tránh nhầm lẫn giữa tiện ích phía Client với phần bảo mật phía Server.
