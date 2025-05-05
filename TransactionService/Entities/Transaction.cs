using System.ComponentModel.DataAnnotations;

namespace TransactionService.Entities;

public class Transaction
{
    public int Id { get; init; }
    public double Amount { get; init; }
    [MaxLength(50)]
    public required string TransferToBank { get; init; }
    [MaxLength(50)]
    public required string TransferFromBank { get; init; }
    public int TransferToId { get; init; }
    public int TransferFromId { get; init; }
    public DateTime CreatedAt { get; init; }
}