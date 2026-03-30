using System.Net;

namespace TaskManagement.Domain.Exceptions;

public abstract class BaseDomainException : Exception
{
    public HttpStatusCode StatusCode { get; }


    protected BaseDomainException(string message, HttpStatusCode statusCode)
    :base(message)
    {
        StatusCode = statusCode;
    }
}