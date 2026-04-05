namespace TaskManagement.Domain.Models;

public class TaskItem : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid UserId { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string title, string description, Guid userId, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty", nameof(title));
        }

        return new TaskItem()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description ?? string.Empty,
            UserId = userId,
            DueDate = dueDate,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(string title, string description, DateTime? duedate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty", nameof(title));
        }
        Title = title;
        Description = description;
        DueDate = duedate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsIncomplete()
    {
        IsCompleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
