using Microsoft.AspNetCore.Mvc;
using EF.Models;

namespace LoanMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController(IHttpClientFactory clientFactory)
    : ControllerBase
{
    private HttpClient Client => clientFactory.CreateClient("LoanClient");

    // ROOM
    [HttpGet]
    public async Task<IActionResult> GetLoans()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<IActionResult> CreateLoan([FromBody] Loan data)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLoan(int id, [FromBody] Loan data)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        throw new NotImplementedException();
    }
}