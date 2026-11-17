using System.Text.Json;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Errors;
using TaskManagement.Api.Models;
using TaskManagement.Api.Repositories;

namespace TaskManagement.Api.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    IUserRepository userRepository) : ITaskService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TaskResponse> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        if (title.Length == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "invalid_title", "Title is required.");
        }

        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "user_not_found",
                "The assigned user does not exist.");
        }

        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = request.Description?.Trim(),
            UserId = user.Id,
            User = user,
            Status = TaskState.Pending,
            AdditionalData = request.AdditionalData is null
                ? null
                : JsonSerializer.Serialize(request.AdditionalData, JsonOptions),
            CreatedAt = DateTime.UtcNow
        };

        await taskRepository.AddAsync(task, cancellationToken);
        return ToResponse(task);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(
        Guid? userId,
        string? status,
        CancellationToken cancellationToken)
    {
        TaskState? parsedStatus = string.IsNullOrWhiteSpace(status) ? null : ParseStatus(status);
        var tasks = await taskRepository.GetAllAsync(userId, parsedStatus, cancellationToken);
        return tasks.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "task_not_found", "Task not found.");
        return ToResponse(task);
    }

    public async Task<TaskResponse> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "task_not_found", "Task not found.");
        var nextStatus = ParseStatus(status);

        if (task.Status == TaskState.Pending && nextStatus == TaskState.Done)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "invalid_status_transition",
                "A task cannot move directly from Pending to Done. Move it to InProgress first.");
        }

        task.Status = nextStatus;
        await taskRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(task);
    }

    private static TaskState ParseStatus(string status)
    {
        if (Enum.TryParse<TaskState>(status, true, out var parsed)
            && Enum.IsDefined(typeof(TaskState), parsed))
        {
            return parsed;
        }

        throw new ApiException(
            StatusCodes.Status400BadRequest,
            "invalid_status",
            "Status must be Pending, InProgress, or Done.");
    }

    private static TaskResponse ToResponse(WorkTask task)
    {
        var metadata = task.AdditionalData is null
            ? null
            : JsonSerializer.Deserialize<TaskMetadataDto>(task.AdditionalData, JsonOptions);

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.UserId,
            task.User.Name,
            task.Status.ToString(),
            metadata,
            task.CreatedAt);
    }
}
