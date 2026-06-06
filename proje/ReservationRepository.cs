using CamasirhaneProje.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proje
{
    internal class ReservationRepository
    {
        public int CancelFutureReservationsByUser(int userId)
        {
            const string sql = @"
        UPDATE Reservations
        SET Status = 'Cancelled'
        WHERE UserId = @UserId
          AND StartTime >= SYSDATETIME()
          AND Status IN ('Pending','Active')";
            return Database.ExecuteNonQuery(sql, new SqlParameter("@UserId", userId));
        }
        public bool IsOverlapping(int machineId, DateTime startTime, DateTime endTime)
        {
            const string sql = @"SELECT COUNT(1)
                         FROM Reservations
                         WHERE MachineId = @MachineId
                           AND Status IN ('Pending','Active')
                           AND NOT (@EndTime <= StartTime OR @StartTime >= EndTime)";
            var countObj = Database.ExecuteScalar(sql,
                new SqlParameter("@MachineId", machineId),
                new SqlParameter("@StartTime", startTime),
                new SqlParameter("@EndTime", endTime));
            return Convert.ToInt32(countObj) > 0;
        }

        public int Create(Reservation reservation)
        {
            const string sql = @"INSERT INTO Reservations (UserId, MachineId, ProgramId, StartTime, EndTime, Status)
                         VALUES (@UserId, @MachineId, @ProgramId, @StartTime, @EndTime, @Status);
                         SELECT SCOPE_IDENTITY();";
            var idObj = Database.ExecuteScalar(sql,
                new SqlParameter("@UserId", reservation.UserId),
                new SqlParameter("@MachineId", reservation.MachineId),
                new SqlParameter("@ProgramId", reservation.ProgramId),
                new SqlParameter("@StartTime", reservation.StartTime),
                new SqlParameter("@EndTime", reservation.EndTime),
                new SqlParameter("@Status", reservation.Status ?? "Pending"));
            return Convert.ToInt32(idObj);
        }
    
        public List<Reservation> GetUpcomingReservationsForReminder()
        {
            try
            {
                // EmailSent kolonu olmayabilir, bu yüzden dinamik SQL kullanıyoruz
                // Önce EmailSent kolonunun var olup olmadığını kontrol edelim
                string sql;
                try
                {
                    // EmailSent kolonu varsa bu sorguyu kullan
                    sql = @"
        SELECT r.Id, r.UserId, r.MachineId, r.ProgramId, r.StartTime, r.EndTime, r.Status
        FROM Reservations r
        WHERE r.Status IN ('Pending', 'Active')
          AND (r.EmailSent = 0 OR r.EmailSent IS NULL)
          AND r.StartTime BETWEEN GETDATE() AND DATEADD(MINUTE, 30, GETDATE())";
                    
                    var testTable = Database.ExecuteQuery(sql);
                    sql = @"
        SELECT r.Id, r.UserId, r.MachineId, r.ProgramId, r.StartTime, r.EndTime, r.Status
        FROM Reservations r
        WHERE r.Status IN ('Pending', 'Active')
          AND (r.EmailSent = 0 OR r.EmailSent IS NULL)
          AND r.StartTime BETWEEN GETDATE() AND DATEADD(MINUTE, 30, GETDATE())";
                }
                catch
                {
                    // EmailSent kolonu yoksa bu sorguyu kullan
                    sql = @"
        SELECT r.Id, r.UserId, r.MachineId, r.ProgramId, r.StartTime, r.EndTime, r.Status
        FROM Reservations r
        WHERE r.Status IN ('Pending', 'Active')
          AND r.StartTime BETWEEN GETDATE() AND DATEADD(MINUTE, 30, GETDATE())";
                }

                var table = Database.ExecuteQuery(sql);
                var list = new List<Reservation>();

                if (table == null || table.Rows == null)
                {
                    System.Diagnostics.Debug.WriteLine("[REPO] Tablo null veya Rows null");
                    return list;
                }

                System.Diagnostics.Debug.WriteLine($"[REPO] Toplam {table.Rows.Count} rezervasyon bulundu");

                foreach (DataRow row in table.Rows)
                {
                    if (row == null) continue;
                    
                    try
                    {
                        var reservation = new Reservation
                        {
                            Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Id"]),
                            UserId = row["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserId"]),
                            MachineId = row["MachineId"] == DBNull.Value ? 0 : Convert.ToInt32(row["MachineId"]),
                            ProgramId = row["ProgramId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ProgramId"]),
                            StartTime = row["StartTime"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(row["StartTime"]),
                            EndTime = row["EndTime"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(row["EndTime"]),
                            Status = row["Status"] == DBNull.Value ? "" : Convert.ToString(row["Status"])
                        };
                        
                        System.Diagnostics.Debug.WriteLine($"[REPO] Rezervasyon eklendi: ID={reservation.Id}, UserId={reservation.UserId}, StartTime={reservation.StartTime:HH:mm}");
                        list.Add(reservation);
                    }
                    catch (Exception rowEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[REPO] Satır işleme hatası: {rowEx.Message}");
                        continue;
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[REPO] GetUpcomingReservationsForReminder hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[REPO] Stack trace: {ex.StackTrace}");
                return new List<Reservation>();
            }
        }

        public void MarkEmailSent(int reservationId)
        {
            // EmailSent kolonu yoksa hata vermemesi için kontrol edelim
            try
            {
                const string sql = "UPDATE Reservations SET EmailSent = 1 WHERE Id = @Id";
                Database.ExecuteNonQuery(sql, new SqlParameter("@Id", reservationId));
            }
            catch
            {
                // EmailSent kolonu yoksa sessizce devam et
            }
        }
    }
   }
