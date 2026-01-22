using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskTrackerApi.Models.Task> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Task entity
            modelBuilder.Entity<TaskTrackerApi.Models.Task>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.Priority)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.DueDate)
                    .IsRequired(false);
            });

            // Seed initial data
            modelBuilder.Entity<TaskTrackerApi.Models.Task>().HasData(
                new TaskTrackerApi.Models.Task
                {
                    Id = 1,
                    Title = "Setup project infrastructure",
                    Description = "Initialize ASP.NET Core project with EF Core",
                    Status = TaskTrackerApi.Models.TaskStatus.Done,
                    Priority = TaskPriority.High,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    DueDate = DateTime.UtcNow.AddDays(-2)
                },
                new TaskTrackerApi.Models.Task
                {
                    Id = 2,
                    Title = "Implement authentication",
                    Description = "Add JWT authentication to the API",
                    Status = TaskTrackerApi.Models.TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    DueDate = DateTime.UtcNow.AddDays(2)
                },
                new TaskTrackerApi.Models.Task
                {
                    Id = 3,
                    Title = "Create frontend UI",
                    Description = "Build React components for task management",
                    Status = TaskTrackerApi.Models.TaskStatus.New,
                    Priority = TaskPriority.Medium,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    DueDate = DateTime.UtcNow.AddDays(7)
                },
                new TaskTrackerApi.Models.Task
                {
                    Id = 4,
                    Title = "Write unit tests",
                    Description = "Add comprehensive test coverage for API endpoints",
                    Status = TaskTrackerApi.Models.TaskStatus.New,
                    Priority = TaskPriority.Low,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = null
                }
            );
        }
    }
}
