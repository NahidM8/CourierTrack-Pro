using CourierTrack.Domain.Common;

namespace CourierTrack.Domain.Exceptions;

public class ConflictException(string message) : BaseException(message, "409", 409)
{
}
