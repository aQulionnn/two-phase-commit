using BankAService.Data;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankAService.Controllers;

[Route("api/bank-accounts")]
[ApiController]
public class BankAccountController(BankADbContext context, CancellationToken token = default) : ControllerBase
{
    private readonly BankADbContext _context = context;

    [HttpPost]
    [Route("/{id:int}/prepare-transfer")]
    public async Task<IActionResult> PrepareTransfer([FromRoute] int id, [FromBody] TransferRequest request)
    {
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == id, token);
        if (account is null) return NotFound();

        if (request.Transfer == TransferType.TransferTo && account.Balance >= request.Amount) return Ok();
        else if (request.Transfer == TransferType.TransferFrom) return Ok();
        else return BadRequest();
    }
}