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
using CamasirhaneProje.Models;


namespace proje
{
    public partial class AdminForm : Form
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private readonly ReservationRepository _reservationRepository = new ReservationRepository();

        private readonly User _mevcutKullanıcı;
        public AdminForm(User mevcutKullanıcı)
        {
            InitializeComponent();

            _mevcutKullanıcı = mevcutKullanıcı;
            Text = $"Yönetici Paneli - Hoşgeldiniz{mevcutKullanıcı.FullName}";
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // TableAdapter'lar kaldırıldı - Artık SQL sorgularıyla veri çekiyoruz
            Ogrenci();
            Duyuru();

        }
        private void Ogrenci()
        {
            try
            {

                string sql = "SELECT Id, RoomNumber, FullName, Email, IsAdmin, IsActive FROM Users ORDER BY RoomNumber";
                var table = Database.ExecuteQuery(sql);
                dataGridView1.DataSource = table;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Öğrenci Listesi yüklenirken hata oluştu."+ex.Message);
            }
        }
        private void Duyuru()
        {
            try
            {
                string sql = "SELECT Id, Title, Content, CreatedAt, IsActive FROM Announcements ORDER BY CreatedAt DESC";
                var table = Database.ExecuteQuery(sql);
                dataGridView2.DataSource = table;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Duyurular yüklenirken hata oluştu." + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var room = txtOdaNumarası.Text?.Trim();
                var email = txtmail.Text?.Trim();
                var fullname = txtAd.Text?.Trim();
                var password = txtsifre.Text?.Trim();

                if (string.IsNullOrWhiteSpace(room) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Boş alan bırakmayın.");
                    return;
                }
                if (password.Length < 4)
                {
                    MessageBox.Show("Şifre en az 4 karakter olmalıdır.");
                    return;
                }
                // Basit e-posta kontrolü
                if (!email.Contains("@") || !email.Contains("."))
                {
                    MessageBox.Show("Geçerli bir e-posta girin.");
                    return;
                }

                const string kontrol = "SELECT COUNT(*) FROM Users WHERE RoomNumber = @RoomNumber";
                var count = Database.ExecuteScalar(kontrol, new SqlParameter("@RoomNumber", room));
                if (Convert.ToInt32(count) > 0)
                {
                    MessageBox.Show("Bu oda numarası zaten kayıtlı.");
                    return;
                }

                const string insertSql = @"INSERT INTO Users (RoomNumber, FullName, Email, PasswordHash, IsAdmin, IsActive) 
                                     VALUES (@RoomNumber, @FullName, @Email, @PasswordHash, @IsAdmin, @IsActive)";

                Database.ExecuteNonQuery(insertSql,
                    new SqlParameter("@RoomNumber", room),
                    new SqlParameter("@FullName", fullname),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@PasswordHash", password),
                    new SqlParameter("@IsAdmin", RBadmin.Checked),
                    new SqlParameter("@IsActive", true));

                MessageBox.Show("Öğrenci başarıyla eklendi!");
                OgrenciTemizle();
                Ogrenci();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }

        }
        private void OgrenciTemizle()
        {
            txtOdaNumarası.Clear();
            txtAd.Clear();
            txtmail.Clear();
            txtsifre.Clear();
            RBadmin.Checked = false;
        }

        private void duyuruekle_Click(object sender, EventArgs e)
        {
            try
            { 
                if(string.IsNullOrWhiteSpace(txtbaslık.Text)||string.IsNullOrWhiteSpace(txticerik.Text))
                {
                    MessageBox.Show("Boşluk bırakmayın!");
                    return;
                }
                string sql= @"INSERT INTO Announcements (Title, Content, CreatedAt, IsActive) 
                              VALUES (@Title, @Content, @CreatedAt, @IsActive)";
                Database.ExecuteNonQuery(sql,
                    new SqlParameter("@Title", txtbaslık.Text.Trim()),
                    new SqlParameter("@Content", txticerik.Text.Trim()),
                    new SqlParameter("@CreatedAt", DateTime.Now),
                    new SqlParameter("@IsActive", true));
                MessageBox.Show("Duyuru Başarıyla Eklendi.");
                duyurutemizle();
                Duyuru();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Duyuru Eklenirken Hata oluştu." + ex.Message);
            }
        }
        private void duyurutemizle()
        {
            txtbaslık.Clear();
            txticerik.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Öğrenci Seçin");
                return;
            }
            
            var currentRow = dataGridView1.CurrentRow;
            if (currentRow == null || currentRow.Cells == null)
            {
                MessageBox.Show("Geçersiz satır seçimi.");
                return;
            }

            // Kolon adlarını kontrol et
            if (dataGridView1.Columns["Id"] == null || dataGridView1.Columns["RoomNumber"] == null)
            {
                MessageBox.Show("Geçersiz kolon yapısı.");
                return;
            }

            if (currentRow.Cells["Id"].Value == null || currentRow.Cells["RoomNumber"].Value == null)
            {
                MessageBox.Show("Seçili satırda veri bulunamadı.");
                return;
            }

            int kısıID = Convert.ToInt32(currentRow.Cells["Id"].Value);
            string odano = Convert.ToString(currentRow.Cells["RoomNumber"].Value);
            
            var onaylama = MessageBox.Show($"{odano} oda kullanıcısını pasifleştirip gelecekteki tüm rezervasyonlarını iptal etmek istiyor musunuz?",
                "Odayı Boşalt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            
            if (onaylama != DialogResult.Yes) return;
            
            try
            {
                int userId = kısıID;
                int red = _reservationRepository.CancelFutureReservationsByUser(userId);
                _userRepository.DeactivateAndFreeRoom(userId);

                OgrenciTemizle();
                Ogrenci(); // listeyi tazele
                MessageBox.Show($"Oda boşaltıldı. İptal edilen rezervasyon: {red}");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

    }
}
