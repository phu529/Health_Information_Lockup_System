// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        int port = 5000;

        TcpListener server = new TcpListener(
            IPAddress.Any, port
        );

        server.Start();

        Console.WriteLine(
            $"Server dang lang nghe port {port}..."
        );

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("Client da ket noi!");

            using (client)
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.AutoFlush = true;

                string? message = reader.ReadLine();

                if (message != null)
                {
                    Console.WriteLine($"Client gui: {message}");

                    writer.WriteLine(message);

                    Console.WriteLine($"Server tra: {message}");
                }
            }

            Console.WriteLine("Client da ngat ket noi.");
        }
    }
}
