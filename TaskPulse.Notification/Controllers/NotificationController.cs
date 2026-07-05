using Microsoft.AspNetCore.Mvc;

namespace TaskPulse.Notification.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    // Other pieces of code running on the app like Azure Functions
    // can send messages to this controller to notify users of changes in the app
    [HttpPost]
    public ActionResult SendNotification([FromBody] NotificationRequest request)
    {
        // This is where we would normally connect to a real email company like Gmail or SendGrid.
        // For now, we will just pretend to send it to the screen so you can see it working!
        Console.WriteLine($"Pretending to send an email to: {request.EmailAddress}");
        Console.WriteLine($"Message: {request.MessageText}");

        return Ok("Notification sent successfully!");
        // In an Ideal situtation where we want to send the message over the internet to an actual email address. We use an Email Service Provider like SendGrid, AWS SES or others.
        // We would write a few lines of code to create a brand new web request. Inside this web request, we put the {request.EmailAddress}, the {request.MessageText}, and your secret SendGrid password (API Key).
        // Our NotificationController code sends this new web request directly over the internet to SendGrid's massive servers.
        // SendGrid takes over: SendGrid's servers receive our request and verify your secret password. 
        // Then, SendGrid uses its own giant systems to push the email directly into the user's real email inbox
    }
}

public class NotificationRequest
{
    public string EmailAddress { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
}
