using TaskTrackerApi.DTOs;

namespace TaskTrackerApi.Services
{
    public interface ITaskService
    {
        Task <List<TaskResponseDto>> GetAllTasksAsync();
        Task <TaskResponseDto?> GetTaskByIdAsync(int id);
        Task <CreateTaskDto> CreateTaskAsync(CreateTaskDto task);
        Task <UpdateTaskDto> UpdateTaskAsync(int id, UpdateTaskDto task);
        Task <bool> DeleteTaskAsync(int id);

    }
}
