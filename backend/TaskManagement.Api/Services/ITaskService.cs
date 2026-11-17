using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(
        Guid? userId,
        string? status,
        CancellationToken cancellationToken);
    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TaskResponse> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken);
}
