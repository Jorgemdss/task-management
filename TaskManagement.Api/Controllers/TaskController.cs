using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Extensions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Infrastructure.AuthHandlers;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly ITaskService _taskService;
    private readonly IAuthorizationService _authorizationService;

    public TaskController(ILogger<TaskController> logger, ITaskService taskService, IAuthorizationService authorizationService)
    {
        _logger = logger;
        _taskService = taskService;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskDto request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Creating new task for user {userId}", userId);


        var result = await _taskService.CreateTaskAsync(request, userId);
        _logger.LogInformation("Task created with success task for user {userId}", userId);

        return CreatedAtAction(nameof(CreateTask), new { Id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} getting task {id}", userId, id);

        var taskOwner = await _taskService.GetOwnerIdAsync(id);
        var auth = await _authorizationService.AuthorizeAsync(User, taskOwner, new UserOwnedResourcePermission());

        if (!auth.Succeeded)
        {
            throw new UnauthorizedTaskAccessException(id);
        }

        var task = await _taskService.GetTaskByIdAsync(id, userId);

        return Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTasks()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Getting all tasks for user {userId}.", userId);

        var result = await _taskService.GetUserTasksAsync(userId);

        return Ok(result);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskDto request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Updating task {id}.", id);

        var result = await _taskService.UpdateTaskAsync(id, request, userId);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {UserId} deleting task {TaskId}", userId, id);

        await _taskService.DeleteTaskAsync(id, userId);

        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> MarkAsCompleted(Guid id)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {UserId} marking task {TaskId} as completed", userId, id);

        var result = await _taskService.MarkTaskAsCompletedAsync(id, userId);

        return Ok(result);
    }

    [HttpPatch("{id}/incomplete")]
    public async Task<IActionResult> MarkAsIncomplete(Guid id)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {UserId} marking task {TaskId} as incomplete", userId, id);

        var result = await _taskService.MarkTaskAsIncompleteAsync(id, userId);

        return Ok(result);
    }
}

