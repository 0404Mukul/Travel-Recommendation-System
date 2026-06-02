using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiRequest request)
        {
            var apiKey = _configuration["DeepSeek:ApiKey"];

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    apiKey
                );

            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a flight recommendation assistant. Recommend flights with flight number, source, destination, date and time."
                    },
                    new
                    {
                        role = "user",
                        content = request.Prompt
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var httpResponse = await client.PostAsync(
                "https://api.deepseek.com/chat/completions",
                content
            );

            var result = await httpResponse.Content.ReadAsStringAsync();

            return Ok(result);
        }
    }

    public class AiRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}