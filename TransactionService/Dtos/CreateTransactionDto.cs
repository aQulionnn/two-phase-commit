namespace TransactionService.Dtos;

public class CreateTransactionDto
{
    public double Amount { get; init; }
    public required string TransferToBank { get; init; }
    public required string TransferFromBank { get; init; }
    public int TransferToId { get; init; }
    public int TransferFromId { get; init; }
}