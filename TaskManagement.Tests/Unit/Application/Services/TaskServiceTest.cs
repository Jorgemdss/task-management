using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Models;
using TaskManagement.Infrastructure.Data;
using Xunit;

namespace TaskManagement.Tests.Unit.Application.Services;

public class TaskServiceTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<ILogger<TaskService>> _logger;
    private readonly TaskService _taskService;

    public TaskServiceTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

        _dbContext = new AppDbContext(optionsBuilder.Options);
        _logger = new Mock<ILogger<TaskService>>();

        _taskService = new TaskService(_dbContext, _logger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidData_ShouldCreateTask()
    {
        var userId = Guid.NewGuid();

        var request = new CreateTaskDto(
            "Title",
            "Desc",
            DateTime.UtcNow.AddDays(3));

        var taskDto = await _taskService.CreateTaskAsync(request, userId);

        taskDto.Title.Should().Be("Title");
        taskDto.Description.Should().Be("Desc");

        var taskItem = await _dbContext.Tasks.SingleOrDefaultAsync(t => t.Id == taskDto.Id);

        taskItem.Should().NotBeNull();
        taskItem.Title.Should().Be("Title");
        taskItem.Description.Should().Be("Desc");
        taskItem.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateTaskAsync_WithoutTitle_ShouldThrow()
    {
        var userId = Guid.NewGuid();

        var request = new CreateTaskDto(
            "",
            "Desc",
            DateTime.UtcNow.AddDays(3));

        await Assert.ThrowsAsync<ArgumentException>(async () => await _taskService.CreateTaskAsync(request, userId));
    }

    [Fact]
    public async Task GetTaskByIdAsync_WhenTaskExists_ShouldReturnTask()
    {
        var userId = Guid.NewGuid();

        var request = new CreateTaskDto(
            "Title",
            "Desc",
            DateTime.UtcNow.AddDays(3));

        var taskDto = await _taskService.CreateTaskAsync(request, userId);

        var taskItem = await _taskService.GetTaskByIdAsync(taskDto.Id, userId);

        taskItem.Should().NotBeNull();
        taskItem.Id.Should().Be(taskDto.Id);
        taskItem.Title.Should().Be(taskDto.Title);
        taskItem.Description.Should().Be(taskDto.Description);
        taskItem.CreatedAt.Should().Be(taskDto.CreatedAt);
        taskItem.DueDate.Should().Be(taskDto.DueDate);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WhenDoesntExist_ShouldThrow()
    {
        await Assert.ThrowsAsync<TaskNotFoundException>(
            async () => await _taskService.GetTaskByIdAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTaskByIdAsync_IfUserNotOwner_ShouldThrowUnauthorized()
    {
        var userId = Guid.NewGuid();

        var request = new CreateTaskDto(
            "Title",
            "Desc",
            DateTime.UtcNow.AddDays(3));

        var taskDto = await _taskService.CreateTaskAsync(request, userId);

        await Assert.ThrowsAsync<UnauthorizedTaskAccessException>(
            async () =>
                await _taskService.GetTaskByIdAsync(taskDto.Id, Guid.NewGuid())
        );
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenExistent_ShouldReturnOnlyUserTasks()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        await _taskService.CreateTaskAsync(new CreateTaskDto("Title 1", "short desc", null), userId1);
        await _taskService.CreateTaskAsync(new CreateTaskDto("New Task nice", "short desc", null), userId1);

        await _taskService.CreateTaskAsync(new CreateTaskDto("Something", "short desc 2", null), userId2);
        await _taskService.CreateTaskAsync(new CreateTaskDto("Do this!", "short desc 2", null), userId2);

        var user1TasksDtos = await _taskService.GetUserTasksAsync(userId1);

        user1TasksDtos.Should().NotBeEmpty();
        user1TasksDtos.Should().HaveCount(2);
        user1TasksDtos.Where(t => t.UserId == userId1).Should().HaveCount(2);
        user1TasksDtos.Where(t => t.UserId == userId2).Should().HaveCount(0);

    }

    [Fact]
    public async Task DeleteTaskAsync_IfExistent_ShouldDeleteTask()
    {
        var userId = Guid.NewGuid();

        await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();

        await _taskService.DeleteTaskAsync(task.Id, userId);

        var result = await _dbContext.Tasks.SingleOrDefaultAsync(t => t.Id == task.Id);

        result.Should().BeNull();

        var tasksCount = await _dbContext.Tasks.CountAsync();
        tasksCount.Should().Be(0); // Verify database is empty
    }


    [Fact]
    public async Task DeleteTaskAsync_IfNotExists_ShouldThrow()
    {
        var userId = Guid.NewGuid();

        await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();

        await Assert.ThrowsAsync<TaskNotFoundException>(
            async () =>
                await _taskService.DeleteTaskAsync(Guid.NewGuid(), userId));
    }

    [Fact]
    public async Task DeleteTaskAsync_IfNotOwner_ShouldThrow()
    {
        var userId = Guid.NewGuid();

        await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();

        await Assert.ThrowsAsync<UnauthorizedTaskAccessException>(
            async () =>
                await _taskService.DeleteTaskAsync(task.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateTaskAsync_WithValidData_ShouldUpdateTask()
    {
        var userId = Guid.NewGuid();

        await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();

        var newDate = DateTime.UtcNow.AddDays(1);
        var updateRequest = new UpdateTaskDto("UpdatedTitle", "new desc", newDate);

        var updatedTask = await _taskService.UpdateTaskAsync(task.Id, updateRequest, userId);

        updatedTask.Title.Should().Be("UpdatedTitle");
        updatedTask.Description.Should().Be("new desc");
        updatedTask.DueDate.Should().Be(newDate);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNoTitle_ShouldThrow()
    {
        var userId = Guid.NewGuid();

        await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();

        var newDate = DateTime.UtcNow.AddDays(1);
        var updateRequest = new UpdateTaskDto("", "new desc", newDate);

        await Assert.ThrowsAsync<ArgumentException>(
            async () =>
                await _taskService.UpdateTaskAsync(task.Id, updateRequest, userId));
    }

    [Fact]
    public async Task UpdateTaskAsync_IfNotOWner_ShouldThrowAuthException()
    {
        var userId = Guid.NewGuid();

        await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();

        var newDate = DateTime.UtcNow.AddDays(1);
        var updateRequest = new UpdateTaskDto("", "new desc", newDate);

        await Assert.ThrowsAsync<UnauthorizedTaskAccessException>(
            async () =>
                await _taskService.UpdateTaskAsync(task.Id, updateRequest, Guid.NewGuid()));
    }

    [Fact]
    public async Task MarkTaskAsCompletedAsync_IfValid_ShouldSetCompletedToTrue()
    {
        var userId = Guid.NewGuid();

        await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();

        var updatedTask = await _taskService.MarkTaskAsCompletedAsync(task.Id, userId);
        updatedTask.IsCompleted.Should().BeTrue();
        updatedTask.UpdatedAt.Should().NotBeNull();
    }


    [Fact]
    public async Task MarkTaskAsIncompletedAsync_IfValid_ShouldSetCompletedToFalse()
    {
        var userId = Guid.NewGuid();

        var result = await _dbContext.AddAsync(TaskItem.Create("Title", "Desc", userId, null));
        result.Entity.MarkAsCompleted();
        await _dbContext.SaveChangesAsync();

        var task = await _dbContext.Tasks.FirstOrDefaultAsync();
        task.Should().NotBeNull();
        task.IsCompleted.Should().BeTrue();

        var updatedTask = await _taskService.MarkTaskAsIncompleteAsync(task.Id, userId);
        updatedTask.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task MarkTaskAsCompletedAsync_WhenNotFound_ShouldThrow()
    {
        await Assert.ThrowsAsync<TaskNotFoundException>(
            async () => await _taskService.MarkTaskAsCompletedAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task MarkTaskAsCompletedAsync_WhenUnauthorized_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        var task = await CreateTaskInDb("Title", "Desc", userId);

        await Assert.ThrowsAsync<UnauthorizedTaskAccessException>(
            async () => await _taskService.MarkTaskAsCompletedAsync(task.Id, Guid.NewGuid()));
    }

    private async Task<TaskItem> CreateTaskInDb(string title, string description, Guid userId, DateTime? dueDate = null)
    {
        var task = TaskItem.Create(title, description, userId, dueDate);
        await _dbContext.AddAsync(task);
        await _dbContext.SaveChangesAsync();
        return task;
    }
}