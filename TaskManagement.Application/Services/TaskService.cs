using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappers;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Models;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private IApplicationDbContext _dbContext;
    private ILogger<TaskService> _logger;

    public TaskService(IApplicationDbContext dbContext, ILogger<TaskService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, Guid userId)
    {
        _logger.LogInformation("Creating task for user {UserId}", userId);

        var task = TaskItem.Create(dto.Title, dto.Description, userId, dto.DueDate);

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Task {id} created with success!", task.Id);

        return task.MapToDto();
    }

    public async Task DeleteTaskAsync(Guid taskId, Guid userId)
    {
        _logger.LogInformation("Deleting task {id}", taskId);

        var task = await GetTaskEntityAsync(taskId, userId);

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} deleted successfully", taskId);

    }

    public async Task<Guid> GetOwnerIdAsync(Guid resourceId)
    {
        var task = await _dbContext.Tasks
           .Where(t => t.Id == resourceId)
           .Select(t => t.Id)
           .SingleOrDefaultAsync();

        if (task == default)
            throw new TaskNotFoundException(resourceId);

        return task;
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId)
    {
        var task = await GetTaskEntityAsync(taskId, userId);
        return task.MapToDto();
    }

    public async Task<IEnumerable<TaskDto>> GetUserTasksAsync(Guid userId)
    {
        var tasks = await _dbContext.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.MapToDtos();
    }

    public async Task<TaskDto> MarkTaskAsCompletedAsync(Guid taskId, Guid userId)
    {
        _logger.LogInformation("Marking task {TaskId} as completed", taskId);

        var task = await GetTaskEntityAsync(taskId, userId);

        task.MarkAsCompleted();

        await _dbContext.SaveChangesAsync();

        return task.MapToDto();
    }

    public async Task<TaskDto> MarkTaskAsIncompleteAsync(Guid taskId, Guid userId)
    {
        _logger.LogInformation("Marking task {TaskId} as incompleted", taskId);

        var task = await GetTaskEntityAsync(taskId, userId);

        task.MarkAsIncomplete();

        await _dbContext.SaveChangesAsync();

        return task.MapToDto();
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, Guid userId)
    {
        _logger.LogInformation("Updating task for user {UserId}", userId);

        var task = await GetTaskEntityAsync(taskId, userId);

        task.Update(dto.Title, dto.Description, dto.DueDate);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Task {id} updated with success!", task.Id);

        return task.MapToDto();
    }

    private async Task<TaskItem> GetTaskEntityAsync(Guid taskId, Guid userId)
    {
        _logger.LogError("Searching for task {taskId} and user {userId}", taskId, userId);

        var task = await _dbContext.Tasks.SingleOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            _logger.LogError("Cannot delete Task with id: {id}, does not exist.", taskId);
            throw new TaskNotFoundException(taskId);
        }

        if (task.UserId != userId)
        {
            // TODO JS: remove this, only here for tests to pass
            throw new UnauthorizedTaskAccessException(taskId);

        }

        return task;
    }
}