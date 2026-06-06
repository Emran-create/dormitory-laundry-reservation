using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace proje
{
    public static class Database
    {
        private static readonly string _connectionString;

        static Database()
        {
            try
            {
                var connString = ConfigurationManager.ConnectionStrings["camashırhane"];
                if (connString == null || string.IsNullOrEmpty(connString.ConnectionString))
                {
                    throw new Exception("Veritabanı bağlantı ayarları bulunamadı! App.config dosyasını kontrol edin.");
                }
                _connectionString = connString.ConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception($"Veritabanı bağlantı string'i yüklenemedi: {ex.Message}", ex);
            }
        }

        public static SqlConnection CreateOpenConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new Exception("Veritabanı bağlantı string'i boş!");
            }
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (var connection = CreateOpenConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 30; // sorgu zaman aşımı (saniye)
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                using (var adapter = new SqlDataAdapter(command))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var baglantı = CreateOpenConnection())
            using (var komut = new SqlCommand(sql, baglantı))
            {

                if(parameters != null && parameters.Length>0)
                {
                    komut.Parameters.AddRange(parameters);
                }
                return komut.ExecuteScalar();

            }
        }
        public static int ExecuteNonQuery(string sql,params SqlParameter[] parameters)
        {
            using (var baglantı = CreateOpenConnection())
            using (var komut = new SqlCommand(sql, baglantı))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    komut.Parameters.AddRange(parameters);
                }
                return komut.ExecuteNonQuery();
            }
        }
    }
}