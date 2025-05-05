using BankBService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankBService.Data;

public class BankBDbContext(DbContextOptions<BankBDbContext> options) : DbContext(options)
{
    public DbSet<BankAccount> BankAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>().HasData(
            new BankAccount
            {
                Id = 1,
                Balance = 750
            }, 
            new BankAccount
            {
                Id = 2,
                Balance = 250
            });
    }
}