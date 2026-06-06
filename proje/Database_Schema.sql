-- Çamaşırhane Projesi Veritabanı Şeması
-- SQL Server için hazırlanmıştır

-- Veritabanını oluştur (isteğe bağlı)
-- CREATE DATABASE CamasirhaneDB;
-- GO
-- USE CamasirhaneDB;
-- GO

-- 1. Kullanıcılar Tablosu
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber NVARCHAR(10) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    PasswordHash NVARCHAR(255) NOT NULL,
    IsAdmin BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- 2. Makineler Tablosu
CREATE TABLE Machines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsUnderMaintenance BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- 3. Çamaşır Programları Tablosu
CREATE TABLE LaundryPrograms (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    DurationMinutes INT NOT NULL,
    TemperatureCelsius INT,
    Description NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- 4. Rezervasyonlar Tablosu
CREATE TABLE Reservations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    MachineId INT NOT NULL,
    ProgramId INT NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Active, Completed, Cancelled
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (MachineId) REFERENCES Machines(Id),
    FOREIGN KEY (ProgramId) REFERENCES LaundryPrograms(Id)
);

-- 5. Duyurular Tablosu
CREATE TABLE Announcements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Index'ler (Performans için)
CREATE INDEX IX_Reservations_UserId ON Reservations(UserId);
CREATE INDEX IX_Reservations_MachineId ON Reservations(MachineId);
CREATE INDEX IX_Reservations_StartTime ON Reservations(StartTime);
CREATE INDEX IX_Reservations_Status ON Reservations(Status);
CREATE INDEX IX_Users_RoomNumber ON Users(RoomNumber);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

-- Örnek Veriler

-- Admin kullanıcı
INSERT INTO Users (RoomNumber, FullName, Email, PasswordHash, IsAdmin, IsActive) 
VALUES ('ADMIN', 'Sistem Yöneticisi', 'admin@yurt.com', 'admin123', 1, 1);

-- Öğrenci kullanıcıları
INSERT INTO Users (RoomNumber, FullName, Email, PasswordHash, IsAdmin, IsActive) 
VALUES 
('101', 'Ahmet Yılmaz', 'ahmet@email.com', '123456', 0, 1),
('102', 'Ayşe Demir', 'ayse@email.com', '123456', 0, 1),
('103', 'Mehmet Kaya', 'mehmet@email.com', '123456', 0, 1),
('104', 'Fatma Öz', 'fatma@email.com', '123456', 0, 1),
('105', 'Ali Çelik', 'ali@email.com', '123456', 0, 1);

-- Çamaşır makineleri
INSERT INTO Machines (Name, IsActive, IsUnderMaintenance) 
VALUES 
('Makine 1', 1, 0),
('Makine 2', 1, 0),
('Makine 3', 1, 0),
('Makine 4', 1, 0),
('Makine 5', 1, 1); -- Bakımda

-- Çamaşır programları
INSERT INTO LaundryPrograms (Name, DurationMinutes, TemperatureCelsius, Description) 
VALUES 
('Hızlı Yıkama', 30, 30, 'Hafif kirli çamaşırlar için'),
('Normal Yıkama', 60, 40, 'Günlük çamaşırlar için'),
('Güçlü Yıkama', 90, 60, 'Çok kirli çamaşırlar için'),
('Hassas Yıkama', 45, 30, 'İnce kumaşlar için'),
('Beyazlar', 75, 90, 'Beyaz çamaşırlar için yüksek sıcaklık');

-- Örnek rezervasyonlar
INSERT INTO Reservations (UserId, MachineId, ProgramId, StartTime, EndTime, Status) 
VALUES 
(2, 1, 2, DATEADD(HOUR, 2, GETDATE()), DATEADD(HOUR, 3, GETDATE()), 'Pending'),
(3, 2, 1, DATEADD(HOUR, 4, GETDATE()), DATEADD(MINUTE, 30, DATEADD(HOUR, 4, GETDATE())), 'Active'),
(4, 3, 3, DATEADD(DAY, 1, GETDATE()), DATEADD(MINUTE, 90, DATEADD(DAY, 1, GETDATE())), 'Pending');

-- Örnek duyurular
INSERT INTO Announcements (Title, Content, CreatedAt, IsActive) 
VALUES 
('Çamaşırhane Kuralları', 'Çamaşırhaneyi temiz kullanın ve zamanında gelin.', GETDATE(), 1),
('Bakım Duyurusu', 'Makine 5 bakımda, yakında hizmete girecek.', GETDATE(), 1),
('Yeni Program', 'Hassas yıkama programı eklendi.', GETDATE(), 1);

PRINT 'Veritabanı şeması ve örnek veriler başarıyla oluşturuldu!';
PRINT 'Admin girişi: Oda No: ADMIN, Şifre: admin123';
PRINT 'Öğrenci girişi: Oda No: 101, Şifre: 123456';
