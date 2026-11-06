
USE master;
GO
IF DB_ID(N'HealthInfoDB') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE HealthInfoDB;');
    ALTER DATABASE HealthInfoDB SET RECOVERY SIMPLE;
END;
GO
USE HealthInfoDB;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'HealthInfoDB'
    THROW 51000, N'Sai CSDL. Dừng để tránh tạo đối tượng nhầm nơi.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Users', N'SoLanSai') IS NULL
        EXEC(N'ALTER TABLE dbo.Users ADD SoLanSai int NOT NULL CHECK (SoLanSai >= 0) CONSTRAINT DF_Users_SoLanSai_Upgrade DEFAULT 0 WITH VALUES;');
    IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Users', N'KhoaDenNgay') IS NULL
        EXEC(N'ALTER TABLE dbo.Users ADD KhoaDenNgay datetime2(3) NULL;');
    IF OBJECT_ID(N'dbo.Diseases', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Diseases', N'MucDoPhoBien') IS NULL
        EXEC(N'ALTER TABLE dbo.Diseases ADD MucDoPhoBien nvarchar(30) NOT NULL DEFAULT N''Chưa xác định'' CHECK (MucDoPhoBien IN (N''Chưa xác định'', N''Hiếm gặp'', N''Ít gặp'', N''Thường gặp'', N''Rất thường gặp''));');
    IF OBJECT_ID(N'dbo.Diseases', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Diseases', N'Nguon') IS NULL
        EXEC(N'ALTER TABLE dbo.Diseases ADD Nguon nvarchar(1000) NULL;');
    IF OBJECT_ID(N'dbo.Diseases', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Diseases', N'NgayTruyCap') IS NULL
        EXEC(N'ALTER TABLE dbo.Diseases ADD NgayTruyCap date NULL;');
    IF OBJECT_ID(N'dbo.Symptoms', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Symptoms', N'Nguon') IS NULL
        EXEC(N'ALTER TABLE dbo.Symptoms ADD Nguon nvarchar(1000) NULL;');
    IF OBJECT_ID(N'dbo.Symptoms', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Symptoms', N'NgayTruyCap') IS NULL
        EXEC(N'ALTER TABLE dbo.Symptoms ADD NgayTruyCap date NULL;');
    IF OBJECT_ID(N'dbo.Medicines', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Medicines', N'Nguon') IS NULL
        EXEC(N'ALTER TABLE dbo.Medicines ADD Nguon nvarchar(1000) NULL;');
    IF OBJECT_ID(N'dbo.Medicines', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Medicines', N'NgayTruyCap') IS NULL
        EXEC(N'ALTER TABLE dbo.Medicines ADD NgayTruyCap date NULL;');
    DECLARE @Expected TABLE (TableName sysname, ColumnName sysname, TypeName sysname);
 --khai bao dang @expected de co the khai bao chung nhieu bang 
    INSERT @Expected VALUES
        (N'Users', N'UserID', N'int'),
        (N'Users', N'TenDangNhap', N'nvarchar'),
        (N'Users', N'MatKhauBam', N'nvarchar'),
        (N'Users', N'Salt', N'nvarchar'),
        (N'Users', N'SoVongLap', N'int'),
        (N'Users', N'ThuatToanBam', N'varchar'),
        (N'Users', N'HoTen', N'nvarchar'),
        (N'Users', N'Email', N'nvarchar'),
        (N'Users', N'VaiTro', N'varchar'),
        (N'Users', N'IsActive', N'bit'),
        (N'Users', N'SoLanSai', N'int'),
        (N'Users', N'KhoaDenNgay', N'datetime2'),
        (N'Users', N'NgayTao', N'datetime2'),
        (N'Users', N'LanDangNhapCuoi', N'datetime2'),
        (N'Users', N'RowVersion', N'timestamp'),
        (N'Categories', N'CategoryID', N'int'),
        (N'Categories', N'TenDanhMuc', N'nvarchar'),
        (N'Categories', N'MoTa', N'nvarchar'),
        (N'Categories', N'IsActive', N'bit'),
        (N'Categories', N'NgayTao', N'datetime2'),
        (N'Symptoms', N'SymptomID', N'int'),
        (N'Symptoms', N'TenTrieuChung', N'nvarchar'),
        (N'Symptoms', N'MoTa', N'nvarchar'),
        (N'Symptoms', N'Nguon', N'nvarchar'),
        (N'Symptoms', N'NgayTruyCap', N'date'),
        (N'Symptoms', N'IsActive', N'bit'),
        (N'Symptoms', N'NgayTao', N'datetime2'),
        (N'Diseases', N'DiseaseID', N'int'),
        (N'Diseases', N'CategoryID', N'int'),
        (N'Diseases', N'TenBenh', N'nvarchar'),
        (N'Diseases', N'MaICD', N'varchar'),
        (N'Diseases', N'TongQuan', N'nvarchar'),
        (N'Diseases', N'NguyenNhan', N'nvarchar'),
        (N'Diseases', N'PhongNgua', N'nvarchar'),
        (N'Diseases', N'CanhBao', N'nvarchar'),
        (N'Diseases', N'MucDoPhoBien', N'nvarchar'),
        (N'Diseases', N'Nguon', N'nvarchar'),
        (N'Diseases', N'NgayTruyCap', N'date'),
        (N'Diseases', N'IsPublished', N'bit'),
        (N'Diseases', N'NguoiCapNhat', N'int'),
        (N'Diseases', N'NgayTao', N'datetime2'),
        (N'Diseases', N'NgayCapNhat', N'datetime2'),
        (N'Diseases', N'RowVersion', N'timestamp'),
        (N'DiseaseSymptoms', N'DiseaseID', N'int'),
        (N'DiseaseSymptoms', N'SymptomID', N'int'),
        (N'DiseaseSymptoms', N'MoTa', N'nvarchar'),
        (N'DiseaseSymptoms', N'MucDoLienQuan', N'tinyint'),
        (N'DiseaseSymptoms', N'Nguon', N'nvarchar'),
        (N'DiseaseSymptoms', N'NgayTruyCap', N'date'),
        (N'Medicines', N'MedicineID', N'int'),
        (N'Medicines', N'TenThuoc', N'nvarchar'),
        (N'Medicines', N'HoatChat', N'nvarchar'),
        (N'Medicines', N'DangBaoChe', N'nvarchar'),
        (N'Medicines', N'ChiDinh', N'nvarchar'),
        (N'Medicines', N'ChongChiDinh', N'nvarchar'),
        (N'Medicines', N'TacDungPhu', N'nvarchar'),
        (N'Medicines', N'CanhBao', N'nvarchar'),
        (N'Medicines', N'CanKeDon', N'bit'),
        (N'Medicines', N'Nguon', N'nvarchar'),
        (N'Medicines', N'NgayTruyCap', N'date'),
        (N'Medicines', N'IsActive', N'bit'),
        (N'Medicines', N'NgayTao', N'datetime2'),
        (N'DiseaseMedicines', N'DiseaseID', N'int'),
        (N'DiseaseMedicines', N'MedicineID', N'int'),
        (N'DiseaseMedicines', N'GhiChu', N'nvarchar'),
        (N'DiseaseMedicines', N'Nguon', N'nvarchar'),
        (N'DiseaseMedicines', N'NgayTruyCap', N'date'),
        (N'HealthProfiles', N'HealthProfileID', N'int'),
        (N'HealthProfiles', N'UserID', N'int'),
        (N'HealthProfiles', N'NgaySinh', N'date'),
        (N'HealthProfiles', N'GioiTinh', N'varchar'),
        (N'HealthProfiles', N'ChieuCaoCm', N'decimal'),
        (N'HealthProfiles', N'CanNangKg', N'decimal'),
        (N'HealthProfiles', N'DiUng', N'nvarchar'),
        (N'HealthProfiles', N'BenhNen', N'nvarchar'),
        (N'HealthProfiles', N'GhiChu', N'nvarchar'),
        (N'HealthProfiles', N'NgayCapNhat', N'datetime2'),
        (N'HealthProfiles', N'RowVersion', N'timestamp'),
        (N'SearchHistory', N'SearchHistoryID', N'bigint'),
        (N'SearchHistory', N'UserID', N'int'),
        (N'SearchHistory', N'TuKhoa', N'nvarchar'),
        (N'SearchHistory', N'DiseaseID', N'int'),
        (N'SearchHistory', N'SoKetQua', N'int'),
        (N'SearchHistory', N'ClientIP', N'varchar'),
        (N'SearchHistory', N'RequestID', N'uniqueidentifier'),
        (N'SearchHistory', N'ThoiGianTim', N'datetime2'),
        (N'AuditLogs', N'AuditLogID', N'bigint'),
        (N'AuditLogs', N'UserID', N'int'),
        (N'AuditLogs', N'HanhDong', N'varchar'),
        (N'AuditLogs', N'TenBang', N'nvarchar'),
        (N'AuditLogs', N'DoiTuongID', N'nvarchar'),
        (N'AuditLogs', N'ThanhCong', N'bit'),
        (N'AuditLogs', N'ChiTiet', N'nvarchar'),
        (N'AuditLogs', N'ClientIP', N'varchar'),
        (N'AuditLogs', N'RequestID', N'uniqueidentifier'),
        (N'AuditLogs', N'ThoiGian', N'datetime2'),
        (N'Sessions', N'SessionID', N'uniqueidentifier'),
        (N'Sessions', N'UserID', N'int'),
        (N'Sessions', N'TokenHash', N'binary'),
        (N'Sessions', N'TaoLuc', N'datetime2'),
        (N'Sessions', N'HetHanLuc', N'datetime2'),
        (N'Sessions', N'ThuHoiLuc', N'datetime2'),
        (N'Sessions', N'ClientIP', N'varchar'),
        (N'Sessions', N'UserAgent', N'nvarchar');
    --khai bao bien chua thong bao loi neu nhu cot hoac cau truy van khong phu hop se gan thong bao vao bien
    DECLARE @Mismatch nvarchar(2048);
        -- gan vao bien thong bao loi 
    SELECT TOP (1) @Mismatch = N'Schema không tương thích: dbo.' + e.TableName
        + N'.' + e.ColumnName + N' (cần kiểu ' + e.TypeName
        + N'). Hãy tạo migration theo CSDL thực tế; không xóa bảng.'
    --kiem tra data cac bang da ton tai trong csdl
    FROM @Expected AS e
    WHERE OBJECT_ID(N'dbo.' + e.TableName, N'U') IS NOT NULL
    --tim cot bi thieu hoac sai 
      AND NOT EXISTS (
        SELECT 1 FROM sys.columns AS c
        JOIN sys.types AS ty ON c.system_type_id = ty.user_type_id
        WHERE c.object_id = OBJECT_ID(N'dbo.' + e.TableName)
          AND c.name = e.ColumnName AND ty.name = e.TypeName
      );
    IF @Mismatch IS NOT NULL THROW 51001, @Mismatch, 1;
    -- tao bang du lieu chua thong tin va quan ly thong tin cua Users 
    -- kiem tra bang user da ton tai hay chua 
    -- neu gia tri cua object_id tra ve la null tuc chua co bang va thuc hien lenh tao bang 
    IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Users (
        UserID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenDangNhap nvarchar(50) NOT NULL UNIQUE,
        MatKhauBam nvarchar(64) NOT NULL,
        Salt nvarchar(32) NOT NULL,
        SoVongLap int NOT NULL DEFAULT 600000 CHECK (SoVongLap >= 600000),
        ThuatToanBam varchar(30) NOT NULL DEFAULT 'PBKDF2-HMAC-SHA256' CHECK (ThuatToanBam = 'PBKDF2-HMAC-SHA256'),
        HoTen nvarchar(100) NOT NULL,
        Email nvarchar(254) NULL,
        VaiTro varchar(20) NOT NULL DEFAULT 'User' CHECK (VaiTro IN ('User','ContentAdmin','SystemAdmin')),
        IsActive bit NOT NULL DEFAULT 1,
        SoLanSai int NOT NULL DEFAULT 0 CHECK (SoLanSai >= 0),
        KhoaDenNgay datetime2(3) NULL,
        NgayTao datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        LanDangNhapCuoi datetime2(3) NULL,
        RowVersion rowversion NOT NULL
        );
    END;
    IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Categories (
        CategoryID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenDanhMuc nvarchar(150) NOT NULL UNIQUE,
        MoTa nvarchar(1000) NULL,
        IsActive bit NOT NULL DEFAULT 1,
        NgayTao datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME()
        );
    END;
    IF OBJECT_ID(N'dbo.Symptoms', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Symptoms (
        SymptomID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenTrieuChung nvarchar(200) NOT NULL UNIQUE,
        MoTa nvarchar(max) NULL,
        Nguon nvarchar(1000) NULL,
        NgayTruyCap date NULL,
        IsActive bit NOT NULL DEFAULT 1,
        NgayTao datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME()
        );
    END;
    IF OBJECT_ID(N'dbo.Diseases', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Diseases (
        DiseaseID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CategoryID int NOT NULL,
        TenBenh nvarchar(200) NOT NULL UNIQUE,
        MaICD varchar(20) NULL,
        TongQuan nvarchar(max) NOT NULL,
        NguyenNhan nvarchar(max) NULL,
        PhongNgua nvarchar(max) NULL,
        CanhBao nvarchar(max) NULL,
        MucDoPhoBien nvarchar(30) NOT NULL DEFAULT N'Chưa xác định' CHECK (MucDoPhoBien IN (N'Chưa xác định', N'Hiếm gặp', N'Ít gặp', N'Thường gặp', N'Rất thường gặp')),
        Nguon nvarchar(1000) NULL,
        NgayTruyCap date NULL,
        IsPublished bit NOT NULL DEFAULT 0,
        NguoiCapNhat int NULL,
        NgayTao datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        NgayCapNhat datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        RowVersion rowversion NOT NULL,
        FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID),
        FOREIGN KEY (NguoiCapNhat) REFERENCES dbo.Users(UserID)
        );
    END;

    IF OBJECT_ID(N'dbo.DiseaseSymptoms', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DiseaseSymptoms (
        DiseaseID int NOT NULL,
        SymptomID int NOT NULL,
        MoTa nvarchar(1000) NULL,
        MucDoLienQuan tinyint NOT NULL DEFAULT 1 CHECK (MucDoLienQuan BETWEEN 1 AND 5),
        Nguon nvarchar(1000) NULL,
        NgayTruyCap date NULL,
        PRIMARY KEY (DiseaseID, SymptomID),
        FOREIGN KEY (DiseaseID) REFERENCES dbo.Diseases(DiseaseID),
        FOREIGN KEY (SymptomID) REFERENCES dbo.Symptoms(SymptomID)
        );
    END;

    IF OBJECT_ID(N'dbo.Medicines', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Medicines (
        MedicineID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenThuoc nvarchar(200) NOT NULL UNIQUE,
        HoatChat nvarchar(500) NULL,
        DangBaoChe nvarchar(200) NULL,
        ChiDinh nvarchar(max) NULL,
        ChongChiDinh nvarchar(max) NULL,
        TacDungPhu nvarchar(max) NULL,
        CanhBao nvarchar(max) NULL,
        CanKeDon bit NOT NULL DEFAULT 1,
        Nguon nvarchar(1000) NULL,
        NgayTruyCap date NULL,
        IsActive bit NOT NULL DEFAULT 1,
        NgayTao datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME()
        );
    END;

    IF OBJECT_ID(N'dbo.DiseaseMedicines', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DiseaseMedicines (
        DiseaseID int NOT NULL,
        MedicineID int NOT NULL,
        GhiChu nvarchar(2000) NULL,
        Nguon nvarchar(1000) NULL,
        NgayTruyCap date NULL,
        PRIMARY KEY (DiseaseID, MedicineID),
        FOREIGN KEY (DiseaseID) REFERENCES dbo.Diseases(DiseaseID),
        FOREIGN KEY (MedicineID) REFERENCES dbo.Medicines(MedicineID)
        );
    END;

    IF OBJECT_ID(N'dbo.HealthProfiles', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.HealthProfiles (
        HealthProfileID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID int NOT NULL UNIQUE,
        NgaySinh date NULL,
        GioiTinh varchar(10) NULL CHECK (GioiTinh IN ('Nam','Nu','Khac')),
        ChieuCaoCm decimal(5,2) NULL CHECK (ChieuCaoCm > 0 AND ChieuCaoCm <= 300),
        CanNangKg decimal(5,2) NULL CHECK (CanNangKg > 0 AND CanNangKg <= 700),
        DiUng nvarchar(2000) NULL,
        BenhNen nvarchar(2000) NULL,
        GhiChu nvarchar(2000) NULL,
        NgayCapNhat datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        RowVersion rowversion NOT NULL,
        FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
        );
    END;

    IF OBJECT_ID(N'dbo.SearchHistory', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SearchHistory (
        SearchHistoryID bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID int NULL,
        TuKhoa nvarchar(500) NOT NULL,
        DiseaseID int NULL,
        SoKetQua int NOT NULL DEFAULT 0 CHECK (SoKetQua >= 0),
        ClientIP varchar(45) NULL,
        RequestID uniqueidentifier NULL,
        ThoiGianTim datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        FOREIGN KEY (DiseaseID) REFERENCES dbo.Diseases(DiseaseID)
        );
    END;

    IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.AuditLogs (
        AuditLogID bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID int NULL,
        HanhDong varchar(60) NOT NULL,
        TenBang sysname NULL,
        DoiTuongID nvarchar(128) NULL,
        ThanhCong bit NOT NULL,
        ChiTiet nvarchar(2000) NULL,
        ClientIP varchar(45) NULL,
        RequestID uniqueidentifier NULL,
        ThoiGian datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
        );
    END;

    IF OBJECT_ID(N'dbo.Sessions', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Sessions (
        SessionID uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
        UserID int NOT NULL,
        TokenHash binary(32) NOT NULL UNIQUE,
        TaoLuc datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        HetHanLuc datetime2(3) NOT NULL,
        ThuHoiLuc datetime2(3) NULL,
        ClientIP varchar(45) NULL,
        UserAgent nvarchar(500) NULL,
        FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CHECK (HetHanLuc > TaoLuc),
        CHECK (ThuHoiLuc IS NULL OR ThuHoiLuc >= TaoLuc)
        );
    END;
    --ta dung not exists kiem tra if cos tra ve du lieu ko neu ko co thi tao
    -- tra ve 1 neu moi dong thoai man dieu kien
    -- kiem tra indexes dung bang
    -- tim theo ten 
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'UX_Users_Email')
        EXEC(N'CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users (Email) WHERE Email IS NOT NULL;');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Diseases') AND name = N'IX_Diseases_Category')
        EXEC(N'CREATE INDEX IX_Diseases_Category ON dbo.Diseases (CategoryID, IsPublished);');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.DiseaseSymptoms') AND name = N'IX_DiseaseSymptoms_Symptom')
        EXEC(N'CREATE INDEX IX_DiseaseSymptoms_Symptom ON dbo.DiseaseSymptoms (SymptomID, DiseaseID);');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.DiseaseMedicines') AND name = N'IX_DiseaseMedicines_Medicine')
        EXEC(N'CREATE INDEX IX_DiseaseMedicines_Medicine ON dbo.DiseaseMedicines (MedicineID, DiseaseID);');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SearchHistory') AND name = N'IX_SearchHistory_UserTime')
        EXEC(N'CREATE INDEX IX_SearchHistory_UserTime ON dbo.SearchHistory (UserID, ThoiGianTim DESC);');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SearchHistory') AND name = N'IX_SearchHistory_Disease')
        EXEC(N'CREATE INDEX IX_SearchHistory_Disease ON dbo.SearchHistory (DiseaseID);');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.AuditLogs') AND name = N'IX_AuditLogs_Time')
        EXEC(N'CREATE INDEX IX_AuditLogs_Time ON dbo.AuditLogs (ThoiGian DESC);');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.AuditLogs') AND name = N'IX_AuditLogs_UserTime')
        EXEC(N'CREATE INDEX IX_AuditLogs_UserTime ON dbo.AuditLogs (UserID, ThoiGian DESC);');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sessions') AND name = N'IX_Sessions_UserExpiry')
        EXEC(N'CREATE INDEX IX_Sessions_UserExpiry ON dbo.Sessions (UserID, HetHanLuc) WHERE ThuHoiLuc IS NULL;');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sessions') AND name = N'IX_Sessions_Expiry')
        EXEC(N'CREATE INDEX IX_Sessions_Expiry ON dbo.Sessions (HetHanLuc);');

    EXEC(N'CREATE OR ALTER PROCEDURE dbo.usp_RecordLoginResult
    @UserID int,
    @PasswordValid bit,
    @ClientIP varchar(45) = NULL,
    @RequestID uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @PasswordValid IS NULL THROW 51010, N''PasswordValid không được NULL.'', 1;
    DECLARE @Now datetime2(3) = SYSUTCDATETIME(), @Failed int,
            @LockUntil datetime2(3), @Active bit, @Ok bit = 0;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @Failed = SoLanSai, @LockUntil = KhoaDenNgay, @Active = IsActive
        FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE UserID = @UserID;
        IF @Active IS NULL THROW 51011, N''Tài khoản không hợp lệ.'', 1;
        IF @Active = 1 AND (@LockUntil IS NULL OR @LockUntil <= @Now)
        BEGIN
            -- Khóa hết hạn: bắt đầu chu kỳ đếm mới. Sau 5 lần sai khóa 15 phút.
            SET @Failed = CASE WHEN @LockUntil IS NOT NULL THEN 0 ELSE @Failed END;
            IF @PasswordValid = 1
            BEGIN
                UPDATE dbo.Users SET SoLanSai = 0, KhoaDenNgay = NULL,
                    LanDangNhapCuoi = @Now WHERE UserID = @UserID;
                SET @Ok = 1;
            END
            ELSE
            BEGIN
                SET @Failed = CASE WHEN @Failed >= 5 THEN 5 ELSE @Failed + 1 END;
                UPDATE dbo.Users SET SoLanSai = @Failed,
                    KhoaDenNgay = CASE WHEN @Failed >= 5 THEN DATEADD(MINUTE, 15, @Now) ELSE NULL END
                WHERE UserID = @UserID;
            END;
        END;
        INSERT dbo.AuditLogs(UserID, HanhDong, ThanhCong, ClientIP, RequestID)
        VALUES (@UserID, ''LOGIN'', @Ok, @ClientIP, @RequestID);
        COMMIT;
        SELECT @Ok AS DangNhapThanhCong;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH;
END;');

    EXEC(N'CREATE OR ALTER PROCEDURE dbo.usp_BackupHealthInfoDB
    @BackupDirectory nvarchar(2000),
    @CertificateName sysname = NULL,
    @CopyOnly bit = 0,
    @AllowUnencrypted bit = 0
AS
BEGIN
    SET NOCOUNT ON;
    -- Không EXECUTE AS OWNER, không tự cấp quyền server cho tài khoản ứng dụng.
    -- Chạy bằng tài khoản vận hành có quyền BACKUP, VERIFYONLY và xem certificate.
    IF @@TRANCOUNT <> 0 THROW 51020, N''BACKUP phải chạy ngoài transaction.'', 1;
    IF @CopyOnly IS NULL OR @AllowUnencrypted IS NULL
        THROW 51021, N''Tham số bit không được NULL.'', 1;
    IF NULLIF(LTRIM(RTRIM(@BackupDirectory)), N'''') IS NULL
        THROW 51022, N''Cần thư mục backup có sẵn trên máy SQL Server.'', 1;
    IF @CertificateName IS NULL AND @AllowUnencrypted = 0
        THROW 51023, N''Cần certificate mã hóa hoặc chủ động chọn backup trên ổ đã mã hóa.'', 1;
    IF @CertificateName IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM master.sys.certificates
        WHERE name = @CertificateName
          AND pvt_key_encryption_type <> ''NA''
          AND expiry_date > GETUTCDATE()
          AND pvt_key_last_backup_date IS NOT NULL
    )
        THROW 51024, N''Certificate thiếu, hết hạn, thiếu private key hoặc chưa export private key.'', 1;

    DECLARE @Now datetime2(3) = SYSUTCDATETIME();
    DECLARE @Sep nchar(1) = CASE WHEN CHARINDEX(N''/'', @BackupDirectory) > 0 THEN N''/'' ELSE N''\'' END;
    DECLARE @File nvarchar(4000) = @BackupDirectory
        + CASE WHEN RIGHT(@BackupDirectory,1) IN (N''/'',N''\'') THEN N'''' ELSE @Sep END
        + N''HealthInfoDB_'' + CONVERT(nvarchar(8), @Now, 112) + N''_''
        + REPLACE(CONVERT(nvarchar(12), @Now, 114), N'':'', N'''') + N''_''
        + REPLACE(CONVERT(nvarchar(36), NEWID()), N''-'', N'''') + N''.bak'';
    DECLARE @SQL nvarchar(max) = N''BACKUP DATABASE [HealthInfoDB] TO DISK = @p
        WITH CHECKSUM, STOP_ON_ERROR, NOINIT, STATS = 10''
        + CASE WHEN @CopyOnly = 1 THEN N'', COPY_ONLY'' ELSE N'''' END
        + CASE WHEN @CertificateName IS NOT NULL THEN
            N'', ENCRYPTION (ALGORITHM = AES_256, SERVER CERTIFICATE = ''
            + QUOTENAME(@CertificateName) + N'')'' ELSE N'''' END + N'';'';
    BEGIN TRY
        EXEC sys.sp_executesql @SQL, N''@p nvarchar(4000)'', @p = @File;
        RESTORE VERIFYONLY FROM DISK = @File WITH FILE = 1, CHECKSUM, STOP_ON_ERROR;
        INSERT dbo.AuditLogs(HanhDong, ThanhCong, ChiTiet)
        VALUES (''BACKUP_VERIFY'', 1, LEFT(@File, 2000));
        SELECT @File AS BackupFile, @Now AS StartedAtUtc, CAST(1 AS bit) AS Verified;
    END TRY
    BEGIN CATCH
        DECLARE @Error nvarchar(2000) = LEFT(ERROR_MESSAGE(), 2000);
        -- Không để lỗi ghi log che mất lỗi backup gốc.
        BEGIN TRY
            INSERT dbo.AuditLogs(HanhDong, ThanhCong, ChiTiet)
            VALUES (''BACKUP_VERIFY'', 0, @Error);
        END TRY
        BEGIN CATCH
            PRINT N''Không ghi được AuditLogs; kiểm tra SQL Server error log/job output.'';
        END CATCH;
        THROW;
    END CATCH;
END;');

    COMMIT;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    THROW;
END CATCH;
GO
