using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.Services;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IEventPublisher _publisher;

    public TestController(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendTestMessage()
    {
        await _publisher.PublishEvent("Backend API Development", " Hi I am Riaz Khan , Test message from API!");
        return Ok("Message sent!");
    }
}
