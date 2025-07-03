using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Utility.Interface;
using NLog;

namespace UserMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IHttpClientFactory clientFactory, IGenericRepo<User> repo)
    : ControllerBase
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private HttpClient Client => clientFactory.CreateClient("UserClient");

    // BOOK
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;
            _logger.Info($"Request: {method} {path}");
            
            var result = await repo.GetAllAsync();

            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.Error(e, "GetUsers error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User data)
    {
        try
        {
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;
            _logger.Info($"Request: {method} {path}");

            var created = await repo.AddAsync(data);
            return CreatedAtAction(nameof(GetUsers), new { id = created.Id }, created);
        }
        catch (Exception e)
        {
            _logger.Error(e, "CreateUser error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User data)
    {
        try
        {
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;
            _logger.Info($"Request: {method} {path}");

            var updated = await repo.UpdateAsync(id, data);
            if (updated == null) return NotFound();

            return Ok(updated);
        }
        catch (Exception e)
        {
            _logger.Error(e, "UpdateUser error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;
            _logger.Info($"Request: {method} {path}");

            var deleted = await repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.Error(e, "DeleteUser error");
            return StatusCode(500, e.Message);
        }
    }
}
