using System.Text;
using System.Text.Json;
using Contracts;
using TransactionService.Abstractions;

namespace TransactionService.Services;

public class BankAApiService(HttpClient httpClient) : IBankApiService
{
    public async Task<HttpResponseMessage> PrepareTransfer(int id, TransferRequest request)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"bank-accounts/{id}/prepare-transfer", content);
        return response;
    }

    public async Task<HttpResponseMessage> CommitTransfer(int id, TransferRequest request)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"bank-accounts/{id}/commit-transfer", content);
        return response;
    }
}