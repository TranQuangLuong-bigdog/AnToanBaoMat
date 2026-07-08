USE JobRecruitmentDB;
GO

/*============================================
1. USERS
=============================================*/

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255),
    Phone NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE()
);

GO

/*============================================
2. JOBS
=============================================*/

CREATE TABLE Jobs
(
    JobId INT IDENTITY(1,1) PRIMARY KEY,
    JobTitle NVARCHAR(200) NOT NULL,
    CompanyName NVARCHAR(200),
    Location NVARCHAR(100),
    Salary DECIMAL(18,2),
    CreatedAt DATETIME DEFAULT GETDATE()
);

GO

/*============================================
3. APPLICATIONS
=============================================*/

CREATE TABLE Applications
(
    ApplicationId INT IDENTITY(1,1) PRIMARY KEY,

    JobId INT NOT NULL,

    UserId INT NOT NULL,

    CVFile NVARCHAR(255),

    IPAddress NVARCHAR(50),

    ApplyTime DATETIME DEFAULT GETDATE(),

    Status NVARCHAR(50) DEFAULT N'Đã gửi',

    CONSTRAINT FK_Application_User
        FOREIGN KEY(UserId)
        REFERENCES Users(UserId),

    CONSTRAINT FK_Application_Job
        FOREIGN KEY(JobId)
        REFERENCES Jobs(JobId)
);

GO

/*============================================
4. BLOCKED IPS
=============================================*/

CREATE TABLE BlockedIPs
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    IPAddress NVARCHAR(50) UNIQUE,

    Reason NVARCHAR(255),

    IsActive BIT DEFAULT 1,

    CreatedAt DATETIME DEFAULT GETDATE()
);

GO

/*============================================
5. CV LOGS
=============================================*/

CREATE TABLE CVUploadLogs
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT,

    JobId INT,

    FileName NVARCHAR(255),

    IPAddress NVARCHAR(50),

    UploadTime DATETIME DEFAULT GETDATE(),

    Status NVARCHAR(50),

    Message NVARCHAR(255),

    FOREIGN KEY(UserId)
        REFERENCES Users(UserId),

    FOREIGN KEY(JobId)
        REFERENCES Jobs(JobId)
);

GO
CREATE TABLE SecuritySettings
(
    Id INT IDENTITY PRIMARY KEY,

    MaxUpload INT NOT NULL,

    TimeLimit INT NOT NULL,

    MaxFileSize INT NOT NULL,

    AllowPdf BIT DEFAULT 1,

    AllowDoc BIT DEFAULT 1,

    AllowDocx BIT DEFAULT 1,

    AutoBlock BIT DEFAULT 1,

    CreatedAt DATETIME DEFAULT GETDATE()
);
ALTER TABLE Users
ADD Role NVARCHAR(20) NOT NULL
DEFAULT 'User';
/*============================================
INDEX
=============================================*/

CREATE INDEX IX_CV_IP
ON CVUploadLogs(IPAddress);

GO

CREATE INDEX IX_CV_TIME
ON CVUploadLogs(UploadTime);

GO

/*============================================
DỮ LIỆU USERS
=============================================*/

INSERT INTO Users
(
FullName,
Email,
PasswordHash,
Phone
)
VALUES
(N'Nguyễn Văn A','a@gmail.com','123456','0900000001'),
(N'Trần Thị B','b@gmail.com','123456','0900000002'),
(N'Lê Văn C','c@gmail.com','123456','0900000003');
GO
INSERT INTO Users
(
    FullName,
    Email,
    Phone,
    PasswordHash,
    Role,
    Status
)
VALUES
(
    N'Administrator',
    'admin@gmail.com',
    '0123456789',
    '123456',
    'Admin',
    1
);
GO
INSERT INTO SecuritySettings
(
    MaxUpload,
    TimeLimit,
    MaxFileSize,
    AllowPdf,
    AllowDoc,
    AllowDocx,
    AutoBlock
)
VALUES
(
    5,
    10,
    5242880,
    1,
    1,
    1,
    1
);
/*============================================
DỮ LIỆU JOBS
=============================================*/

INSERT INTO Jobs
(
JobTitle,
CompanyName,
Location,
Salary
)
VALUES
(N'Lập trình ASP.NET MVC',N'FPT Software',N'Hà Nội',15000000),
(N'Frontend Developer',N'VNG',N'HCM',18000000),
(N'Backend Developer',N'Viettel',N'Hà Nội',20000000);

GO

/*============================================
IP BỊ CHẶN
=============================================*/

INSERT INTO BlockedIPs
(
IPAddress,
Reason
)
VALUES
('192.168.1.100',N'Spam'),
('10.10.10.10',N'Bot');

GO

/*============================================
APPLICATIONS
=============================================*/

INSERT INTO Applications
(
JobId,
UserId,
CVFile,
IPAddress
)
VALUES
(1,1,'3d0ac44e-2ffe-4d75-a292-56ab0ce82b2e.enc','113.161.20.10'),
(2,2,'196e1caf-4cc4-43ea-bc0c-33849a749f9f.enc','113.161.20.11'),
(3,3,'3a058628-f1bb-466d-8dbb-a7c7ee22e9e0.enc','113.161.20.12');

GO

/*============================================
LOG
=============================================*/

INSERT INTO CVUploadLogs
(
UserId,
JobId,
FileName,
IPAddress,
Status,
Message
)
VALUES
(1,1,'cvA.pdf','113.161.20.10',N'Thành công',N''),
(2,2,'cvB.pdf','113.161.20.11',N'Thành công',N''),
(3,3,'cvC.pdf','113.161.20.12',N'Thành công',N'');

GO

/*============================================
VIEW
=============================================*/

CREATE VIEW vw_CVUploadByIP
AS
SELECT
    IPAddress,
    COUNT(*) AS TotalUpload,
    MAX(UploadTime) AS LastUpload
FROM CVUploadLogs
GROUP BY IPAddress;

GO

CREATE TABLE SecurityEvents
(
    EventId INT IDENTITY PRIMARY KEY,

    UserId INT NULL,

    IPAddress NVARCHAR(50),

    AttackType NVARCHAR(100),

    Severity NVARCHAR(20),

    Description NVARCHAR(MAX),

    EventTime DATETIME DEFAULT GETDATE(),

    Status NVARCHAR(30)
);

/*============================================
PROC KIỂM TRA IP
=============================================*/

CREATE PROC sp_CheckBlockedIP
@IPAddress NVARCHAR(50)
AS
BEGIN

IF EXISTS
(
SELECT *
FROM BlockedIPs
WHERE IPAddress=@IPAddress
AND IsActive=1
)

SELECT 1 AS IsBlocked;

ELSE

SELECT 0 AS IsBlocked;

END

GO

/*============================================
PROC KIỂM TRA GIỚI HẠN
=============================================*/

CREATE PROC sp_CheckUploadLimit
@IPAddress NVARCHAR(50)
AS
BEGIN

DECLARE @Count INT

SELECT @Count=COUNT(*)
FROM Applications
WHERE IPAddress=@IPAddress
AND ApplyTime>=DATEADD(MINUTE,-10,GETDATE())

SELECT
@Count AS UploadCount,

CASE
WHEN @Count>=5 THEN 1
ELSE 0
END AS IsLimit

END

GO

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
SELECT * FROM Users;
SELECT * FROM Jobs;
SELECT * FROM Applications;
SELECT * FROM BlockedIPs;
SELECT * FROM CVUploadLogs;
SELECT * FROM vw_CVUploadByIP;
EXEC sp_CheckBlockedIP '113.161.20.10';
EXEC sp_CheckUploadLimit '113.161.20.10';

ALTER TABLE Users
ADD Role NVARCHAR(20) NOT NULL DEFAULT 'User';

UPDATE Users
SET Role = 'Admin'
WHERE Email = 'admin@gmail.com';

ALTER TABLE Applications
ADD

UploaderIP NVARCHAR(50),

SessionToken NVARCHAR(100),

Nonce NVARCHAR(100),

FileHash NVARCHAR(200),

ExpireTime DATETIME,

EncryptedFileName NVARCHAR(255);
SELECT name
FROM sys.databases;
SELECT TOP 1 *
FROM Applications
CREATE TABLE DownloadLogs
(
    LogId INT IDENTITY PRIMARY KEY,

    ApplicationId INT,

    EmployerId INT,

    DownloadTime DATETIME,

    IP NVARCHAR(50),

    Status NVARCHAR(50)
)
ALTER TABLE Applications
ADD OriginalFileName NVARCHAR(255);
ALTER TABLE Users
ADD SessionToken NVARCHAR(100);
UPDATE Users
SET Role='Admin'
WHERE Email='admin@gmail.com';

UPDATE Users
SET Role='Employer'
WHERE Email='employer@gmail.com';

UPDATE Users
SET Role='Candidate'
WHERE Email='user@gmail.com';
CREATE TABLE SecurityEvents
(
    EventId INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NULL,

    EventType NVARCHAR(100),

    Description NVARCHAR(500),

    IPAddress NVARCHAR(50),

    EventTime DATETIME DEFAULT(GETDATE())
);
CREATE TABLE AuditLogs
(
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NULL,

    Action NVARCHAR(100),

    Description NVARCHAR(300),

    IPAddress NVARCHAR(50),

    LogTime DATETIME DEFAULT(GETDATE()),

    Status NVARCHAR(50)
);
INSERT INTO Users
(FullName, Email, PasswordHash, Role)
VALUES
('Trần Quang Lương',
'admin@gmail.com',
'123456',
'Admin');

INSERT INTO Users
(FullName, Email, PasswordHash, Role)
VALUES
('Nguyễn Minh Quân',
'admin@jobsecure.vn',
'123456',
'Admin');

INSERT INTO Users
(FullName, Email, PasswordHash,Phone, Role)
VALUES

('Trần Quang Lương',
'luongmc0948@gmail.com',
'123456',
'0333894838',
'Employer');

('Trần Quốc Huy',
'huy.hr@fpt.vn',
'123456',
'Employer'),

('Tô Đức Thiện',
'thien1@gmail.com',
'123456',
'Employer'),


('Lê Thu Trang',
'trang.hr@viettel.vn',
'123456',
'Employer'),

('Lê Thị Vân Anh',
'vanh@gmail.com',
'123456',
'Employer'),

('Phạm Đức Anh',
'anh.hr@topcv.vn',
'123456',
'Employer');

INSERT INTO Users
(FullName, Email, PasswordHash, Role)
VALUES

('Lê Thị Vân Anh',
'vanh@gmail.com',
'123456',
'Candidate'),

('Tô Đức Thiện',
'thien@gmail.com',
'123456',
'Candidate'),

('Lê Hoàng Long',
'long@gmail.com',
'123456',
'Candidate'),

('Phạm Thị Hương',
'huong@gmail.com',
'123456',
'Candidate'),

('Đỗ Minh Khôi',
'khoi@gmail.com',
'123456',
'Candidate');

SELECT TOP 5 * FROM Users;

Select *from Applications

SELECT ApplicationId, Cvfile
FROM Applications

SELECT TOP 10 ApplicationId, Cvfile
FROM Applications
ORDER BY ApplicationId DESC;

DELETE FROM Applications;

SELECT Cvfile FROM Applications;

SELECT
ApplicationId,
Cvfile,
OriginalFileName
FROM Applications;

SELECT TOP 5
ApplicationId,
Cvfile,
OriginalFileName
FROM Applications;


SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Applications';

SELECT TOP 5
Cvfile,
OriginalFileName
FROM Applications
ORDER BY ApplicationId DESC

ALTER TABLE Applications
ADD IsEncrypted bit NOT NULL DEFAULT 0;

ALTER TABLE Applications
ADD PublicKey nvarchar(max);

ALTER TABLE Applications
ADD PrivateKey nvarchar(max);

ALTER TABLE Applications
ADD Signature nvarchar(max);

SELECT COLUMN_NAME,
       IS_NULLABLE,
       DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='Applications'

ALTER TABLE Applications
ADD IsVerified bit NOT NULL DEFAULT 0;

SELECT IsSigned
FROM Applications

ALTER TABLE Applications
ADD IsEncrypted bit NOT NULL DEFAULT 0;

ALTER TABLE Applications
ADD IsSigned bit NOT NULL DEFAULT 0;

ALTER TABLE Applications
ADD IsVerified bit NOT NULL DEFAULT 0;

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Applications';
ALTER TABLE Applications
ADD IsVerified bit NOT NULL DEFAULT 0;

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='Applications'
ORDER BY ORDINAL_POSITION;


ALTER TABLE Applications
ADD IsSigned bit NOT NULL
CONSTRAINT DF_Applications_IsSigned DEFAULT 0;

SELECT DB_NAME() AS CurrentDatabase;

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Applications';

ALTER TABLE SecurityEvents
ADD Device NVARCHAR(255) NULL;

ALTER TABLE SecurityEvents
ADD Severity NVARCHAR(20) NULL;

ALTER TABLE SecurityEvents
ADD Status NVARCHAR(30) NULL;

SELECT *
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SecurityEvents';

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SecurityEvents';

CREATE TABLE DownloadOTP
(
    OTPId INT IDENTITY PRIMARY KEY,

    UserId INT NOT NULL,

    ApplicationId INT NOT NULL,

    OTPCode NVARCHAR(6),

    OTPType NVARCHAR(20),

    CreatedTime DATETIME DEFAULT GETDATE(),

    ExpireTime DATETIME,

    IsUsed BIT DEFAULT 0
);

sp_help DownloadOTP

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='DownloadOTP'
SELECT *
FROM DownloadOtps
ORDER BY CreatedTime DESC;
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';

SELECT * FROM DownloadOTP;

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SecurityEvents';

ALTER TABLE SecurityEvents
ADD ApplicationId INT NULL;

ALTER TABLE Users
ADD LastIP NVARCHAR(100) NULL,
    LastDevice NVARCHAR(MAX) NULL,
    LastLogin DATETIME NULL;

    SELECT LastIP, LastDevice, LastLogin
FROM Users;