using ApiGateway.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using NLog;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatewayController(IHttpClientFactory clientFactory, IOptions<MicroServicesOptions> options)
    : ControllerBase
{
    private readonly MicroServicesOptions _services = options.Value;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private HttpClient Client => clientFactory.CreateClient("GatewayClient");

    // ROOM
    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms() =>
        await ProxyGet($"{_services.Room}/rooms");

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom([FromBody] JsonElement data) =>
        await ProxyPost($"{_services.Room}/rooms", data);

    [HttpPut("rooms/{id}")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] JsonElement data) =>
        await ProxyPut($"{_services.Room}/rooms/{id}", data);

    [HttpDelete("rooms/{id}")]
    public async Task<IActionResult> DeleteRoom(int id) =>
        await ProxyDelete($"{_services.Room}/rooms/{id}");

    // BOOK
    [HttpGet("books")]
    public async Task<IActionResult> GetBooks() =>
        await ProxyGet($"{_services.Book}/books");

    [HttpPost("books")]
    public async Task<IActionResult> CreateBook([FromBody] JsonElement data) =>
        await ProxyPost($"{_services.Book}/books", data);

    [HttpPut("books/{id}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] JsonElement data) =>
        await ProxyPut($"{_services.Book}/books/{id}", data);

    [HttpDelete("books/{id}")]
    public async Task<IActionResult> DeleteBook(int id) =>
        await ProxyDelete($"{_services.Book}/books/{id}");

    // LOAN
    [HttpGet("loans")]
    public async Task<IActionResult> GetLoans() =>
        await ProxyGet($"{_services.Loan}/loans");

    [HttpPost("loans")]
    public async Task<IActionResult> CreateLoan([FromBody] JsonElement data) =>
        await ProxyPost($"{_services.Loan}/loans", data);

    [HttpPut("loans/{id}")]
    public async Task<IActionResult> UpdateLoan(int id, [FromBody] JsonElement data) =>
        await ProxyPut($"{_services.Loan}/loans/{id}", data);

    [HttpDelete("loans/{id}")]
    public async Task<IActionResult> DeleteLoan(int id) =>
        await ProxyDelete($"{_services.Loan}/loans/{id}");

    // USER
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers() =>
        await ProxyGet($"{_services.User}/users");

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] JsonElement data) =>
        await ProxyPost($"{_services.User}/users", data);

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] JsonElement data) =>
        await ProxyPut($"{_services.User}/users/{id}", data);

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id) =>
        await ProxyDelete($"{_services.User}/users/{id}");

    // ---------- Proxy Helper Methods ----------
    private async Task<IActionResult> ProxyGet(string url)
    {
        LogRequest();
        try
        {
            var res = await Client.GetAsync(url);
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ProxyGet error");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProxyPost(string url, JsonElement data)
    {
        LogRequest();
        try
        {
            var res = await Client.PostAsync(url, new StringContent(data.ToString(), Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ProxyPost error");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProxyPut(string url, JsonElement data)
    {
        LogRequest();
        try
        {
            var res = await Client.PutAsync(url, new StringContent(data.ToString(), Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ProxyPut error");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProxyDelete(string url)
    {
        LogRequest();
        try
        {
            var res = await Client.DeleteAsync(url);
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ProxyDelete error");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private void LogRequest()
    {
        var method = HttpContext.Request.Method;
        var path = HttpContext.Request.Path;
        _logger.Info($"API Gateway Proxy Request: {method} {path}");
    }
}
