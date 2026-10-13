
using System;
using Microsoft.Data.SqlClient;

namespace heathl_Infor_Server
{
    public static class DatabaseHelper
    {
        private const string ConnectionString =
            @"Server=LAPTOP-R8V3GT1G;Database=HealthInfoDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public static bool TestConnection(out string message)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    message = "Ket noi SQL Server thanh cong!";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Ket noi that bai: " + ex.Message;
                return false;
            }
        }
    }
}
