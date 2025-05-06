using BankBService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankBService.Data;

public class BankBDbContext : DbContext
{
    public BankBDbContext(DbContextOptions<BankBDbContext> options) : base(options)
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
                Balance = 750
            });
            
            BankAccounts.Add(new BankAccount
            {
                Id = 2,
                Balance = 250
            });
        }
        
        SaveChanges();
    }
}