namespace TransactionService.DelegatingHandlers;

public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger = logger;
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending HTTP {Method} request to {Url}", request.Method, request.RequestUri);
            
            var result = await base.SendAsync(request, cancellationToken);
            result.EnsureSuccessStatusCode();
            
            _logger.LogInformation("HTTP request to {Url} completed successfully", request.RequestUri);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request to {Url} failed: {Message}", request.RequestUri, ex.Message);
            throw;
        }
    }
}