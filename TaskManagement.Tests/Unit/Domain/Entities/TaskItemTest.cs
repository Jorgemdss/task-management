
using FluentAssertions;
using TaskManagement.Domain.Models;
using Xunit;

namespace TaskManagement.Tests.Unit.Domain.Entities;

public class TaskItemTest
{
    [Fact]
    public void Create_WithValidData_ShouldCreate()
    {
        var title = "Some title";
        var description = "Some description";
        var userId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(2);

        var task = TaskItem.Create(title, description, userId, dueDate);

        task.Title.Should().Be(title);
        task.Description.Should().Be(description);
        task.UserId.Should().Be(userId);
        task.DueDate.Should().Be(dueDate);
        task.IsCompleted.Should().Be(false);
        task.UpdatedAt.Should().BeNull();
        task.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithoutTitle_ShouldThrow()
    {
        var title = "";
        var description = "Some description";
        var userId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(2);

        var result = Assert.Throws<ArgumentException>(
            () => TaskItem.Create(title, description, userId, dueDate));
    }

    [Fact]
    public void Create_WithNullDescription_ShouldCreateTaskWithEmptyDescription()
    {
        var title = "Some title";
        var description = "";
        var userId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(2);

        var task = TaskItem.Create(title, description, userId, dueDate);

        task.Title.Should().Be(title);
        task.Description.Should().BeEmpty();
        task.UserId.Should().Be(userId);
        task.DueDate.Should().Be(dueDate);
        task.IsCompleted.Should().Be(false);
        task.UpdatedAt.Should().BeNull();
        task.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdate()
    {
        var task = TaskItem.Create("First title", "First Description", Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        var newTitle = "some new title";
        var newDescription = "im new";
        var newDueDate = DateTime.UtcNow.AddDays(7);

        task.Update(newTitle, newDescription, newDueDate);

        task.Title.Should().Be(newTitle);
        task.Description.Should().Be(newDescription);
        task.DueDate.Should().Be(newDueDate);
    }

    [Fact]
    public void Update_WithoutTitle_ShouldThrow()
    {
        var task = TaskItem.Create("InitialTitle", "First Description", Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        var newTitle = "";
        var newDescription = "im new";
        var newDueDate = DateTime.UtcNow.AddDays(7);

        Assert.Throws<ArgumentException>(() =>
            task.Update(newTitle, newDescription, newDueDate)
        );
    }

    [Fact]
    public void MarkAsCompleted_ShouldSetIsCompletedToTrue()
    {
        var task = TaskItem.Create("First title", "First Description", Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        var dateBefore = DateTime.UtcNow;
        task.MarkAsCompleted();
        var dateAfter = DateTime.UtcNow;

        task.IsCompleted.Should().BeTrue();
        task.UpdatedAt.Should().NotBeNull();

        Assert.InRange(task.UpdatedAt.Value, dateBefore, dateAfter);

    }

    [Fact]
    public void MarkAsIncompleted_ShouldSetIsCompletedToFalse()
    {
        var task = TaskItem.Create("First title", "First Description", Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        task.MarkAsCompleted();

        var dateBefore = DateTime.UtcNow;
        task.MarkAsIncomplete();
        var dateAfter = DateTime.UtcNow;

        task.IsCompleted.Should().BeFalse();
        task.UpdatedAt.Should().NotBeNull();
        Assert.InRange(task.UpdatedAt.Value, dateBefore, dateAfter);

    }




}