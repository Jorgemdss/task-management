using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Models;

namespace TaskManagement.Application.Mappers;

public static class TaskMapper
{
    public static TaskDto MapToDto(this TaskItem task)
    {
        return new TaskDto(
            Id: task.Id,
            UserId: task.UserId,
            Title: task.Title,
            Description: task.Description,
            IsCompleted: task.IsCompleted,
            DueDate: task.DueDate,
            CreatedAt: task.CreatedAt,
            UpdatedAt: task.UpdatedAt
        );
    }

    public static IEnumerable<TaskDto> MapToDtos(this IEnumerable<TaskItem> tasks)
    {
        return [.. tasks.Select(t => t.MapToDto())];
    }
}