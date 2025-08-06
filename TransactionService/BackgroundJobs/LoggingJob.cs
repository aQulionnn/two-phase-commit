using TickerQ.Utilities.Base;
using TickerQ.Utilities.Models;
using TransactionService.Entities;

namespace TransactionService.BackgroundJobs;

public class LoggingJob(ILogger<LoggingJob> logger)
{
    private readonly ILogger<LoggingJob> _logger = logger;

    [TickerFunction("LoggingJob")]
    public void Execute(TickerFunctionContext<List<Transaction>> tickerContext)
    {
        _logger.LogInformation("Retrieved Transactions: {List<Transaction>}", tickerContext.Request);
    }
}