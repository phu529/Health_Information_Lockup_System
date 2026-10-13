using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.SqlClient;

class Program
{
    static string connectionString =
        @"Server=LAPTOP-R8V3GT1G;Database=HealthInfoDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

    static void Main()
    {
        int port = 5000;

        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();

        Console.WriteLine($"Server dang lang nghe port {port}...");

        while (true)
        {
            using TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("Client da ket noi!");

            try
            {
                using NetworkStream stream = client.GetStream();
                using StreamReader reader =
                    new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer =
                    new StreamWriter(stream, Encoding.UTF8);

                writer.AutoFlush = true;

                string? message = reader.ReadLine();

                if (message != null)
                {
                    Console.WriteLine($"Client gui: {message}");
                    string clientIP =((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();
                    try
                    {
                        long maLichSu = LuuLichSu(clientIP, message);
                        string response = $"Da luu request vao SQL. Ma lich su: {maLichSu}";
                        writer.WriteLine(response);
                        Console.WriteLine(response);
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"Loi SQL: {ex.Message}");
                        writer.WriteLine("Luu request vao SQL that bai.");
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Loi truyen du lieu: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi xu ly: {ex.Message}");
            }

            Console.WriteLine("Client da ngat ket noi.");
        }
    }

    static long LuuLichSu(string clientIP, string noiDung)
    {
        using SqlConnection connection =
            new SqlConnection(connectionString);

        connection.Open();

        string sql = @"
            INSERT INTO dbo.LichSuRequest(ClientIP, LoaiRequest, NoiDung) OUTPUT INSERTED.MaLichSu
            VALUES
                (@ClientIP, @LoaiRequest, @NoiDung);";

        using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@ClientIP", SqlDbType.VarChar, 45
        ).Value = clientIP;

        command.Parameters.Add(
            "@LoaiRequest", SqlDbType.NVarChar, 50
        ).Value = "TEST";

        command.Parameters.Add(
            "@NoiDung", SqlDbType.NVarChar, 2000
        ).Value = noiDung;

        return Convert.ToInt64(command.ExecuteScalar());
    }
}