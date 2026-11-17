using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public sealed record CreateUserRequest(
    [Required, StringLength(120, MinimumLength = 1)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email);

public sealed record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAt);
