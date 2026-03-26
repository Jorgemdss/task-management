using System.Net;

namespace TaskManagement.Domain.Exceptions;

public class UnauthorizedTaskAccessException : BaseDomainException
{
    public UnauthorizedTaskAccessException(Guid taskId)
        : base($"You are not authorized to access task '{taskId}'.", HttpStatusCode.Forbidden)
    {
    }
}