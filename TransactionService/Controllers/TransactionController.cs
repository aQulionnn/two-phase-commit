using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransactionService.Abstractions;
using TransactionService.Data;
using TransactionService.Entities;
using TransactionService.Services;

namespace TransactionService.Controllers;

[Route("api/transactions")]
[ApiController]
public class TransactionController
    (
        TransactionDbContext context,
        IBankApiService bankA,
        IBankApiService bankB,
        CancellationToken toke = default
    ) : ControllerBase
{
    private readonly TransactionDbContext _context = context;
    private readonly IBankApiService _bankA = bankA;
    private readonly IBankApiService _bankB = bankB;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transaction transaction)
    {
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

        var fromService = transaction.TransferFromBank switch
        {
            "BankA" => _bankA,
            "BankB" => _bankB,
            _ => null
        };

        var toService = transaction.TransferToBank switch
        {
            "BankA" => _bankA,
            "BankB" => _bankB,
            _ => null
        };
        
        if (fromService is null || toService is null) return BadRequest("Invalid bank");

        var prepareFrom = await fromService.PrepareTransfer(transaction.TransferFromId, transferFrom);
        var prepareTo = await toService.PrepareTransfer(transaction.TransferToId, transferTo);
        
        if (prepareFrom.IsSuccessStatusCode && prepareTo.IsSuccessStatusCode) return Ok("Preparation successful");
        
        return BadRequest("Preparation failed");
    }
    
}