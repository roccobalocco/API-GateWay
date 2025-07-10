using System.Security.Claims;
using ApiGateway.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using ApiGateway.Models;
using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NLog;

namespace ApiGateway.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GatewayController(
    IHttpClientFactory clientFactory,
    IOptions<MicroServicesOptions> options,
    IOptions<UsersAllowedOptions> usersAllowedOptionsAccessor)
    : ControllerBase
{
    private readonly MicroServicesOptions _services = options.Value;
    private readonly UsersAllowedOptions _usersAllowedOptions = usersAllowedOptionsAccessor.Value;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private HttpClient CreateClientForService(string serviceName) => serviceName switch
    {
        "Room" => clientFactory.CreateClient("RoomClient"),
        "Book" => clientFactory.CreateClient("BookClient"),
        "Loan" => clientFactory.CreateClient("LoanClient"),
        "User" => clientFactory.CreateClient("UserClient"),
        _ => throw new ArgumentException("Invalid service name")
    };

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginInformation request,
        [FromServices] IOptions<JWTOptions> jwtOptionsAccessor)
    {
        if (!_usersAllowedOptions.UsersAllowed.Any(user =>
                user.Username == request.Username && user.Password == request.Password))
            return Unauthorized("Invalid credentials");

        var jwtOptions = jwtOptionsAccessor.Value;
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtOptions.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            ]),
            Expires = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes),
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new LoginResponse { AccessToken = tokenString });
    }

    // ROOM
    [HttpGet("room")]
    public Task<IActionResult> GetRooms() =>
        ProxyGet($"{_services.Room}/room", "Room");

    [HttpPost("room")]
    public Task<IActionResult> CreateRoom([FromBody] Room data) =>
        ProxyPost($"{_services.Room}/room", data, "Room");

    [HttpPut("room/{id}")]
    public Task<IActionResult> UpdateRoom(int id, [FromBody] Room data) =>
        ProxyPut($"{_services.Room}/room/{id}", data, "Room");

    [HttpDelete("room/{id}")]
    public Task<IActionResult> DeleteRoom(int id) =>
        ProxyDelete($"{_services.Room}/room/{id}", "Room");

    // BOOK
    [HttpGet("book")]
    public Task<IActionResult> GetBooks() =>
        ProxyGet($"{_services.Book}/book", "Book");

    [HttpPost("book")]
    public Task<IActionResult> CreateBook([FromBody] Book data) =>
        ProxyPost($"{_services.Book}/book", data, "Book");

    [HttpPut("book/{id}")]
    public Task<IActionResult> UpdateBook(int id, [FromBody] Book data) =>
        ProxyPut($"{_services.Book}/book/{id}", data, "Book");

    [HttpDelete("book/{id:int}")]
    public Task<IActionResult> DeleteBook(int id) =>
        ProxyDelete($"{_services.Book}/book/{id}", "Book");

    // LOAN
    [HttpGet("loan")]
    public Task<IActionResult> GetLoans() =>
        ProxyGet($"{_services.Loan}/loan", "Loan");

    [HttpPost("loan")]
    public Task<IActionResult> CreateLoan([FromBody] Loan data) =>
        ProxyPost($"{_services.Loan}/loan", data, "Loan");

    [HttpPut("loan/{id:int}")]
    public Task<IActionResult> UpdateLoan(int id, [FromBody] Loan data) =>
        ProxyPut($"{_services.Loan}/loan/{id}", data, "Loan");

    [HttpDelete("loan/{id:int}")]
    public Task<IActionResult> DeleteLoan(int id) =>
        ProxyDelete($"{_services.Loan}/loan/{id}", "Loan");

    // USER
    [HttpGet("user")]
    public Task<IActionResult> GetUsers() =>
        ProxyGet($"{_services.User}/user", "User");

    [HttpPost("user")]
    public Task<IActionResult> CreateUser([FromBody] User data) =>
        ProxyPost($"{_services.User}/user", data, "User");

    [HttpPut("user/{id:int}")]
    public Task<IActionResult> UpdateUser(int id, [FromBody] User data) =>
        ProxyPut($"{_services.User}/user/{id}", data, "User");

    [HttpDelete("user/{id:int}")]
    public Task<IActionResult> DeleteUser(int id) =>
        ProxyDelete($"{_services.User}/user/{id}", "User");

    // ---------- Proxy Helper Methods ----------
    private async Task<IActionResult> ProxyGet(string url, string serviceName)
    {
        LogRequest();
        try
        {
            var client = CreateClientForService(serviceName);
            var res = await client.GetAsync(url);
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"ProxyGet error on {serviceName}");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProxyPost<T>(string url, T data, string serviceName)
    {
        LogRequest();
        try
        {
            var client = CreateClientForService(serviceName);
            var json = JsonSerializer.Serialize(data);
            var res = await client.PostAsync(url,
                new StringContent(json, Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"ProxyPost error on {serviceName}");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProxyPut<T>(string url, T data, string serviceName)
    {
        LogRequest();
        try
        {
            var client = CreateClientForService(serviceName);
            var json = JsonSerializer.Serialize(data);
            var res = await client.PutAsync(url,
                new StringContent(json, Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"ProxyPut error on {serviceName}");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProxyDelete(string url, string serviceName)
    {
        LogRequest();
        try
        {
            var client = CreateClientForService(serviceName);
            var res = await client.DeleteAsync(url);
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"ProxyDelete error on {serviceName}");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private void LogRequest()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var method = HttpContext.Request.Method;
        var path = HttpContext.Request.Path;
        var user = User.Identity?.Name ?? "anonymous";

        _logger.Info($"Request from IP={ip} User={user} Method={method} Path={path}");
    }
}
