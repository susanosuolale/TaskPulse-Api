using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskPulse.Application.DTOs;

namespace TaskPulse.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetAllAsync();
    Task<TaskResponseDto> GetByIdAsync(Guid id);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    Task UpdateAsync(Guid id, UpdateTaskDto dto);
    Task DeleteAsync(Guid id);
}
