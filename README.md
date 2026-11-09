# Hệ thống tra cứu triệu chứng và thông tin sức khỏe (NT106)

Client C# WinForms, Server C# TCP (cổng 5000), CSDL SQL Server.

```
src/HealthShared   Thư viện lớp dùng chung
src/HealthServer   Máy chủ TCP chạy trên cửa sổ dòng lệnh
src/HealthClient   Ứng dụng WinForms, giao diện và thao tác của người dùng
docs/              Tài liệu giao thức và sơ đồ
database/          Tập lệnh SQL
```

Mở file `.sln` bằng Visual Studio (.NET 10), Build Solution phải 0 lỗi.
`main` chỉ chứa sản phẩm đồ án cuối cùng; làm việc trên nhánh `feature/*`.
