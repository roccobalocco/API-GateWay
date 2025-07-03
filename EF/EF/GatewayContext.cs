using EF.Models;
using Microsoft.EntityFrameworkCore;

namespace EF.EF;

public class GatewayContext(DbContextOptions<GatewayContext> options) : DbContext(options) {
    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Loan> Loans { get; set; }
}
