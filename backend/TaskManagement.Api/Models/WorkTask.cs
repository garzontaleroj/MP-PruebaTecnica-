namespace TaskManagement.Api.Models;

public sealed class WorkTask
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public TaskState Status { get; set; }
    public string? AdditionalData { get; set; }
    public DateTime CreatedAt { get; set; }
}
