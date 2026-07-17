using Microsoft.AspNetCore.Mvc;
using TaskPulse.Application.DTOs;
using TaskPulse.Application.Interfaces;

// Because simple HTML literally cannot send a PUT or a 
// DELETE signal, MVC applications are forced to use POST
//  for almost everything that changes data

namespace TaskPulseApi.Controllers;

// 1. Removed [ApiController] and [Route]
// 2. Changed from ControllerBase to Controller
public class TasksController : Controller
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // 3. Changed name from GetAll to Index (standard for web pages)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var dtos = await _taskService.GetAllAsync();
        
        // 4. Return View instead of Ok
        // This tells your application to find a web page and 
        // send your tasks to it.
        // Because your controller is named Tasks and the method is named Index, 
        // the application is smart. It automatically knows to go look for a file at Views/Tasks/Index.cshtml
        return View(dtos);
    }

    [HttpGet]
    public async Task<IActionResult> GetById(System.Guid id)
    {
        var dto = await _taskService.GetByIdAsync(id);
        if (dto == null)
        {
            return NotFound();
        }
        //the method name is bypassed here by passing the name of the view to the View() method
        return View("Details", dto); // Return the beautiful Details web page!
    }

    // --- STEP 1: Show the empty Create form ---
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskDto dto)
    {
        var responseDto = await _taskService.CreateAsync(dto);
        return RedirectToAction(nameof(Index)); // Go back to the list after creating
    }

    // --- STEP 1: Show the Update form with the old data ---
    [HttpGet]
    public async Task<IActionResult> Update(System.Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var updateDto = new UpdateTaskDto
        {
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted
        };

        return View(updateDto);
    }

    // --- STEP 2: Save the updated data using POST ---
    [HttpPost]
    public async Task<IActionResult> Update(System.Guid id, UpdateTaskDto dto)
    {
        try
        {
            await _taskService.UpdateAsync(id, dto);
            return RedirectToAction(nameof(Index));
        }
        catch (System.Exception)
        {
            return NotFound();
        }
    }

    // --- Receive the delete command from the tiny form ---
    [HttpPost]
    public async Task<IActionResult> Delete(System.Guid id)
    {
        try
        {
            await _taskService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (System.Exception ex)
        {
            // If it fails, log it and go back to index anyway so the user doesn't get stuck
            Console.WriteLine("Delete failed: " + ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}
