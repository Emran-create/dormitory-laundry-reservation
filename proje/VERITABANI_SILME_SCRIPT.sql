-- ========================================================================
-- MEVCUT VERİTABANINI SİLMEK İÇİN SCRIPT
-- ========================================================================
-- Bu script mevcut veritabanını (Türkçe veya İngilizce) siler
-- Böylece eski backup dosyanızı (.bak) restore edebilirsiniz
-- ========================================================================

-- ÖNEMLİ: Bu script veritabanını TAMAMEN SİLER!
-- Eğer veritabanında önemli veriler varsa önce backup alın!

USE master;
GO

-- Aktif bağlantıları kapat
ALTER DATABASE CamasirhaneDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Veritabanını sil


PRINT 'Veritabanı başarıyla silindi. Şimdi backup dosyanızı (.bak) restore edebilirsiniz.';
GO


