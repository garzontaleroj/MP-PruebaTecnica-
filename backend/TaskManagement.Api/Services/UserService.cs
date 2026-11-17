using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Errors;
using TaskManagement.Api.Models;
using TaskManagement.Api.Repositories;

namespace TaskManagement.Api.Services;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim();
        if (name.Length == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "invalid_name", "Name is required.");
        }

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "email_already_exists",
                "A user with this email already exists.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        try
        {
            await userRepository.AddAsync(user, cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "email_already_exists",
                "A user with this email already exists.");
        }

        return ToResponse(user);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        (await userRepository.GetAllAsync(cancellationToken)).Select(ToResponse).ToList();

    private static UserResponse ToResponse(AppUser user) =>
        new(user.Id, user.Name, user.Email, user.CreatedAt);
}
