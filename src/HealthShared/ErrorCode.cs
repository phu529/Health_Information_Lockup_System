namespace HealthShared
{
    public enum ErrorCode
    {
        Success = 200,
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        PayloadTooLarge = 413,
        InternalServerError = 500
    }
}