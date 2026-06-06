-- Reservations tablosuna EmailSent kolonu ekleme
-- Bu kolon, e-posta hatırlatmasının gönderilip gönderilmediğini takip eder

USE CamasirhaneDB;
GO

-- EmailSent kolonu yoksa ekle
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Reservations]') 
    AND name = 'EmailSent'
)
BEGIN
    ALTER TABLE Reservations
    ADD EmailSent BIT NOT NULL DEFAULT 0;
    
    PRINT 'EmailSent kolonu başarıyla eklendi!';
END
ELSE
BEGIN
    PRINT 'EmailSent kolonu zaten mevcut.';
END
GO


