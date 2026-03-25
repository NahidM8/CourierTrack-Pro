namespace CourierTrack.Domain.Common;

public class BaseException(string message, string errorCode, int statusCode = 500) : Exception(message)
{
    public string ErrorCode { get; init; } = errorCode;
    public int StatusCode { get; init; } = statusCode;
}
