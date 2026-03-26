using System.Security.Cryptography.X509Certificates;

namespace TaskManagement.Application.DTOs;

public record TaskDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? UpdatedAt
);