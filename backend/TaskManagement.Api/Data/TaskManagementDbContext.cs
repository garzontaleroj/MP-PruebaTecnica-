using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data;

public sealed class TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Name).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<WorkTask>(entity =>
        {
            entity.ToTable("Tasks", table =>
                table.HasCheckConstraint(
                    "CK_Tasks_AdditionalData_IsJson",
                    "[AdditionalData] IS NULL OR ISJSON([AdditionalData]) = 1"));
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Title).HasMaxLength(200).IsRequired();
            entity.Property(task => task.Description).HasMaxLength(2000);
            entity.Property(task => task.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();
            entity.Property(task => task.AdditionalData).HasColumnType("nvarchar(max)");
            entity.Property(task => task.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(task => task.User)
                .WithMany(user => user.Tasks)
                .HasForeignKey(task => task.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(task => new { task.UserId, task.Status, task.CreatedAt });
        });
    }
}
