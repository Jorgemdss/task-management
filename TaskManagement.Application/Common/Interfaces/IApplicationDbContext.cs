using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Models;

public interface IApplicationDbContext
{
    DbSet<TaskItem> Tasks {get;}

    Task<int> SaveChangesAsync(CancellationToken token = default);
}