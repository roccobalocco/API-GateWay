using EF.EF;
using EF.Models;
using Microsoft.EntityFrameworkCore;
using Utility.Interface;

namespace UserMS.Repository;

public class UserRepo(GatewayContext context) : IGenericRepo<User>
{
    public async Task<List<User>> GetAllAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<User> AddAsync(User book)
    {
        context.Users.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task<User?> UpdateAsync(int id, User book)
    {
        var existing = await context.Users.FindAsync(id);
        if (existing is null)
            return null;

        context.Entry(existing).CurrentValues.SetValues(book);
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Users.FindAsync(id);
        if (existing is null)
            return false;

        context.Users.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}