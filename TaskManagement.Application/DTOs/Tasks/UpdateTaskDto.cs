namespace TaskManagement.Application.DTOs;

public record UpdateTaskDto(
    string Title,
    string Description,
    DateTime? DueDate
);

