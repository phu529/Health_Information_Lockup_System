namespace HealthShared
{
    public class BaseResponse
    {
        public string Command { get; set; }
        // Bước 1.3: mã định danh do Client đặt; Server trả lại NGUYÊN VẸN trong phản hồi
        // để Client ghép đúng phản hồi với yêu cầu (Test Case #2, #22). Có thể null.
        public string? RequestId { get; set; }
        public ErrorCode Status { get; set; }
        public string Message { get; set; }
        public string Payload { get; set; }
    }
}