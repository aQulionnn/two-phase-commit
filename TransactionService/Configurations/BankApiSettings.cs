namespace TransactionService.Configurations;

public sealed class BankApiSettings
{
    public const string SectionName = "BankApi";
    
    public string InternalAuthKey { get; init; } = string.Empty;
}