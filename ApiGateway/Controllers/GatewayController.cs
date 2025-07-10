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
[Authorize] // Tutti gli endpoint richiedono token JWT valido
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

    private HttpClient Client => clientFactory.CreateClient("GatewayClient");

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
    public async Task<IActionResult> GetRooms() =>
        await ProxyGet($"{_services.Room}/room");

    [HttpPost("room")]
    public async Task<IActionResult> CreateRoom([FromBody] Room data) =>
        await ProxyPost($"{_services.Room}/room", data);

    [HttpPut("room/{id}")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room data) =>
        await ProxyPut($"{_services.Room}/room/{id}", data);

    [HttpDelete("room/{id}")]
    public async Task<IActionResult> DeleteRoom(int id) =>
        await ProxyDelete($"{_services.Room}/room/{id}");

    // BOOK
    [HttpGet("book")]
    public async Task<IActionResult> GetBooks() =>
        await ProxyGet($"{_services.Book}/book");

    [HttpPost("book")]
    public async Task<IActionResult> CreateBook([FromBody] Book data) =>
        await ProxyPost($"{_services.Book}/book", data);

    [HttpPut("book/{id}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] Book data) =>
        await ProxyPut($"{_services.Book}/book/{id}", data);

    [HttpDelete("book/{id:int}")]
    public async Task<IActionResult> DeleteBook(int id) =>
        await ProxyDelete($"{_services.Book}/book/{id}");

    // LOAN
    [HttpGet("loan")]
    public async Task<IActionResult> GetLoans() =>
        await ProxyGet($"{_services.Loan}/loan");

    [HttpPost("loan")]
    public async Task<IActionResult> CreateLoan([FromBody] Loan data) =>
        await ProxyPost($"{_services.Loan}/loan", data);

    [HttpPut("loan/{id:int}")]
    public async Task<IActionResult> UpdateLoan(int id, [FromBody] Loan data) =>
        await ProxyPut($"{_services.Loan}/loan/{id}", data);

    [HttpDelete("loan/{id:int}")]
    public async Task<IActionResult> DeleteLoan(int id) =>
        await ProxyDelete($"{_services.Loan}/loan/{id}");

    // USER
    [HttpGet("user")]
    public async Task<IActionResult> GetUsers() =>
        await ProxyGet($"{_services.User}/user");

    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody] User data) =>
        await ProxyPost($"{_services.User}/user", data);

    [HttpPut("user/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User data) =>
        await ProxyPut($"{_services.User}/user/{id}", data);

    [HttpDelete("user/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id) =>
        await ProxyDelete($"{_services.User}/user/{id}");

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

    private async Task<IActionResult> ProxyPost<T>(string url, T data)
    {
        LogRequest();
        try
        {
            var json = JsonSerializer.Serialize(data); // <-- serialize properly
            var res = await Client.PostAsync(url,
                new StringContent(json, Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, content);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ProxyPost error");
            return StatusCode(503, $"Service unavailable: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProxyPut<T>(string url, T data)
    {
        LogRequest();
        try
        {
            var json = JsonSerializer.Serialize(data); // <-- serialize properly
            var res = await Client.PostAsync(url,
                new StringContent(json, Encoding.UTF8, "application/json"));
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