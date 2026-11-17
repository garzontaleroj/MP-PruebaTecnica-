namespace TaskManagement.Api.Models;

public sealed class AppUser
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<WorkTask> Tasks { get; set; } = [];
}
