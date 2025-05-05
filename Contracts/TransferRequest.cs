namespace Contracts;

public class TransferRequest
{
    public double Amount { get; init; }
    public TransferType Transfer { get; init; }
}