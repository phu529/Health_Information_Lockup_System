using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthServer
{
    internal class Program
    {
        private const int UDP_PORT = 8888;
        private const string DISCOVERY_REQUEST = "DISCOVER_HEALTH_SERVER";
        private const string DISCOVERY_RESPONSE = "HEALTH_SERVER_RESPONSE";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== HEALTH SERVER IS STARTING ===");

            if (Array.Exists(args, arg => arg == "--test-db"))
            {
                var database = new DatabaseHelper();

                await using var connection = database.CreateConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";

                var result = await command.ExecuteScalarAsync();
                Console.WriteLine($"[DB] SELECT 1 = {result}");
                return;
            }

            if (args.Length == 2 && args[0] == "--test-user")
            {
                var repository = new UserRepository(new DatabaseHelper());
                var user = await repository.FindForLoginAsync(args[1]);

                Console.WriteLine(user is null
                    ? "[DB] Không tìm thấy tài khoản."
                    : $"[DB] Tìm thấy tài khoản. UserID = {user.UserId}");
                return;
            }

            if (args.Length == 1 && args[0] == "--test-register")
            {
                Console.Write("Nhập mật khẩu thử: ");
                var password = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Mật khẩu thử không được để trống.");
                    return;
                }

                var salt = RandomNumberGenerator.GetBytes(16);
                var hash = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, 600_000, HashAlgorithmName.SHA256, 32);

                var repository = new UserRepository(new DatabaseHelper());
                var userId = await repository.CreateUserAsync(
                    "test_repo",
                    Convert.ToBase64String(hash),
                    Convert.ToBase64String(salt),
                    "Tài khoản thử",
                    email: null);

                Console.WriteLine($"[DB] Đã tạo test_repo. UserID = {userId}");
                return;
            }

            if (args.Length == 1 && args[0] == "--test-password")
            {
                var repository = new UserRepository(new DatabaseHelper());
                var user = await repository.FindForLoginAsync("test_repo");

                if (user is null)
                {
                    Console.WriteLine("[DB] Không tìm thấy test_repo.");
                    return;
                }

                Console.Write("Nhập mật khẩu thử: ");
                var password = Console.ReadLine() ?? string.Empty;

                var salt = Convert.FromBase64String(user.Salt);
                var storedHash = Convert.FromBase64String(user.PasswordHash);
                var enteredHash = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, user.Iterations, HashAlgorithmName.SHA256, 32);

                bool matches = CryptographicOperations.FixedTimeEquals(enteredHash, storedHash);
                bool loginSucceeded = await repository.RecordLoginResultAsync(user.UserId, matches);

                Console.WriteLine(loginSucceeded
                    ? "[DB] Đăng nhập thành công."
                    : "[DB] Đăng nhập bị từ chối.");
                return;
            }

            if (args.Length == 1 && args[0] == "--test-profile")
            {
                var repository = new UserRepository(new DatabaseHelper());
                var profile = await repository.GetProfileAsync(1);

                if (profile is null)
                {
                    Console.WriteLine("[DB] Không tìm thấy hồ sơ.");
                    return;
                }

                Console.WriteLine($"[DB] UserID: {profile.UserId}");
                Console.WriteLine($"Tên đăng nhập: {profile.Username}");
                Console.WriteLine($"Họ tên: {profile.FullName}");
                Console.WriteLine($"Email: {profile.Email ?? "(chưa có)"}");
                return;
            }

            if (args.Length == 1 && args[0] == "--test-update-profile")
            {
                var repository = new UserRepository(new DatabaseHelper());
                bool updated = await repository.UpdateProfileAsync(
                    userId: 1,
                    fullName: "Tài khoản thử đã cập nhật",
                    email: "test_repo@example.test");

                Console.WriteLine(updated
                    ? "[DB] Đã cập nhật hồ sơ."
                    : "[DB] Không tìm thấy hồ sơ để cập nhật.");
                return;
            }

            using var shutdown = new CancellationTokenSource();

            var serverRepository = new UserRepository(new DatabaseHelper());
            var tcpServer = new AuthTcpServer(new AuthRequestHandler(serverRepository));
            var tcpTask = tcpServer.StartAsync(shutdown.Token);

            _ = Task.Run(StartUdpDiscoveryListener);

            Console.WriteLine($"Server listening for UDP Broadcast on port {UDP_PORT}");
            Console.WriteLine("Press Enter to stop server...");
            Console.ReadLine();

            shutdown.Cancel();
            await tcpTask;
        }

        private static void StartUdpDiscoveryListener()
        {
            using var udpServer = new UdpClient(UDP_PORT);
            var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    byte[] data = udpServer.Receive(ref remoteEndpoint);
                    string message = Encoding.UTF8.GetString(data);

                    if (message == DISCOVERY_REQUEST)
                    {
                        Console.WriteLine($"[UDP] Received discovery from: {remoteEndpoint.Address}");

                        byte[] response = Encoding.UTF8.GetBytes(DISCOVERY_RESPONSE);
                        udpServer.Send(response, response.Length, remoteEndpoint);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[UDP Error] " + ex.Message);
                }
            }
        }
    }
}