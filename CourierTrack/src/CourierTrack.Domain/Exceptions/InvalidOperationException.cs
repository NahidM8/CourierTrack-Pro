namespace CourierTrack.Domain.Exceptions;

public class InvalidOperationException(string message) : BaseException(message, "400", 400)
{
}