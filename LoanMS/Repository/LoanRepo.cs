using EF.EF;
using EF.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanMS.Repository;

public class LoanRepo(GatewayContext context)
{
    public async Task<List<Loan>> GetAllAsync()
    {
        return await context.Loans.ToListAsync();
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await context.Loans.FindAsync(id);
    }

    public async Task<Loan> AddAsync(Loan book)
    {
        context.Loans.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task<Loan?> UpdateAsync(int id, Loan book)
    {
        var existing = await context.Loans.FindAsync(id);
        if (existing is null)
            return null;

        context.Entry(existing).CurrentValues.SetValues(book);
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Loans.FindAsync(id);
        if (existing is null)
            return false;

        context.Loans.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}