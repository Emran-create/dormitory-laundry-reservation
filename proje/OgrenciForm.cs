using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CamasirhaneProje.Models;
using System.Windows.Forms;

namespace proje
{
    public partial class OgrenciForm : Form
    {
        private readonly ReservationRepository reservationRepository;
        private DateTime _selectedDate = DateTime.Today;
        private readonly User _mvctUser;
        private DataGridView dataGridViewDuyurular;

        public OgrenciForm(User mvctUser)
        {
            InitializeComponent();
            
            if (mvctUser == null)
            {
                throw new ArgumentNullException(nameof(mvctUser), "Kullanıcı bilgisi boş olamaz!");
            }
            
            _mvctUser = mvctUser;
            reservationRepository = new ReservationRepository();
            Text = $"Hoşgeldin -{mvctUser.FullName ?? "Kullanıcı"}";

            // Duyurular gridini programatik olarak ekle
            dataGridViewDuyurular = new DataGridView();
            dataGridViewDuyurular.Name = "dataGridViewDuyurular";
            dataGridViewDuyurular.ReadOnly = true;
            dataGridViewDuyurular.AllowUserToAddRows = false;
            dataGridViewDuyurular.AllowUserToDeleteRows = false;
            dataGridViewDuyurular.AllowUserToResizeRows = false;
            dataGridViewDuyurular.MultiSelect = false;
            dataGridViewDuyurular.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDuyurular.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewDuyurular.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewDuyurular.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewDuyurular.RowHeadersVisible = false;
            dataGridViewDuyurular.Dock = DockStyle.Bottom;
            dataGridViewDuyurular.Height = 160;
            Controls.Add(dataGridViewDuyurular);

        }

        private void OgrenciForm_Load(object sender, EventArgs e)
        {
            try
            {
                // TableAdapter kaldırıldı - Artık SQL sorgularıyla veri çekiyoruz
                if (dateTimePicker1 != null)
                {
                    dateTimePicker1.Value = DateTime.Today;
                    dateTimePicker1.MinDate = DateTime.Today;
                    // Hafta sonu seçimine izin verme: başlangıç tarihi hafta sonu ise ilk iş gününe çek
                    if (dateTimePicker1.Value.DayOfWeek == DayOfWeek.Saturday || dateTimePicker1.Value.DayOfWeek == DayOfWeek.Sunday)
                    {
                        var d = dateTimePicker1.Value;
                        while (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                        {
                            d = d.AddDays(1);
                        }
                        dateTimePicker1.Value = d;
                    }
                }

                LoadMachines();
                LoadPrograms();
                LoadTimeSlots();
                LoadAnnouncements();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Form yüklenirken hata: {ex.Message}");
            }
        }

        private void LoadMachines()
        {
            try
            {
                if (cmbMakine == null) return;
                
                const string sql = "SELECT Id, Name FROM Machines WHERE IsActive = 1 ORDER BY Name";
                var table = Database.ExecuteQuery(sql);
                if (table == null || table.Rows.Count == 0)
                {
                    MessageBox.Show("Aktif makine bulunamadı. Lütfen admin paneline başvurun.");
                    return;
                }
                cmbMakine.DisplayMember = "Name";
                cmbMakine.ValueMember = "Id";
                cmbMakine.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Makine listesi yüklenirken hata: {ex.Message}");
            }
        }

        private void LoadAnnouncements()
        {
            try
            {
                if (dataGridViewDuyurular == null) return;
                
                const string sql = "SELECT Title, Content, CreatedAt AS Tarih FROM Announcements WHERE IsActive = 1 ORDER BY CreatedAt DESC";
                var table = Database.ExecuteQuery(sql);
                if (table != null)
                {
                    dataGridViewDuyurular.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Duyurular yüklenirken hata: {ex.Message}");
            }
        }

        private void LoadPrograms()
        {
            try
            {
                if (cmbProgram == null) return;
                
                const string sql = "SELECT Id, Name, DurationMinutes FROM LaundryPrograms ORDER BY Name";
                var table = Database.ExecuteQuery(sql);
                if (table == null || table.Rows.Count == 0)
                {
                    MessageBox.Show("Program bulunamadı. Lütfen admin paneline başvurun.");
                    return;
                }
                cmbProgram.DisplayMember = "Name";
                cmbProgram.ValueMember = "Id";
                cmbProgram.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Program listesi yüklenirken hata: {ex.Message}");
            }
        }
        private void LoadTimeSlots()
        {
            try
            {
                if (dateTimePicker1 == null) return;
                _selectedDate = dateTimePicker1.Value.Date;

                if (dataGridView2 == null) return;
                if (cmbMakine == null) return;

                // Makine seçili mi kontrol et
                if (cmbMakine.SelectedValue == null)
                {
                    // Makine seçili değilse boş tablo göster
                    var emptyTable = new DataTable();
                    emptyTable.Columns.Add("Saat", typeof(string));
                    emptyTable.Columns.Add("Durum", typeof(string));
                    emptyTable.Columns.Add("Renk", typeof(string));

                    dataGridView2.DataSource = emptyTable;
                    return;
                }

                var table = new DataTable();
                table.Columns.Add("Saat", typeof(string));
                table.Columns.Add("Durum", typeof(string));
                table.Columns.Add("Renk", typeof(string));

                // Yalnızca hafta içi ve 08:00-16:00 arası
                if (_selectedDate.DayOfWeek == DayOfWeek.Saturday || _selectedDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    var emptyTable = new DataTable();
                    emptyTable.Columns.Add("Saat", typeof(string));
                    emptyTable.Columns.Add("Durum", typeof(string));
                    emptyTable.Columns.Add("Renk", typeof(string));
                    if (dataGridView2 != null)
                    {
                        dataGridView2.DataSource = emptyTable;
                    }
                    return;
                }

                for (int hour = 8; hour < 24; hour++)
                {
                    var slotStart = _selectedDate.AddHours(hour);
                    var slotEnd = slotStart.AddHours(1);

                    bool isOccupied = IsSlotOccupied(slotStart, slotEnd);

                    var row = table.NewRow();
                    row["Saat"] = $"{hour:00}:00 - {(hour + 1):00}:15";
                    row["Durum"] = isOccupied ? "DOLU" : "BOŞ";
                    row["Renk"] = isOccupied ? "Kırmızı" : "Yeşil";
                    table.Rows.Add(row);
                }

                // Tasarımda önceden tanımlı kolonlar varsa temizleyip isimleri DataTable ile hizala
                if (dataGridView2 != null)
                {
                    dataGridView2.AutoGenerateColumns = true;
                    dataGridView2.Columns.Clear();
                    dataGridView2.DataSource = table;
                    // Renkleri uygula
                    if (dataGridView2.Rows != null)
                    {
                        foreach (DataGridViewRow dgRow in dataGridView2.Rows)
                        {
                            if (dgRow == null || dgRow.Cells == null || dgRow.Cells.Count < 2) continue;

                            var state = Convert.ToString(dgRow.Cells[1].Value);
                            if (state == "DOLU")
                            {
                                dgRow.DefaultCellStyle.BackColor = Color.MistyRose;
                                dgRow.DefaultCellStyle.ForeColor = Color.DarkRed;
                            }
                            else
                            {
                                dgRow.DefaultCellStyle.BackColor = Color.Honeydew;
                                dgRow.DefaultCellStyle.ForeColor = Color.DarkGreen;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Zaman dilimleri yüklenirken hata: {ex.Message}");
            }
        }

        private bool IsSlotOccupied(DateTime startTime, DateTime endTime)
        {
            try
            {
                if (reservationRepository == null) return false;
                if (cmbMakine == null || cmbMakine.SelectedValue == null) return false;

                int machineId = Convert.ToInt32(cmbMakine.SelectedValue);
                return reservationRepository.IsOverlapping(machineId, startTime, endTime);
            }
            catch (Exception ex)
            {
                // Sessizce false döndür - hata loglama
                System.Diagnostics.Debug.WriteLine($"Slot kontrolü sırasında hata: {ex.Message}");
                return false;
            }
        }
        private void cmbProgram_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (reservationRepository == null)
                {
                    MessageBox.Show("Rezervasyon sistemi hazır değil. Lütfen tekrar deneyin.");
                    return;
                }

                if (_mvctUser == null)
                {
                    MessageBox.Show("Kullanıcı bilgisi bulunamadı.");
                    return;
                }

                // Hafta sonu rezervasyonuna izin verme
                if (_selectedDate.DayOfWeek == DayOfWeek.Saturday || _selectedDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    MessageBox.Show("Hafta sonu rezervasyon yapılamaz.");
                    return;
                }

                if (cmbMakine == null || cmbProgram == null)
                {
                    MessageBox.Show("Form kontrolleri yüklenemedi. Lütfen tekrar deneyin.");
                    return;
                }

                if (cmbMakine.SelectedValue == null || cmbProgram.SelectedValue == null)
                {
                    MessageBox.Show("Makine ve Program seçiniz.");
                    return;
                }

                if (dataGridView2 == null || dataGridView2.CurrentRow == null)
                {
                    MessageBox.Show("Saat seçiniz.");
                    return;
                }
                var selectedrow = dataGridView2.CurrentRow;
                if (selectedrow == null || selectedrow.Cells == null || selectedrow.Cells.Count < 2)
                {
                    MessageBox.Show("Geçersiz satır seçimi. Lütfen tekrar deneyin.");
                    return;
                }
                string durum = Convert.ToString(selectedrow.Cells[1].Value ?? "");

                if (durum == "DOLU")
                {
                    MessageBox.Show("Bu saat dolu");
                    return;
                }

                int hour = 8 + dataGridView2.CurrentRow.Index;
                var startTime = _selectedDate.AddHours(hour);

                // Program seçimi kontrolü
                var selectedItem = cmbProgram.SelectedItem as DataRowView;
                if (selectedItem == null)
                {
                    MessageBox.Show("Program seçimi geçersiz. Lütfen tekrar deneyin.");
                    return;
                }
                var programRow = selectedItem.Row;
                if (programRow == null || programRow["DurationMinutes"] == null || programRow["DurationMinutes"] == DBNull.Value)
                {
                    MessageBox.Show("Program süresi alınamadı. Lütfen tekrar deneyin.");
                    return;
                }
                int duration = Convert.ToInt32(programRow["DurationMinutes"]);
                var endTime = startTime.AddMinutes(duration);

                // Geçmiş saat için engelleme
                if (startTime < DateTime.Now)
                {
                    MessageBox.Show("Geçmiş bir saat için rezervasyon yapılamaz.");
                    return;
                }

                // Çakışma kontrolü (program süresine göre)
                if (IsSlotOccupied(startTime, endTime))
                {
                    MessageBox.Show("Seçilen zaman aralığı dolu. Başka bir saat deneyin.");
                    return;
                }

                var reservation = new Reservation
                {
                    UserId = _mvctUser.Id,
                    MachineId = Convert.ToInt32(cmbMakine.SelectedValue),
                    ProgramId = Convert.ToInt32(cmbProgram.SelectedValue),
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = "Pending"
                };

                try
                {
                    reservationRepository.Create(reservation);
                    MessageBox.Show($"Rezervasyon oluşturuldu!\nSaat: {startTime:HH:mm}\nBitiş: {endTime:HH:mm}");
                    LoadTimeSlots();
                    if (label1 != null)
                    {
                        label1.Text = "Seçilen Saat: -";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rezervasyon oluşturulurken hata: {ex.Message}");
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (dateTimePicker1 == null) return;
                
                // Hafta sonu seçilirse otomatik olarak bir sonraki pazartesiye taşı
                if (dateTimePicker1.Value.DayOfWeek == DayOfWeek.Saturday || dateTimePicker1.Value.DayOfWeek == DayOfWeek.Sunday)
                {
                    var d = dateTimePicker1.Value;
                    while (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                    {
                        d = d.AddDays(1);
                    }
                    dateTimePicker1.Value = d;
                }
                LoadTimeSlots();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tarih değiştirilirken hata: {ex.Message}");
            }
        }

        private void cmbMakine_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTimeSlots();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Makine seçimi değiştirilirken hata: {ex.Message}");
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || dataGridView2.Rows == null || e.RowIndex >= dataGridView2.Rows.Count)
                    return;

                var row = dataGridView2.Rows[e.RowIndex];
                if (row == null || row.Cells == null || row.Cells.Count < 2)
                    return;

                string durum = Convert.ToString(row.Cells[1].Value ?? "");

                if (durum == "DOLU")
                {
                    MessageBox.Show("Bu saat dolu! Başka bir saat seçiniz.");
                    return;
                }
                
                if (row.Cells.Count > 0 && row.Cells[0].Value != null)
                {
                    label1.Text = $"Seçilen Saat: {Convert.ToString(row.Cells[0].Value)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satır seçilirken hata: {ex.Message}");
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
