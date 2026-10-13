
IF DB_ID(N'HealthInfoDB') IS NULL
    CREATE DATABASE HealthInfoDB;
GO

USE HealthInfoDB;
GO

IF OBJECT_ID(N'dbo.Articles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Articles
    (
        ArticleId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Summary NVARCHAR(1000) NULL,
        Content NVARCHAR(MAX) NULL,
        IsPublished BIT NOT NULL DEFAULT 0
    );
END;
GO

CREATE TABLE dbo.LichSuRequest
(
    MaLichSu BIGINT IDENTITY(1,1) PRIMARY KEY,
    ThoiGianNhan DATETIME2 NOT NULL
        DEFAULT SYSDATETIME(),

    ClientIP VARCHAR(45) NOT NULL,
    LoaiRequest NVARCHAR(50) NOT NULL,
    NoiDung NVARCHAR(2000) NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM dbo.Articles)
BEGIN
    INSERT INTO dbo.Articles
        (Title, Summary, Content, IsPublished)
    VALUES
    (
        N'Đau đầu',
        N'Thông tin tổng quan về triệu chứng đau đầu.',
        N'Đau đầu có nhiều nguyên nhân khác nhau. Nếu triệu chứng kéo dài, nặng lên hoặc đáng lo ngại, hãy tìm tư vấn y tế.',
        1
    ),
    (
        N'Sốt',
        N'Thông tin tổng quan về triệu chứng sốt.',
        N'Sốt có thể xuất hiện trong nhiều tình trạng sức khỏe. Cần xem xét triệu chứng đi kèm và tìm tư vấn y tế khi cần.',
        1
    );
END;
GO

SELECT ArticleId, Title, Summary
FROM dbo.Articles
WHERE IsPublished = 1;
GO

SELECT *
FROM dbo.LichSuRequest
ORDER BY MaLichSu DESC;

INSERT INTO dbo.LichSuRequest
    (ClientIP, LoaiRequest, NoiDung)
VALUES
    ('127.0.0.1', N'TEST', N'Yêu cầu thử từ Client');


