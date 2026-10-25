# Hệ thống tra cứu triệu chứng và thông tin sức khỏe (NT106)

Client C# WinForms, Server C# TCP (cổng 5000), CSDL SQL Server.

```
src/HealthShared   Class Library dùng chung
src/HealthServer   Console - TCP Server
src/HealthClient   WinForms - giao diện
docs/              Đặc tả giao thức, sơ đồ
database/          Script SQL
```

Mở file `.sln` bằng Visual Studio (.NET 10), Build Solution phải 0 lỗi.
`main` chỉ chứa sản phẩm đồ án cuối cùng; làm việc trên nhánh `feature/*`.
