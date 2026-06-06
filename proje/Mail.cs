using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace proje
{
    public class Mail
    {
        // Gmail için ayarlar
        private string smtpHost = "smtp.gmail.com";
        private int smtpPort = 587; // Port 587 (TLS) - Gmail için önerilen port
        private string stmpUsername = "emirhang9076@gmail.com";
        private string stmpPass = "xilvubiaafvjwyea"; // App Password'ü buraya yapıştırın (boşluksuz)

        public bool SendReminderEmail(string toEmail, string kullaniciAdi, DateTime rezervasyonSaati, string makineAdi)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MAIL] E-posta gönderiliyor...");
                System.Diagnostics.Debug.WriteLine($"[MAIL] Gönderen: {stmpUsername}");
                System.Diagnostics.Debug.WriteLine($"[MAIL] Alıcı: {toEmail}");
                System.Diagnostics.Debug.WriteLine($"[MAIL] App Password uzunluğu: {stmpPass?.Length ?? 0} karakter");
                
                // MimeMessage oluştur
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Çamaşırhane Sistemi", stmpUsername));
                message.To.Add(new MailboxAddress(kullaniciAdi, toEmail));
                message.Subject = "Çamaşırhane Rezervasyon Hatırlatması";
                
                // Rezervasyon saatine kalan süreyi hesapla
                TimeSpan kalanSure = rezervasyonSaati - DateTime.Now;
                string kalanSureMetni = "";
                if (kalanSure.TotalMinutes >= 30)
                {
                    int dakika = (int)kalanSure.TotalMinutes;
                    kalanSureMetni = $"Yarım saatiniz kaldı! ({dakika} dakika)";
                }
                else if (kalanSure.TotalMinutes >= 15)
                {
                    int dakika = (int)kalanSure.TotalMinutes;
                    kalanSureMetni = $"Rezervasyonunuz yaklaşıyor! ({dakika} dakika kaldı)";
                }
                else if (kalanSure.TotalMinutes >= 5)
                {
                    int dakika = (int)kalanSure.TotalMinutes;
                    kalanSureMetni = $"Rezervasyonunuz çok yakın! ({dakika} dakika kaldı)";
                }
                else
                {
                    kalanSureMetni = "Rezervasyonunuz başlamak üzere!";
                }
                
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = $"Sayın {kullaniciAdi},\n\n" +
                                      $"═══════════════════════════════════════\n" +
                                      $"   ÇAMAŞIRHANE REZERVASYON HATIRLATMASI\n" +
                                      $"═══════════════════════════════════════\n\n" +
                                      $"⏰ {kalanSureMetni}\n\n" +
                                      $"📅 Rezervasyon Detayları:\n" +
                                      $"   • Makine: {makineAdi}\n" +
                                      $"   • Tarih: {rezervasyonSaati:dd.MM.yyyy}\n" +
                                      $"   • Saat: {rezervasyonSaati:HH:mm}\n\n" +
                                      $"💡 Lütfen rezervasyon saatinde hazır olun.\n" +
                                      $"   Geç kalmanız durumunda rezervasyonunuz iptal edilebilir.\n\n" +
                                      $"═══════════════════════════════════════\n" +
                                      $"İyi günler dileriz.\n" +
                                      $"Çamaşırhane Yönetimi";
                message.Body = bodyBuilder.ToMessageBody();

                // NetworkCredential oluştururken boşlukları temizle
                string cleanPassword = stmpPass?.Trim().Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                
                System.Diagnostics.Debug.WriteLine($"[MAIL] Şifre temizlendi, uzunluk: {cleanPassword?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[MAIL] SMTP bağlantısı kuruluyor: {smtpHost}:{smtpPort}");
                
                // MailKit SmtpClient kullan
                using (var client = new SmtpClient())
                {
                    // Bağlan
                    client.Connect(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                    
                    System.Diagnostics.Debug.WriteLine($"[MAIL] Bağlantı kuruldu, kimlik doğrulama yapılıyor...");
                    
                    // Kimlik doğrula
                    client.Authenticate(stmpUsername, cleanPassword);
                    
                    System.Diagnostics.Debug.WriteLine($"[MAIL] Kimlik doğrulama başarılı, e-posta gönderiliyor...");
                    
                    // E-posta gönder
                    client.Send(message);
                    
                    System.Diagnostics.Debug.WriteLine($"[MAIL] E-posta başarıyla gönderildi!");
                    
                    // Bağlantıyı kapat
                    client.Disconnect(true);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                // Hata olursa logla (arka planda çalıştığı için MessageBox göstermiyoruz)
                System.Diagnostics.Debug.WriteLine($"[MAIL] E-posta gönderme hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[MAIL] Inner exception: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"[MAIL] Stack trace: {ex.StackTrace}");
                
                // Özel hata mesajları
                if (ex.Message.Contains("Authentication") || ex.Message.Contains("5.7.0") || ex.Message.Contains("Invalid"))
                {
                    System.Diagnostics.Debug.WriteLine($"[MAIL] ⚠️ GMAIL KİMLİK DOĞRULAMA HATASI!");
                    System.Diagnostics.Debug.WriteLine($"[MAIL] Kontrol edilecekler:");
                    System.Diagnostics.Debug.WriteLine($"[MAIL] 1. Google hesabınızda 2 Adımlı Doğrulama açık mı?");
                    System.Diagnostics.Debug.WriteLine($"[MAIL] 2. App Password doğru mu? (16 karakter, boşluksuz)");
                    System.Diagnostics.Debug.WriteLine($"[MAIL] 3. Proje yeniden derlendi mi? (Build → Rebuild Solution)");
                    System.Diagnostics.Debug.WriteLine($"[MAIL] 4. Eski .exe dosyası çalışıyor olabilir, uygulamayı kapatıp yeniden başlatın");
                    System.Diagnostics.Debug.WriteLine($"[MAIL] 5. App Password'ü silip yeniden oluşturmayı deneyin");
                }
                
                return false;
            }
        }
    }
}
