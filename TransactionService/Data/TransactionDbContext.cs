using Microsoft.EntityFrameworkCore;
using TransactionService.Entities;

namespace TransactionService.Data;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }
}