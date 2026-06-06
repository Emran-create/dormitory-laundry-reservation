-- Çamaşırhane Projesi Veritabanı Şeması (TÜRKÇE)
-- SQL Server için hazırlanmıştır

-- Veritabanını oluştur (isteğe bağlı)
-- CREATE DATABASE CamasirhaneDB;
-- GO
-- USE CamasirhaneDB;
-- GO

-- ÖNEMLİ: Eğer eski veritabanı varsa, önce tabloları silin:
-- DROP TABLE IF EXISTS Rezervasyonlar;
-- DROP TABLE IF EXISTS Duyurular;
-- DROP TABLE IF EXISTS Programlar;
-- DROP TABLE IF EXISTS Makineler;
-- DROP TABLE IF EXISTS Kullanicilar;

-- 1. Kullanıcılar Tablosu
CREATE TABLE Kullanicilar (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OdaNumarasi NVARCHAR(10) NOT NULL UNIQUE,
    AdSoyad NVARCHAR(100) NOT NULL,
    Eposta NVARCHAR(100),
    SifreHash NVARCHAR(255) NOT NULL,
    YoneticiMi BIT NOT NULL DEFAULT 0,
    AktifMi BIT NOT NULL DEFAULT 1,
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE()
);

-- 2. Makineler Tablosu
CREATE TABLE Makineler (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Ad NVARCHAR(50) NOT NULL,
    AktifMi BIT NOT NULL DEFAULT 1,
    BakimdaMi BIT NOT NULL DEFAULT 0,
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE()
);

-- 3. Çamaşır Programları Tablosu
CREATE TABLE Programlar (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Ad NVARCHAR(100) NOT NULL,
    SureDakika INT NOT NULL,
    SicaklikSantigrat INT,
    Aciklama NVARCHAR(255),
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE()
);

-- 4. Rezervasyonlar Tablosu
CREATE TABLE Rezervasyonlar (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    KullaniciId INT NOT NULL,
    MakineId INT NOT NULL,
    ProgramId INT NOT NULL,
    BaslangicZamani DATETIME2 NOT NULL,
    BitisZamani DATETIME2 NOT NULL,
    Durum NVARCHAR(20) NOT NULL DEFAULT 'Beklemede', -- Beklemede, Aktif, Tamamlandi, Iptal
    EmailGonderildiMi BIT NOT NULL DEFAULT 0,
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (KullaniciId) REFERENCES Kullanicilar(Id),
    FOREIGN KEY (MakineId) REFERENCES Makineler(Id),
    FOREIGN KEY (ProgramId) REFERENCES Programlar(Id)
);

-- 5. Duyurular Tablosu
CREATE TABLE Duyurular (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Baslik NVARCHAR(200) NOT NULL,
    Icerik NVARCHAR(MAX),
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE(),
    AktifMi BIT NOT NULL DEFAULT 1
);

-- Index'ler (Performans için)
CREATE INDEX IX_Rezervasyonlar_KullaniciId ON Rezervasyonlar(KullaniciId);
CREATE INDEX IX_Rezervasyonlar_MakineId ON Rezervasyonlar(MakineId);
CREATE INDEX IX_Rezervasyonlar_BaslangicZamani ON Rezervasyonlar(BaslangicZamani);
CREATE INDEX IX_Rezervasyonlar_Durum ON Rezervasyonlar(Durum);
CREATE INDEX IX_Kullanicilar_OdaNumarasi ON Kullanicilar(OdaNumarasi);
CREATE INDEX IX_Kullanicilar_AktifMi ON Kullanicilar(AktifMi);

-- Örnek Veriler

-- Admin kullanıcı
INSERT INTO Kullanicilar (OdaNumarasi, AdSoyad, Eposta, SifreHash, YoneticiMi, AktifMi) 
VALUES ('ADMIN', 'Sistem Yöneticisi', 'admin@yurt.com', 'admin123', 1, 1);

-- Öğrenci kullanıcıları
INSERT INTO Kullanicilar (OdaNumarasi, AdSoyad, Eposta, SifreHash, YoneticiMi, AktifMi) 
VALUES 
('101', 'Ahmet Yılmaz', 'ahmet@email.com', '123456', 0, 1),
('102', 'Ayşe Demir', 'ayse@email.com', '123456', 0, 1),
('103', 'Mehmet Kaya', 'mehmet@email.com', '123456', 0, 1),
('104', 'Fatma Öz', 'fatma@email.com', '123456', 0, 1),
('105', 'Ali Çelik', 'ali@email.com', '123456', 0, 1);

-- Çamaşır makineleri
INSERT INTO Makineler (Ad, AktifMi, BakimdaMi) 
VALUES 
('Makine 1', 1, 0),
('Makine 2', 1, 0),
('Makine 3', 1, 0),
('Makine 4', 1, 0),
('Makine 5', 1, 1); -- Bakımda

-- Çamaşır programları
INSERT INTO Programlar (Ad, SureDakika, SicaklikSantigrat, Aciklama) 
VALUES 
('Hızlı Yıkama', 30, 30, 'Hafif kirli çamaşırlar için'),
('Normal Yıkama', 60, 40, 'Günlük çamaşırlar için'),
('Güçlü Yıkama', 90, 60, 'Çok kirli çamaşırlar için'),
('Hassas Yıkama', 45, 30, 'İnce kumaşlar için'),
('Beyazlar', 75, 90, 'Beyaz çamaşırlar için yüksek sıcaklık');

-- Örnek rezervasyonlar
INSERT INTO Rezervasyonlar (KullaniciId, MakineId, ProgramId, BaslangicZamani, BitisZamani, Durum) 
VALUES 
(2, 1, 2, DATEADD(HOUR, 2, GETDATE()), DATEADD(HOUR, 3, GETDATE()), 'Beklemede'),
(3, 2, 1, DATEADD(HOUR, 4, GETDATE()), DATEADD(MINUTE, 30, DATEADD(HOUR, 4, GETDATE())), 'Aktif'),
(4, 3, 3, DATEADD(DAY, 1, GETDATE()), DATEADD(MINUTE, 90, DATEADD(DAY, 1, GETDATE())), 'Beklemede');

-- Örnek duyurular
INSERT INTO Duyurular (Baslik, Icerik, OlusturmaTarihi, AktifMi) 
VALUES 
('Çamaşırhane Kuralları', 'Çamaşırhaneyi temiz kullanın ve zamanında gelin.', GETDATE(), 1),
('Bakım Duyurusu', 'Makine 5 bakımda, yakında hizmete girecek.', GETDATE(), 1),
('Yeni Program', 'Hassas yıkama programı eklendi.', GETDATE(), 1);

PRINT 'Veritabanı şeması ve örnek veriler başarıyla oluşturuldu! (TÜRKÇE)';
PRINT 'Admin girişi: Oda No: ADMIN, Şifre: admin123';
PRINT 'Öğrenci girişi: Oda No: 101, Şifre: 123456';

