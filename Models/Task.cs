using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.Models
{
    public enum TaskStatus
    {
        New,
        InProgress,
        Done
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.New;

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Low;

        public DateTime? DueDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
