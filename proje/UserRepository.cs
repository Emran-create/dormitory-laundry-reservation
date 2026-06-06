using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CamasirhaneProje.Models;

namespace proje
{
    internal class UserRepository
    {
        public int DeactivateAndFreeRoom(int userId)
        {
            // Not: UNIQUE kısıtına takılmamak için RoomNumber'ı benzersiz bir "eski" değere taşıyoruz.
            // RoomNumber kolonu kısa olduğu için sadece Id kullanarak unique değer oluşturuyoruz
            const string sql = @"
        UPDATE Users
        SET IsActive = 0,
            RoomNumber = 'OLD_' + CAST(@UserId AS VARCHAR(10))
        WHERE Id = @UserId";
            return Database.ExecuteNonQuery(sql, new SqlParameter("@UserId", userId));
        }
        public User OdaNoalma(string roomNumber)
        {
            try
            {

                const string sql = "SELECT TOP 1 Id, RoomNumber, FullName, Email, PasswordHash, IsAdmin, IsActive FROM Users WHERE RoomNumber = @RoomNumber";
                var table = Database.ExecuteQuery(sql, new SqlParameter("@RoomNumber", roomNumber ?? ""));
                if (table.Rows.Count == 0) return null;

                var row = table.Rows[0];
                return Map(row);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Kullanıcı sorgulanırken hata: {ex.Message}");
                return null;
            }
        }
        private User Map(DataRow row) {
            return new User
            {
                Id = Convert.ToInt32(row["Id"]),
                RoomNumber = Convert.ToString(row["RoomNumber"]),
                FullName = Convert.ToString(row["FullName"]),
                Email = Convert.ToString(row["Email"]),
                PasswordHash = Convert.ToString(row["PasswordHash"]),
                IsAdmin = Convert.ToBoolean(row["IsAdmin"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
        public User GetById(int userId)
        {
            const string sql = "SELECT TOP 1 Id, RoomNumber, FullName, Email, PasswordHash, IsAdmin, IsActive FROM Users WHERE Id = @Id";
            var table = Database.ExecuteQuery(sql, new SqlParameter("@Id", userId));
            if (table.Rows.Count == 0) return null;

            var row = table.Rows[0];
            return new User
            {
                Id = Convert.ToInt32(row["Id"]),
                RoomNumber = Convert.ToString(row["RoomNumber"]),
                FullName = Convert.ToString(row["FullName"]),
                Email = Convert.ToString(row["Email"]),
                PasswordHash = Convert.ToString(row["PasswordHash"]),
                IsAdmin = Convert.ToBoolean(row["IsAdmin"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
