using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Repositories;

public sealed class TaskRepository(TaskManagementDbContext dbContext) : ITaskRepository
{
    public Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tasks.Include(task => task.User)
            .SingleOrDefaultAsync(task => task.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkTask>> GetAllAsync(
        Guid? userId,
        TaskState? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Tasks.AsNoTracking().Include(task => task.User).AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(task => task.UserId == userId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(task => task.Status == status.Value);
        }

        return await query.OrderByDescending(task => task.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkTask task, CancellationToken cancellationToken)
    {
        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
