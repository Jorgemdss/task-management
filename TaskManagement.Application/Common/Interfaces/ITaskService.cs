using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Common.Interfaces;

public interface ITaskService : IResourceOwnerService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, Guid userId);
    Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId);
    Task<IEnumerable<TaskDto>> GetUserTasksAsync(Guid userId);
    Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, Guid userId);
    Task DeleteTaskAsync(Guid taskId, Guid userId);
    Task<TaskDto> MarkTaskAsCompletedAsync(Guid taskId, Guid userId);
    Task<TaskDto> MarkTaskAsIncompleteAsync(Guid taskId, Guid userId);
}