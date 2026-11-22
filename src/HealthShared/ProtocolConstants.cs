namespace HealthShared
{
    /// <summary>
    /// Hằng số giao thức dùng chung cho Client và Server (Bước 1.3).
    /// Mô tả đầy đủ ở docs/protocol.md. Đổi số ở đây thì cả hai phía đổi theo.
    /// </summary>
    public static class ProtocolConstants
    {
        /// <summary>Cổng TCP của Server.</summary>
        public const int DefaultTcpPort = 5000;

        /// <summary>Kích thước tối đa MỘT thông điệp (tính theo byte UTF-8, chưa kể LF). 64 KiB.</summary>
        public const int MaxMessageBytes = 64 * 1024;

        /// <summary>Server cắt kết nối nếu im lặng quá số giây này (Client nên gửi PING định kỳ).</summary>
        public const int ServerIdleTimeoutSeconds = 90;

        /// <summary>Gợi ý chu kỳ Client gửi PING để giữ kết nối (phải nhỏ hơn thời gian chờ ở trên).</summary>
        public const int ClientPingIntervalSeconds = 30;

        /// <summary>Client chờ phản hồi một yêu cầu tối đa bấy nhiêu giây.</summary>
        public const int ClientRequestTimeoutSeconds = 10;

        /// <summary>Client chờ nối được tới Server tối đa bấy nhiêu giây.</summary>
        public const int ClientConnectTimeoutSeconds = 5;
    }

    /// <summary>Lệnh của lõi giao thức (không thuộc nghiệp vụ nào). Lệnh đăng nhập/đăng ký xem AuthCommands.</summary>
    public static class CoreCommands
    {
        /// <summary>Kiểm tra sống: Server trả Status=200, Message="PONG", Payload = đúng Payload đã gửi.</summary>
        public const string Ping = "PING";
    }
}
