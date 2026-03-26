using System.Net;

namespace TaskManagement.Domain.Exceptions;

public class TaskNotFoundException : BaseDomainException
{
    public TaskNotFoundException(Guid taskId)
        : base($"Task with ID '{taskId}' was not found.", HttpStatusCode.NotFound)
    {
    }
}