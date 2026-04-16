using CourierTrack.Domain.Common;

namespace CourierTrack.Domain.Exceptions;

public class BadRequestException(string message) : BaseException(message, "BAD_REQUEST", 400)
{
}
