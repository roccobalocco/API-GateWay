using EF.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController(IHttpClientFactory clientFactory)
    : ControllerBase
{
    private HttpClient Client => clientFactory.CreateClient("BookClient");

    // BOOK
    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book data)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] Book data)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        throw new NotImplementedException();
    }
}