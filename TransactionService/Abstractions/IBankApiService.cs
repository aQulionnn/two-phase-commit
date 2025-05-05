using Contracts;

namespace TransactionService.Abstractions;

public interface IBankApiService
{
    Task<HttpResponseMessage> PrepareTransfer(int id, TransferRequest request);
}