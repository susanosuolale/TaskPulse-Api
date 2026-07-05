using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using TaskPulse.Application.DTOs;
using TaskPulse.Application.Interfaces;
using TaskPulse.Domain.Entities;

namespace TaskPulseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    //Fast memory for data storage
    private readonly IDistributedCache _cache;
    private const string CacheKey = "GetAllTasks";

    public TasksController(ITaskRepository taskRepository, IDistributedCache cache)
    {
        _taskRepository = taskRepository;
        _cache = cache;
    }

    [HttpGet]
    // lets a user pass specific instructions to filter or sort data
    [EnableQuery]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll()
    {
        // This line connects to the Redis server and asks a direct question: \
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
                return Ok(deserialized);
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

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponseDto>> GetById(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var dto = new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateTaskDto dto)
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
        try
        {
            // the queueClient is the literal C# object that establishes the network 
            // connection to Azure Storage with destination being new-tasks-queue 
            // and delivers the message
            var queueClient = new QueueClient("UseDevelopmentStorage=true", "new-tasks-queue");
            
            // 2. Make sure the bucket actually exists
            await queueClient.CreateIfNotExistsAsync();
            
            // 3. Create the text note
            var noteText = $"A new task called '{task.Title}' was created!";
            
            // 4. Drop the note into the bucket
            await queueClient.SendMessageAsync(noteText);
        }
        catch (Exception ex)
        {
            // If the local Storage Simulator is turned off, catch the error and do nothing
            // so the app does not crash for the user.
            Console.WriteLine($"Could not drop note into bucket. Error: {ex.Message}");
        }
        // -----------------------------------------------------

        var responseDto = new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, responseDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateTaskDto dto)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            return NotFound();
        }

        existingTask.Title = dto.Title;
        existingTask.Description = dto.Description;
        existingTask.IsCompleted = dto.IsCompleted;

        await _taskRepository.UpdateAsync(existingTask);
        await _cache.RemoveAsync(CacheKey);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            return NotFound();
        }

        await _taskRepository.DeleteAsync(id);
        await _cache.RemoveAsync(CacheKey);

        return NoContent();
    }
}
