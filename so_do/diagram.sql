CREATE SCHEMA `dbo`;

CREATE TABLE `dbo`.`Users` (
  `UserID` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `TenDangNhap` nvarchar(50) UNIQUE NOT NULL,
  `MatKhauBam` nvarchar(64) NOT NULL,
  `Salt` nvarchar(32) NOT NULL,
  `SoVongLap` int NOT NULL CHECK (SoVongLap >= 600000) DEFAULT 600000,
  `ThuatToanBam` varchar(30) NOT NULL CHECK (ThuatToanBam = 'PBKDF2-HMAC-SHA256') DEFAULT 'PBKDF2-HMAC-SHA256',
  `HoTen` nvarchar(100) NOT NULL,
  `Email` nvarchar(254),
  `VaiTro` varchar(20) NOT NULL CHECK (VaiTro IN ('User', 'ContentAdmin', 'SystemAdmin')) DEFAULT 'User',
  `IsActive` bit NOT NULL DEFAULT 1,
  `SoLanSai` int NOT NULL CHECK (SoLanSai >= 0) DEFAULT 0,
  `KhoaDenNgay` datetime2(3),
  `NgayTao` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
  `LanDangNhapCuoi` datetime2(3),
  `RowVersion` rowversion NOT NULL
);

CREATE TABLE `dbo`.`Categories` (
  `CategoryID` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `TenDanhMuc` nvarchar(150) UNIQUE NOT NULL,
  `MoTa` nvarchar(1000),
  `IsActive` bit NOT NULL DEFAULT 1,
  `NgayTao` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE `dbo`.`Symptoms` (
  `SymptomID` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `TenTrieuChung` nvarchar(200) UNIQUE NOT NULL,
  `MoTa` nvarchar(max),
  `Nguon` nvarchar(1000),
  `NgayTruyCap` date,
  `IsActive` bit NOT NULL DEFAULT 1,
  `NgayTao` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE `dbo`.`Diseases` (
  `DiseaseID` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `CategoryID` int NOT NULL,
  `TenBenh` nvarchar(200) UNIQUE NOT NULL,
  `MaICD` varchar(20),
  `TongQuan` nvarchar(max) NOT NULL,
  `NguyenNhan` nvarchar(max),
  `PhongNgua` nvarchar(max),
  `CanhBao` nvarchar(max),
  `MucDoPhoBien` nvarchar(30) NOT NULL CHECK (MucDoPhoBien IN (N'Chưa xác định', N'Hiếm gặp', N'Ít gặp', N'Thường gặp', N'Rất thường gặp')) DEFAULT 'Chưa xác định',
  `Nguon` nvarchar(1000),
  `NgayTruyCap` date,
  `IsPublished` bit NOT NULL DEFAULT 0,
  `NguoiCapNhat` int,
  `NgayTao` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
  `NgayCapNhat` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
  `RowVersion` rowversion NOT NULL
);

CREATE TABLE `dbo`.`DiseaseSymptoms` (
  `DiseaseID` int NOT NULL,
  `SymptomID` int NOT NULL,
  `MoTa` nvarchar(1000),
  `MucDoLienQuan` tinyint NOT NULL CHECK (MucDoLienQuan BETWEEN 1 AND 5) DEFAULT 1,
  `Nguon` nvarchar(1000),
  `NgayTruyCap` date,
  PRIMARY KEY (`DiseaseID`, `SymptomID`)
);

CREATE TABLE `dbo`.`Medicines` (
  `MedicineID` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `TenThuoc` nvarchar(200) UNIQUE NOT NULL,
  `HoatChat` nvarchar(500),
  `DangBaoChe` nvarchar(200),
  `ChiDinh` nvarchar(max),
  `ChongChiDinh` nvarchar(max),
  `TacDungPhu` nvarchar(max),
  `CanhBao` nvarchar(max),
  `CanKeDon` bit NOT NULL DEFAULT 1,
  `Nguon` nvarchar(1000),
  `NgayTruyCap` date,
  `IsActive` bit NOT NULL DEFAULT 1,
  `NgayTao` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE `dbo`.`DiseaseMedicines` (
  `DiseaseID` int NOT NULL,
  `MedicineID` int NOT NULL,
  `GhiChu` nvarchar(2000),
  `Nguon` nvarchar(1000),
  `NgayTruyCap` date,
  PRIMARY KEY (`DiseaseID`, `MedicineID`)
);

CREATE TABLE `dbo`.`HealthProfiles` (
  `HealthProfileID` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `UserID` int UNIQUE NOT NULL,
  `NgaySinh` date,
  `GioiTinh` varchar(10) CHECK (GioiTinh IN ('Nam', 'Nu', 'Khac')),
  `ChieuCaoCm` decimal(5,2) CHECK (ChieuCaoCm > 0 AND ChieuCaoCm <= 300),
  `CanNangKg` decimal(5,2) CHECK (CanNangKg > 0 AND CanNangKg <= 700),
  `DiUng` nvarchar(2000),
  `BenhNen` nvarchar(2000),
  `GhiChu` nvarchar(2000),
  `NgayCapNhat` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
  `RowVersion` rowversion NOT NULL
);

CREATE TABLE `dbo`.`SearchHistory` (
  `SearchHistoryID` bigint PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `UserID` int,
  `TuKhoa` nvarchar(500) NOT NULL,
  `DiseaseID` int,
  `SoKetQua` int NOT NULL CHECK (SoKetQua >= 0) DEFAULT 0,
  `ClientIP` varchar(45),
  `RequestID` uniqueidentifier,
  `ThoiGianTim` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE `dbo`.`AuditLogs` (
  `AuditLogID` bigint PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `UserID` int,
  `HanhDong` varchar(60) NOT NULL,
  `TenBang` nvarchar(128),
  `DoiTuongID` nvarchar(128),
  `ThanhCong` bit NOT NULL,
  `ChiTiet` nvarchar(2000),
  `ClientIP` varchar(45),
  `RequestID` uniqueidentifier,
  `ThoiGian` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE `dbo`.`Sessions` (
  `SessionID` uniqueidentifier PRIMARY KEY NOT NULL DEFAULT (NEWID()),
  `UserID` int NOT NULL,
  `TokenHash` binary(32) UNIQUE NOT NULL,
  `TaoLuc` datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
  `HetHanLuc` datetime2(3) NOT NULL,
  `ThuHoiLuc` datetime2(3),
  `ClientIP` varchar(45),
  `UserAgent` nvarchar(500),
  CHECK (HetHanLuc > TaoLuc),
  CHECK (ThuHoiLuc IS NULL OR ThuHoiLuc >= TaoLuc)
);

ALTER TABLE `dbo`.`Diseases` ADD FOREIGN KEY (`CategoryID`) REFERENCES `dbo`.`Categories` (`CategoryID`);

ALTER TABLE `dbo`.`Diseases` ADD FOREIGN KEY (`NguoiCapNhat`) REFERENCES `dbo`.`Users` (`UserID`);

ALTER TABLE `dbo`.`DiseaseSymptoms` ADD FOREIGN KEY (`DiseaseID`) REFERENCES `dbo`.`Diseases` (`DiseaseID`);

ALTER TABLE `dbo`.`DiseaseSymptoms` ADD FOREIGN KEY (`SymptomID`) REFERENCES `dbo`.`Symptoms` (`SymptomID`);

ALTER TABLE `dbo`.`DiseaseMedicines` ADD FOREIGN KEY (`DiseaseID`) REFERENCES `dbo`.`Diseases` (`DiseaseID`);

ALTER TABLE `dbo`.`DiseaseMedicines` ADD FOREIGN KEY (`MedicineID`) REFERENCES `dbo`.`Medicines` (`MedicineID`);

ALTER TABLE `dbo`.`HealthProfiles` ADD FOREIGN KEY (`UserID`) REFERENCES `dbo`.`Users` (`UserID`);

ALTER TABLE `dbo`.`SearchHistory` ADD FOREIGN KEY (`UserID`) REFERENCES `dbo`.`Users` (`UserID`);

ALTER TABLE `dbo`.`SearchHistory` ADD FOREIGN KEY (`DiseaseID`) REFERENCES `dbo`.`Diseases` (`DiseaseID`);

ALTER TABLE `dbo`.`AuditLogs` ADD FOREIGN KEY (`UserID`) REFERENCES `dbo`.`Users` (`UserID`);

ALTER TABLE `dbo`.`Sessions` ADD FOREIGN KEY (`UserID`) REFERENCES `dbo`.`Users` (`UserID`);
