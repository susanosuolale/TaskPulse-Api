using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Caching.Distributed;
using TaskPulse.Application.DTOs;
using TaskPulse.Application.Interfaces;
using TaskPulse.Domain.Entities;

namespace TaskPulse.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    //Fast memory for data storage
    private readonly IDistributedCache _cache;
    private const string CacheKey = "GetAllTasks";

    public TaskService(ITaskRepository taskRepository, IDistributedCache cache)
    {
        _taskRepository = taskRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync()
    {
        // This line connects to the Redis server and asks a direct question: 
        // "Do you have any data saved under the name "GetAllTasks"?"
        var cachedTasks = await _cache.GetStringAsync(CacheKey);
        
        // If the answer is yes, and the data is not empty, it retrieves and returns the data to the frontend immediately.
        if (!string.IsNullOrEmpty(cachedTasks))
        {
            // takes the raw json text and converts into a c# object(a list of <Taskitem>)
            var deserialized = JsonSerializer.Deserialize<IEnumerable<TaskResponseDto>>(cachedTasks);
            if (deserialized != null)
            {
                // returns the list of <Taskitem>
                return deserialized;
            }
        }

        var tasks = await _taskRepository.GetAllAsync();
        var dtos = tasks.Select(t => new TaskResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt
        });
        
        //  converts the list of <TaskResponseDto> into a json string
        var serialized = JsonSerializer.Serialize(dtos);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        // _cache.SetStringAsync saves the JSON text in Redis using the name 
        // "GetAllTasks" and applies the 5-minute time limit
        await _cache.SetStringAsync(CacheKey, serialized, cacheOptions);

        return dtos;
    }

    public async Task<TaskResponseDto> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return null; // The controller will handle the NotFound logic
        }

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        // after any changes is made, RemoveAsync command connects to Redis 
        // and immediately deletes the text stored under the name "GetAllTasks"
        await _cache.RemoveAsync(CacheKey);

        // --- NEW CODE: Drop the note into the cloud bucket ---
        // (Commented out because the local Azure Simulator is off, which causes a 10 second delay!)
        /*
        try
        {
            var queueClient = new QueueClient("UseDevelopmentStorage=true", "new-tasks-queue");
            await queueClient.CreateIfNotExistsAsync();
            var noteText = $"A new task called '{task.Title}' was created!";
            await queueClient.SendMessageAsync(noteText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not drop note into bucket. Error: {ex.Message}");
        }
        */
        // -----------------------------------------------------

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };
    }

    public async Task UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            throw new Exception("Task not found"); // Will be caught or checked later
        }

        existingTask.Title = dto.Title;
        existingTask.Description = dto.Description;
        existingTask.IsCompleted = dto.IsCompleted;

        await _taskRepository.UpdateAsync(existingTask);
        await _cache.RemoveAsync(CacheKey);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            throw new Exception("Task not found"); // Will be caught or checked later
        }

        await _taskRepository.DeleteAsync(id);
        await _cache.RemoveAsync(CacheKey);
    }
}
