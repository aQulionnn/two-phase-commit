using Microsoft.Extensions.Options;
using TransactionService.Configurations;

namespace TransactionService.DelegatingHandlers;

public class BankApiAuthHandler(IOptions<BankApiSettings> options) : DelegatingHandler
{
    private readonly BankApiSettings _settings = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Internal-Auth-Key", _settings.InternalAuthKey);
        
        return base.SendAsync(request, cancellationToken);
    }
}