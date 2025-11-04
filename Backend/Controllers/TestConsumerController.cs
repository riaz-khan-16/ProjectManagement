using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.Services;

namespace ProjectManagementAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TestConsumerController:ControllerBase
    {

        private readonly IEventConsumer _consumer;
        public  TestConsumerController( IEventConsumer consumer)
        {

            _consumer = consumer;
        }

        [HttpGet("start")]
        public async Task<IActionResult> StartListening()
        {
            _ = _consumer.StartListeningAsync("Backend API Development"); // queue name
            return Ok("Consumer started and listening...");
        }


    }
}
