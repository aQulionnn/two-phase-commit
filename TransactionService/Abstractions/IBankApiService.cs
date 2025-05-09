using Contracts;
using Refit;

namespace TransactionService.Abstractions;

public interface IBankApiService
{
    [Post("/bank-accounts/{id}/prepare-transfer")]
    Task<HttpResponseMessage> PrepareTransfer(int id, [Body] TransferRequest request);
    
    [Post("/bank-accounts/{id}/commit-transfer")]
    Task<HttpResponseMessage> CommitTransfer(int id, [Body] TransferRequest request);
}