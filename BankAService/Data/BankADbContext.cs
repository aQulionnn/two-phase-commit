using BankAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankAService.Data;

public class BankADbContext : DbContext
{
    public BankADbContext(DbContextOptions<BankADbContext> options) :  base(options)
    {
        SeedData();
    }
    
    public DbSet<BankAccount> BankAccounts { get; set; }

    private void SeedData()
    {
        if (!BankAccounts.Any())
        {
            BankAccounts.Add(new BankAccount
            {
                Id = 1,
                Balance = 1000
            });

            BankAccounts.Add(new BankAccount
            {
                Id = 2,
                Balance = 500
            });
        }

        SaveChanges();
    }
}