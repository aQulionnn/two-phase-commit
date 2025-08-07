using TickerQ.Utilities.Enums;
using TickerQ.Utilities.Interfaces;

namespace TransactionService.Handlers;

public class BackgroundJobExceptionHandler(ILogger<BackgroundJobExceptionHandler> logger) 
    : ITickerExceptionHandler
{
    private readonly ILogger<BackgroundJobExceptionHandler> _logger = logger;
    
    public Task HandleExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
    {
        _logger.LogError(exception, "Error in background job {TickerId} of type {TickerType}", tickerId, tickerType);
        return Task.CompletedTask;
    }

    public Task HandleCanceledExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
    {
        _logger.LogError(exception, "The background job {TickerId} of type {TickerType} was cancelled", tickerId, tickerType);
        return Task.CompletedTask;
    }
}