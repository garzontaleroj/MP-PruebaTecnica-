using TaskManagement.Api.Models;

namespace TaskManagement.Api.Repositories;

public interface ITaskRepository
{
    Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<WorkTask>> GetAllAsync(
        Guid? userId,
        TaskState? status,
        CancellationToken cancellationToken);
    Task AddAsync(WorkTask task, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
