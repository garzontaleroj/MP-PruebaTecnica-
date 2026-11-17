using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
}
