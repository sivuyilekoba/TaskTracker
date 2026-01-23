using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.DTOs;
using TaskTrackerApi.Models;
using TaskTrackerApi.Services;

namespace TaskTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ApplicationDbContext context, ILogger<TaskController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        [HttpGet]
        [ProducesResponseType(typeof(List<TaskResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TaskResponseDto>>> GetAllTasks(
            [FromQuery] string? q,
            [FromQuery] string sort = "dueDate:asc")
        {
            try
            {
                var query = _context.Tasks.AsNoTracking().AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var searchTerm = q.ToLower();
                    query = query.Where(t => 
                        t.Title.ToLower().Contains(searchTerm) || 
                        t.Description.ToLower().Contains(searchTerm));
                }

                // Apply sorting
                query = sort.ToLower() switch
                {
                    "duedate:desc" => query.OrderByDescending(t => t.DueDate),
                    "duedate:asc" => query.OrderBy(t => t.DueDate),
                    _ => query.OrderBy(t => t.DueDate) // Default sorting
                };

                var tasks = await query.ToListAsync();

                var response = tasks.Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate
                }).ToList();

                _logger.LogInformation("Retrieved {Count} tasks", response.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tasks");
                return StatusCode(500, ex.Message);
            }
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTaskById(int id)
        {
            try
            {
                var task = await _context.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found", id);
                    return NotFound();
                }

                var response = new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status.ToString(),
                    Priority = task.Priority.ToString(),
                    CreatedAt = task.CreatedAt,
                    DueDate = task.DueDate
                };

                _logger.LogInformation("Retrieved task with ID {TaskId}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving task with ID {TaskId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        
        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] CreateTaskDto taskDto)
        {
            try
            {
                // Validate title
                if (string.IsNullOrWhiteSpace(taskDto.Title))
                {
                    return BadRequest();
                }

                // Validate and parse Status enum
                if (!Enum.TryParse<TaskTrackerApi.Models.TaskStatus>(taskDto.Status, true, out var status))
                {
                    return BadRequest(400);
                }

                // Validate and parse Priority enum
                if (!Enum.TryParse<TaskPriority>(taskDto.Priority, true, out var priority))
                {
                    return BadRequest(400);
                }

                // Validate DueDate if provided
                if (taskDto.DueDate.HasValue && taskDto.DueDate.Value.Kind != DateTimeKind.Utc)
                {
                    return BadRequest();
                }

                var task = new Models.Task
                {
                    Title = taskDto.Title,
                    Description = taskDto.Description ?? string.Empty,
                    Status = status,
                    Priority = priority,
                    DueDate = taskDto.DueDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                var response = new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status.ToString(),
                    Priority = task.Priority.ToString(),
                    CreatedAt = task.CreatedAt,
                    DueDate = task.DueDate
                };

                _logger.LogInformation("Created task with ID {TaskId}", task.Id);
                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating task");
                return StatusCode(500);
            }
        }

       
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, [FromBody] UpdateTaskDto taskDto)
        {
            try
            {
                var existingTask = await _context.Tasks.FindAsync(id);

                if (existingTask == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found for update", id);
                    return NotFound();
                }

                // Validate title
                if (string.IsNullOrWhiteSpace(taskDto.Title))
                {
                    return BadRequest();
                }

                // Validate and parse Status enum
                if (!Enum.TryParse<TaskTrackerApi.Models.TaskStatus>(taskDto.Status, true, out var status))
                {
                    return BadRequest();
                }

                // Validate and parse Priority enum
                if (!Enum.TryParse<TaskPriority>(taskDto.Priority, true, out var priority))
                {
                    return BadRequest();
                }

                // Validate DueDate if provided
                if (taskDto.DueDate.HasValue && taskDto.DueDate.Value.Kind != DateTimeKind.Utc)
                {
                    return BadRequest();
                }

                // Update properties
                existingTask.Title = taskDto.Title;
                existingTask.Description = taskDto.Description ?? string.Empty;
                existingTask.Status = status;
                existingTask.Priority = priority;
                existingTask.DueDate = taskDto.DueDate;

                _context.Tasks.Update(existingTask);
                await _context.SaveChangesAsync();

                var response = new TaskResponseDto
                {
                    Id = existingTask.Id,
                    Title = existingTask.Title,
                    Description = existingTask.Description,
                    Status = existingTask.Status.ToString(),
                    Priority = existingTask.Priority.ToString(),
                    CreatedAt = existingTask.CreatedAt,
                    DueDate = existingTask.DueDate
                };

                _logger.LogInformation("Updated task with ID {TaskId}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating task with ID {TaskId}", id);
                return StatusCode(500,ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteTask(int id)
        {
            try
            {
                var existingTask = await _context.Tasks.FindAsync(id);
                if (existingTask == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                    return false;
                }
                _context.Tasks.Remove(existingTask);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted task with ID {TaskId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting task");
                throw;
            }
        }

    }
}
