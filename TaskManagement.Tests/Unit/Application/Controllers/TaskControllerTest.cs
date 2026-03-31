
using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Api.Controllers;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Infrastructure.AuthHandlers;
using Xunit;

namespace TaskManagement.Tests.Unit.Application.Controllers;

public class TaskControllerTest
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ILogger<TaskController>> _loggerMock;

    private readonly TaskController _controller;
    private readonly Guid _testUserId;


    public TaskControllerTest()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _loggerMock = new Mock<ILogger<TaskController>>();
        _controller = new TaskController(_loggerMock.Object, _taskServiceMock.Object);

        _testUserId = Guid.NewGuid();
        SetupSimpleUserClaims(_testUserId);
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturn201Created()
    {
        var createRequest = new CreateTaskDto(
            "Some title",
            "Some description",
            DateTime.UtcNow.AddDays(2)
        );

        var expectedTaskDto = new TaskDto(
            Guid.NewGuid(),
            _testUserId,
            createRequest.Title,
            createRequest.Description,
            false,
            DateTime.UtcNow,
            createRequest.DueDate,
            null
        );

        // Use the mocked the service, assume it will return a valid value
        _taskServiceMock
            .Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>(), _testUserId))
            .ReturnsAsync(expectedTaskDto);

        var result = await _controller.CreateTask(createRequest);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().BeEquivalentTo(expectedTaskDto);

        _taskServiceMock.Verify(
            s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>(), _testUserId),
            Times.Once);
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_ShouldThrow()
    {
        var createRequest = new CreateTaskDto(
            "",
            "Some description",
            DateTime.UtcNow.AddDays(2)
        );

        _taskServiceMock
            .Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>(), _testUserId))
            .Throws(new ArgumentException());

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _controller.CreateTask(createRequest));

        _taskServiceMock.Verify(
            s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>(), _testUserId),
            Times.Once);
    }


    [Fact]
    public async Task GetTask_WithValidId_ShouldReturn200Ok()
    {
        var taskId = Guid.NewGuid();

        var expectedTaskDto = new TaskDto(
            taskId,
            _testUserId,
            "someTitle",
            "someDescription",
            false,
            DateTime.UtcNow,
            null,
            null
        );

        _taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), _testUserId))
            .ReturnsAsync(expectedTaskDto);


        var operation = await _controller.GetTask(taskId);
        var result = operation.Should().BeOfType<OkObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.Value.Should().BeEquivalentTo(expectedTaskDto);

        _taskServiceMock.Verify(s =>
            s.GetTaskByIdAsync(It.IsAny<Guid>(), _testUserId),
            Times.Once);
    }

    [Fact]
    public async Task GetTask_WhenTaskNotFound_ShouldReturn404()
    {
        var taskId = Guid.NewGuid();

        _taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), _testUserId))
            .Throws(new TaskNotFoundException(taskId));

        await Assert.ThrowsAsync<TaskNotFoundException>(
            async () => await _controller.GetTask(taskId));

        _taskServiceMock.Verify(s =>
            s.GetTaskByIdAsync(It.IsAny<Guid>(), _testUserId),
            Times.Once);
    }


    [Fact]
    public async Task GetUserTasks_ShouldReturn200Ok()
    {
        var expectedTasks = new List<TaskDto>
        {
            new (Guid.NewGuid(), _testUserId, "Some title", "Some Description", false, DateTime.UtcNow, null,null),
            new (Guid.NewGuid(), _testUserId, "Some title 2", "AAAA", false, DateTime.UtcNow, null,null),
        };

        _taskServiceMock.Setup(s =>
            s.GetUserTasksAsync(_testUserId))
            .ReturnsAsync(expectedTasks);

        var operation = await _controller.GetUserTasks();

        var result = operation.Should().BeOfType<OkObjectResult>().Subject;

        result.Value.Should().BeEquivalentTo(expectedTasks);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ShouldReturn200Ok()
    {
        var updatedAtDate = DateTime.UtcNow.AddSeconds(2);

        var taskTittle = "Bug 333";
        var newDescription = "UI bug when clicking 'retry' button";
        var originalTask = new TaskDto(Guid.NewGuid(), _testUserId, taskTittle, "UI bug when clicking 'cancel' button", false, DateTime.UtcNow, null, null);

        var updateRequest = new UpdateTaskDto(taskTittle, newDescription, null);
        var expectedTask = new TaskDto(originalTask.Id, _testUserId, taskTittle, newDescription, false, DateTime.UtcNow, null, updatedAtDate);

        _taskServiceMock.Setup(s =>
            s.UpdateTaskAsync(originalTask.Id, updateRequest, _testUserId))
            .ReturnsAsync(expectedTask);

        var operation = await _controller.UpdateTask(originalTask.Id, updateRequest);

        var result = operation.Should().BeOfType<OkObjectResult>().Subject;
        result.Value.Should().BeEquivalentTo(expectedTask);

        result.Value.As<TaskDto>().UpdatedAt.Should().Be(updatedAtDate);

        _taskServiceMock.Verify(s =>
            s.UpdateTaskAsync(originalTask.Id, updateRequest, _testUserId),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WhenNotFound_ShouldThrow()
    {
        var taskId = Guid.NewGuid();

        _taskServiceMock.Setup(s =>
            s.UpdateTaskAsync(It.IsAny<Guid>(), It.IsAny<UpdateTaskDto>(), _testUserId))
            .ThrowsAsync(new TaskNotFoundException(taskId));

        var updateRequest = new UpdateTaskDto("taskTittle", "Description", null);
        await Assert.ThrowsAsync<TaskNotFoundException>(
            async () => await _controller.UpdateTask(taskId, updateRequest));

        _taskServiceMock.Verify(s =>
            s.UpdateTaskAsync(It.IsAny<Guid>(), It.IsAny<UpdateTaskDto>(), _testUserId),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTask_WhenSuccessful_ShouldReturn204()
    {
        var taskId = Guid.NewGuid();

        _taskServiceMock
            .Setup(s => s.DeleteTaskAsync(It.IsAny<Guid>(), _testUserId))
            .Returns(Task.CompletedTask);

        var operation = await _controller.DeleteTask(taskId);
        operation.Should().BeOfType<NoContentResult>();

        _taskServiceMock.Verify(s =>
            s.DeleteTaskAsync(It.IsAny<Guid>(), _testUserId),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var taskId = Guid.NewGuid();

        _taskServiceMock
            .Setup(s => s.DeleteTaskAsync(It.IsAny<Guid>(), _testUserId))
            .ThrowsAsync(new TaskNotFoundException(taskId));

        await Assert.ThrowsAsync<TaskNotFoundException>(
            async () => await _controller.DeleteTask(taskId));
    }

    [Fact]
    public async Task MarkCompleted_WhenSuccessful_ShouldReturn200()
    {
        var taskId = Guid.NewGuid();

        var updatedAt = DateTime.UtcNow.AddSeconds(1);
        var expectedTask = new TaskDto(taskId, _testUserId, "Title", "Desc", true, DateTime.UtcNow, null, updatedAt);

        _taskServiceMock
            .Setup(s => s.MarkTaskAsCompletedAsync(It.IsAny<Guid>(), _testUserId))
            .ReturnsAsync(expectedTask);

        var operation = await _controller.MarkAsCompleted(taskId);
        var result = operation.Should().BeOfType<OkObjectResult>().Subject;
        result.Value.Should().BeEquivalentTo(expectedTask);

        _taskServiceMock.Verify(
            s => s.MarkTaskAsCompletedAsync(It.IsAny<Guid>(), _testUserId), Times.Once);
    }

    [Fact]
    public async Task MarkIncompleted_WhenSuccessful_ShouldReturn200()
    {
        var taskId = Guid.NewGuid();

        var updatedAt = DateTime.UtcNow.AddSeconds(1);
        var expectedTask = new TaskDto(taskId, _testUserId, "Title", "Desc", false, DateTime.UtcNow, null, updatedAt);

        _taskServiceMock
            .Setup(s => s.MarkTaskAsIncompleteAsync(It.IsAny<Guid>(), _testUserId))
            .ReturnsAsync(expectedTask);

        var operation = await _controller.MarkAsIncomplete(taskId);
        var result = operation.Should().BeOfType<OkObjectResult>().Subject;
        result.Value.Should().BeEquivalentTo(expectedTask);

        _taskServiceMock.Verify(
            s => s.MarkTaskAsIncompleteAsync(It.IsAny<Guid>(), _testUserId), Times.Once);
    }


    private void SetupSimpleUserClaims(Guid userId)
    {
        // Create the claims that would normally come from the JWT
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userId.ToString()),
            new (ClaimTypes.Email, "test@example.com"),
            new (ClaimTypes.Role, Role.UserRole)
        };

        // Creat an identity with our claims and give it a name "TestAuth"
        var identity = new ClaimsIdentity(claims, "TestAuth");

        // Wrap the entity in the principal
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Inject the HttpContext with your principal because in a test scenario HttpContext is null
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    // TODO JS: admin user
    // private void SetupAdminUserClaims(Guid userId)
    // {
    //     // Create the claims that would normally come from the JWT
    //     var claims = new List<Claim>
    //     {
    //         new (ClaimTypes.NameIdentifier, userId.ToString()),
    //         new (ClaimTypes.Email, "test@example.com"),
    //         new (ClaimTypes.Role, Role.AdminRole)
    //     };

    //     // Creat an identity with our claims and give it a name "TestAuth"
    //     var identity = new ClaimsIdentity(claims, "TestAuth");

    //     // Wrap the entity in the principal
    //     var claimsPrincipal = new ClaimsPrincipal(identity);

    //     // Inject the HttpContext with your principal because in a test scenario HttpContext is null
    //     _controller.ControllerContext = new ControllerContext
    //     {
    //         HttpContext = new DefaultHttpContext { User = claimsPrincipal }
    //     };
    // }
}