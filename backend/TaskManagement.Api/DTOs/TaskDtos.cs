using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.DTOs;

public sealed record TaskMetadataDto(
    string? Priority,
    DateTimeOffset? EstimatedCompletionDate,
    IReadOnlyList<string>? Tags,
    Dictionary<string, JsonElement>? Metadata);

public sealed record CreateTaskRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [StringLength(2000)] string? Description,
    [Required] Guid UserId,
    TaskMetadataDto? AdditionalData);

public sealed record UpdateTaskStatusRequest(
    [Required, StringLength(20)] string Status);

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid UserId,
    string UserName,
    string Status,
    TaskMetadataDto? AdditionalData,
    DateTime CreatedAt);
