using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Utility.Interface;
using NLog;

namespace LoanMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController(IHttpClientFactory clientFactory, IGenericRepo<Loan> repo)
    : ControllerBase
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private HttpClient Client => clientFactory.CreateClient("LoanClient");

    // BOOK
    [HttpGet]
    public async Task<IActionResult> GetLoans()
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
            _logger.Error(e, "GetLoans error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateLoan([FromBody] Loan data)
    {
        try
        {
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;
            _logger.Info($"Request: {method} {path}");

            var created = await repo.AddAsync(data);
            return CreatedAtAction(nameof(GetLoans), new { id = created.Id }, created);
        }
        catch (Exception e)
        {
            _logger.Error(e, "CreateLoan error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLoan(int id, [FromBody] Loan data)
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
            _logger.Error(e, "UpdateLoan error");
            return StatusCode(500, e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLoan(int id)
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
            _logger.Error(e, "DeleteLoan error");
            return StatusCode(500, e.Message);
        }
    }
}
