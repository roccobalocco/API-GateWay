using Microsoft.AspNetCore.Mvc;
using Utility.Interface;

namespace Utility.Class;

[ApiController]
[Route("api/[controller]")]
public class GenericController<T>(IGenericRepo<T> repository) : ControllerBase
    where T : class
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await repository.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] T entity)
    {
        var created = await repository.AddAsync(entity);
        return CreatedAtAction(nameof(Get), new { id = GetEntityId(created) }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] T entity)
    {
        var updated = await repository.UpdateAsync(id, entity);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // Helper per recuperare l'id da un'entità, presuppone proprietà "Id"
    private static object? GetEntityId(T entity)
    {
        var prop = typeof(T).GetProperty("Id");
        return prop?.GetValue(entity);
    }
}