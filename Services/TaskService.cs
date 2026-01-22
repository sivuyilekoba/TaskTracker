using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.DTOs;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TaskResponseDto>> GetAllTasksAsync()
        {
            try
            {
                var tasks = await _context.Tasks
                    .AsNoTracking()
                    .ToListAsync();

                return tasks.Select(MapToResponseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all tasks");
                throw;
            }
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int id)
        {
            try
            {
                var task = await _context.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                return task == null ? null : MapToResponseDto(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving task with ID {TaskId}", id);
                throw;
            }
        }

        public async Task<CreateTaskDto> CreateTaskAsync(CreateTaskDto taskDto)
        {
            try
            {
                var task = new Models.Task
                {
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    Status = Enum.Parse<TaskTrackerApi.Models.TaskStatus>(taskDto.Status, ignoreCase: true),
                    Priority = Enum.Parse<TaskPriority>(taskDto.Priority, ignoreCase: true),
                    DueDate = taskDto.DueDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task created successfully with ID {TaskId}", task.Id);

                taskDto.Id = task.Id;
                taskDto.CreatedAt = task.CreatedAt;
                return taskDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new task");
                throw;
            }
        }

        public async Task<UpdateTaskDto> UpdateTaskAsync(int id, UpdateTaskDto taskDto)
        {
            try
            {
                var existingTask = await _context.Tasks.FindAsync(id);
                
                if (existingTask == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found for update", id);
                    throw new KeyNotFoundException($"Task with ID {id} not found.");
                }

                // Update properties
                existingTask.Title = taskDto.Title;
                existingTask.Description = taskDto.Description;
                existingTask.Status = Enum.Parse<TaskTrackerApi.Models.TaskStatus>(taskDto.Status, ignoreCase: true);
                existingTask.Priority = Enum.Parse<TaskPriority>(taskDto.Priority, ignoreCase: true);
                existingTask.DueDate = taskDto.DueDate;

                _context.Tasks.Update(existingTask);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task with ID {TaskId} updated successfully", id);

                taskDto.Id = existingTask.Id;
                taskDto.CreatedAt = existingTask.CreatedAt;
                return taskDto;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating task with ID {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                
                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                    return false;
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task with ID {TaskId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting task with ID {TaskId}", id);
                throw;
            }
        }

        // Helper method to map Task entity to TaskResponseDto
        private static TaskResponseDto MapToResponseDto(Models.Task task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                Priority = task.Priority.ToString(),
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate
            };
        }
    }
}
