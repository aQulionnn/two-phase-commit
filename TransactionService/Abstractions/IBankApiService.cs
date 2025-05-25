using Contracts;
using Refit;

namespace TransactionService.Abstractions;

public interface IBankApiService
{
    [Post("/api/bank-accounts/{id}/prepare-transfer")]
    Task<HttpResponseMessage> PrepareTransfer(int id, [Body] TransferRequest request);
    
    [Post("/api/bank-accounts/{id}/commit-transfer")]
    Task<HttpResponseMessage> CommitTransfer(int id, [Body] TransferRequest request);
}