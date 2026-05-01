namespace TaskManagement.Application.Common;

public static class CacheKeys
{
    public static string Task(Guid id) => $"task:{id}";

    public static string TaskList(Guid userId) => $"tasks:user:{userId}";
}
