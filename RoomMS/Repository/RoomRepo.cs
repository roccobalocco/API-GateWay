using EF.EF;
using EF.Models;
using Microsoft.EntityFrameworkCore;

namespace RoomMS.Repository;

public class RoomRepo(GatewayContext context)
{
    public async Task<List<Room>> GetAllAsync()
    {
        return await context.Rooms.ToListAsync();
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await context.Rooms.FindAsync(id);
    }

    public async Task<Room> AddAsync(Room book)
    {
        context.Rooms.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task<Room?> UpdateAsync(int id, Room book)
    {
        var existing = await context.Rooms.FindAsync(id);
        if (existing is null)
            return null;

        context.Entry(existing).CurrentValues.SetValues(book);
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Rooms.FindAsync(id);
        if (existing is null)
            return false;

        context.Rooms.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}