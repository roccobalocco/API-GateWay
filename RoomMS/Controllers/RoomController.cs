using Microsoft.AspNetCore.Mvc;
using EF.Models;

namespace RoomMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController(IHttpClientFactory clientFactory)
    : ControllerBase
{
    private HttpClient Client => clientFactory.CreateClient("RoomClient");

    // ROOM
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] Room data)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room data)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        throw new NotImplementedException();
    }
}