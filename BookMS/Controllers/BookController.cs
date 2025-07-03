using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Utility.Interface;
using NLog;

namespace BookMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController(IHttpClientFactory clientFactory, IGenericRepo<Book> repo)
    : ControllerBase
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private HttpClient Client => clientFactory.CreateClient("BookClient");

    // BOOK
    [HttpGet]
    public async Task<IActionResult> GetBooks()
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
            _logger.Error(e, "GetBooks error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book data)
    {
        try
        {
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;
            _logger.Info($"Request: {method} {path}");

            var created = await repo.AddAsync(data);
            return CreatedAtAction(nameof(GetBooks), new { id = created.Id }, created);
        }
        catch (Exception e)
        {
            _logger.Error(e, "CreateBook error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] Book data)
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
            _logger.Error(e, "UpdateBook error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
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
            _logger.Error(e, "DeleteBook error");
            return StatusCode(500, e.Message);
        }
    }
}
