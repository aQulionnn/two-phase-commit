using BankAService.Data;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankAService.Controllers;

[Route("api/bank-accounts")]
[ApiController]
public class BankAccountController(BankADbContext context, ILogger<BankAccountController> logger, CancellationToken token = default) : ControllerBase
{
    private readonly BankADbContext _context = context;
    private readonly ILogger<BankAccountController> _logger = logger;

    [HttpPost]
    [Route("{id:int}/prepare-transfer")]
    public async Task<IActionResult> PrepareTransfer([FromRoute] int id, [FromBody] TransferRequest request)
    {
        _logger.LogError("Received transfer request: Id={Id}, Type={Type}, Amount={Amount}", id, request.Transfer, request.Amount);
        
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == id, token);
        if (account is null) return NotFound();

        if (request.Transfer == TransferType.TransferTo && account.Balance >= request.Amount) return Ok();
        else if (request.Transfer == TransferType.TransferFrom) return Ok();
        else return BadRequest();
    }

    [HttpPost]
    [Route("{id:int}/commit-transfer")]
    public async Task<IActionResult> CommitTransfer([FromRoute] int id, [FromBody] TransferRequest request)
    {
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == id, token);
        
        if (request.Transfer == TransferType.TransferTo) account!.Balance -= request.Amount;
        else if (request.Transfer == TransferType.TransferFrom) account!.Balance += request.Amount;
        
        await _context.SaveChangesAsync(token);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _context.BankAccounts.ToListAsync();
        return Ok(result);
    }
}