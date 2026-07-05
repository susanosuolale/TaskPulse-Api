using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
// azure functions watches the new-tasks-queue for new items dropped into the "new-tasks-queue" cloud bucket
// and triggers the method when there is a new item
// It reads the message and sends it to the Notification Microservice

namespace TaskPulse.Functions;

public class TaskNotifier
{
    private readonly ILogger _logger;

    public TaskNotifier(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TaskNotifier>();
    }
    // When your main API saves a new task, it drops a small text note into a digital cloud bucket called new-tasks-queue
    // This is the alarm clock! It wakes up automatically when a new message appears in the "new-tasks-queue"
    [Function("TaskNotifier")]
    public async Task Run([QueueTrigger("new-tasks-queue")] string myQueueItem)
    {
        _logger.LogInformation($"New task detected in the queue: {myQueueItem}");
        
        // 1. Create the data package
        var dataPackage = new 
        {
            EmailAddress = "user@example.com",
            MessageText = myQueueItem
        };

        // 2. Convert the data package into JSON text format
        var jsonText = JsonSerializer.Serialize(dataPackage);
        var content = new StringContent(jsonText, Encoding.UTF8, "application/json");

        // 3. Create the tool that sends web requests
        using var httpClient = new HttpClient();

        _logger.LogInformation("Sending web request to Notification Microservice...");
        
        // 4. Send the JSON text over the network directly to the Notification Microservice
        // Note: localhost means "this exact computer", and we guess port 5001 for now.
        var response = await httpClient.PostAsync("https://localhost:5001/api/notification", content);
        
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("The Notification Microservice successfully received the data!");
        }
    }
}
