using EF.EF;
using EF.Models;
using Microsoft.EntityFrameworkCore;
using Utility.Interface;

namespace BookMS.Repository;

public class BookRepo(GatewayContext context) : IGenericRepo<Book>
{
    public async Task<List<Book>> GetAllAsync()
    {
        return await context.Books.ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await context.Books.FindAsync(id);
    }

    public async Task<Book> AddAsync(Book book)
    {
        context.Books.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateAsync(int id, Book book)
    {
        var existing = await context.Books.FindAsync(id);
        if (existing is null)
            return null;

        context.Entry(existing).CurrentValues.SetValues(book);
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Books.FindAsync(id);
        if (existing is null)
            return false;

        context.Books.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}