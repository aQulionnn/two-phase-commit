using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models.Ticker;
using TransactionService.Abstractions;
using TransactionService.BackgroundJobs;
using TransactionService.Data;
using TransactionService.Dtos;
using TransactionService.Entities;

namespace TransactionService.Controllers;

[Route("api/transactions")]
[ApiController]
public class TransactionController
    (
        TransactionDbContext context,
        Func<string, IBankApiService> bankApiFactory,
        ILogger<TransactionController> logger,
        ITimeTickerManager<TimeTicker> timeTickerManager,
        CancellationToken token = default
    ) 
    : ControllerBase
{
    private readonly TransactionDbContext _context = context;
    private readonly Func<string, IBankApiService> _bankApiFactory =  bankApiFactory;
    private readonly ILogger<TransactionController> _logger = logger;
    private readonly ITimeTickerManager<TimeTicker> _timeTickerManager = timeTickerManager;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var transaction = new Transaction
        {
            Amount = dto.Amount,
            TransferToBank = dto.TransferToBank,
            TransferFromBank = dto.TransferFromBank,
            TransferToId = dto.TransferToId,
            TransferFromId = dto.TransferFromId,
            CreatedAt = DateTime.Now
        };
        
        var fromService = _bankApiFactory(transaction.TransferFromBank);
        var toService = _bankApiFactory(transaction.TransferToBank);
        
        var transferFrom = new TransferRequest
        {
            Amount = transaction.Amount,
            Transfer = TransferType.TransferFrom
        };
        
        var transferTo = new TransferRequest
        {
            Amount = transaction.Amount,
            Transfer = TransferType.TransferTo
        };
        
        var prepareFrom = await fromService.PrepareTransfer(transaction.TransferFromId, transferFrom);
        _logger.LogError("TransferFrom response: {StatusCode}, {Content}, URL: {Url}", 
            prepareFrom.StatusCode, 
            await prepareFrom.Content.ReadAsStringAsync(),
            prepareFrom.RequestMessage!.RequestUri);
        
        var prepareTo = await toService.PrepareTransfer(transaction.TransferToId, transferTo);
        _logger.LogError("TransferTo response: {StatusCode}, {Content}, URL: {Url}", 
            prepareTo.StatusCode, 
            await prepareTo.Content.ReadAsStringAsync(),
            prepareTo.RequestMessage!.RequestUri);

        if (prepareFrom.IsSuccessStatusCode && prepareTo.IsSuccessStatusCode)
        {
            var commitFrom = await fromService.CommitTransfer(transaction.TransferFromId, transferFrom);
            var commitTo = await toService.CommitTransfer(transaction.TransferToId, transferTo);

            if (commitFrom.IsSuccessStatusCode && commitTo.IsSuccessStatusCode)
            {
                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();
                return Ok("Transaction created successfully");
            }
            else return BadRequest("Transaction creation failed");
        }
        else return BadRequest("Preparation failed");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _context.Transactions.ToListAsync();

        await _timeTickerManager.AddAsync(new TimeTicker
        {
            Request = TickerHelper.CreateTickerRequest(result),
            ExecutionTime = DateTime.Now.AddSeconds(5),
            Function = nameof(LoggingJob.Execute),
            Description = "All Transactions",
            Retries = 3,
            RetryIntervals = [1, 5, 10]
        });
        
        return Ok(result);
    }
}