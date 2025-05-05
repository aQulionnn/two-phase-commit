using BankAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankAService.Data;

public class BankADbContext(DbContextOptions<BankADbContext> options) : DbContext(options)
{
    public DbSet<BankAccount> BankAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>().HasData(
            new BankAccount
            {
                Id = 1,
                Balance = 1000
            }, 
            new BankAccount
            {
                Id = 2,
                Balance = 500
            });
    }
}