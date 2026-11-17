using TaskManagement.Api.Models;

namespace TaskManagement.Api.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(AppUser user, CancellationToken cancellationToken);
}
