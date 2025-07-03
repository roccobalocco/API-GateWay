using Microsoft.AspNetCore.Mvc;
using EF.Models;

namespace UserMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IHttpClientFactory clientFactory)
    : ControllerBase
{
    private HttpClient Client => clientFactory.CreateClient("UserClient");

    // ROOM
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User data)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User data)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        throw new NotImplementedException();
    }
}