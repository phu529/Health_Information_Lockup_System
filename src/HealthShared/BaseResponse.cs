namespace HealthShared
{
    public class BaseResponse
    {
        public string Command { get; set; }
        public ErrorCode Status { get; set; }
        public string Message { get; set; }
        public string Payload { get; set; }
    }
}