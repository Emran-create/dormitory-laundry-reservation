using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using CamasirhaneProje.Models;

namespace proje
{
    public partial class GirisForm : Form
    {
        private System.Timers.Timer reminderTimer;

        public GirisForm()
        {
            InitializeComponent();
            // Hide(); // Test için formu gösteriyoruz
            
            // E-posta hatırlatma timer'ını başlat
            reminderTimer = new System.Timers.Timer(60000); // 60 saniye = 1 dakika
            reminderTimer.Elapsed += ReminderTimer_Elapsed;
            reminderTimer.AutoReset = true; // Timer sürekli çalışsın
            reminderTimer.Start();
            
            // Test için: İlk kontrolü hemen yap
            System.Diagnostics.Debug.WriteLine("[FORM] Form açıldı, timer başlatıldı");
            
            // TEST İÇİN: Timer'ı 10 saniyeye düşür (hızlı test için)
            // reminderTimer.Interval = 10000; // 10 saniye
            // reminderTimer.Start();
            
            // TEST İÇİN: Manuel kontrol (yorum satırını kaldırarak test edebilirsiniz)
            // ReminderTimer_Elapsed(null, null);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtRoomNumber.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Oda numarası ve şifre alanları boş olamaz.");
                    return;
                }

                var repo = new UserRepository();
                User user = repo.OdaNoalma(txtRoomNumber.Text.Trim());

                if (user == null)
                {
                    MessageBox.Show($"Oda numarası '{txtRoomNumber.Text}' bulunamadı.");
                    return;
                }

                if (!user.IsActive)
                {
                    MessageBox.Show("Bu kullanıcı hesabı pasif durumda.");
                    return;
                }

                if (user.PasswordHash != txtPassword.Text)
                {
                    MessageBox.Show("Şifre hatalı.");
                    return;
                }

                // Başarılı giriş
                Hide();
                Form mainForm = user.IsAdmin
                    ? (Form)new AdminForm(user)
                    : (Form)new OgrenciForm(user);

                mainForm.FormClosed += (s, args) => Close();
                mainForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Giriş yapılırken hata oluştu: {ex.Message}");
            }
        }

        private void ReminderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var emailService = new Mail();
                var reservationRepo = new ReservationRepository();
                var userRepo = new UserRepository();
                
                // Debug: Timer çalışıyor mu kontrol et
                System.Diagnostics.Debug.WriteLine($"[TIMER] Kontrol zamanı: {DateTime.Now:HH:mm:ss}");
                
                var upcomingReservations = reservationRepo.GetUpcomingReservationsForReminder();
                
                System.Diagnostics.Debug.WriteLine($"[TIMER] Bulunan rezervasyon sayısı: {(upcomingReservations?.Count ?? 0)}");

                if (upcomingReservations != null && upcomingReservations.Count > 0)
                {
                    foreach (var reservation in upcomingReservations)
                    {
                        if (reservation == null) continue;

                        System.Diagnostics.Debug.WriteLine($"[TIMER] Rezervasyon ID: {reservation.Id}, Kullanıcı ID: {reservation.UserId}, Saat: {reservation.StartTime:HH:mm}");

                        // Kullanıcı bilgisini al
                        var user = userRepo.GetById(reservation.UserId);
                        if (user == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[TIMER] Kullanıcı bulunamadı! UserId: {reservation.UserId}");
                            continue;
                        }
                        
                        if (string.IsNullOrEmpty(user.Email))
                        {
                            System.Diagnostics.Debug.WriteLine($"[TIMER] Kullanıcının e-posta adresi yok! UserId: {reservation.UserId}, Ad: {user.FullName}");
                            continue;
                        }

                        // Makine adını al
                        var machineName = GetMachineName(reservation.MachineId);
                        if (string.IsNullOrEmpty(machineName))
                            machineName = "Bilinmeyen Makine";
                        
                        System.Diagnostics.Debug.WriteLine($"[TIMER] E-posta gönderiliyor: {user.Email}, Makine: {machineName}");
                        
                        // E-posta gönder
                        try
                        {
                            bool sent = emailService.SendReminderEmail(
                                user.Email, 
                                user.FullName ?? "Kullanıcı", 
                                reservation.StartTime, 
                                machineName
                            );
                            
                            if (sent)
                            {
                                System.Diagnostics.Debug.WriteLine($"[TIMER] E-posta başarıyla gönderildi! Rezervasyon ID: {reservation.Id}");
                                reservationRepo.MarkEmailSent(reservation.Id);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[TIMER] E-posta gönderilemedi! Rezervasyon ID: {reservation.Id}");
                            }
                        }
                        catch (Exception emailEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[TIMER] E-posta gönderme hatası: {emailEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata loglama
                System.Diagnostics.Debug.WriteLine($"[TIMER] Genel hata: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[TIMER] Stack trace: {ex.StackTrace}");
            }
        }

        private string GetMachineName(int machineId)
        {
            try
            {
                const string sql = "SELECT Name FROM Machines WHERE Id = @Id";
                var table = Database.ExecuteQuery(sql, new SqlParameter("@Id", machineId));
                if (table.Rows.Count > 0)
                    return Convert.ToString(table.Rows[0]["Name"]);
                return "Bilinmeyen Makine";
            }
            catch
            {
                return "Bilinmeyen Makine";
            }
        }
    }
}
