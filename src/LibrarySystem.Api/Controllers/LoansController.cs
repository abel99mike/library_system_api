using LibrarySystem.Api.DTOs.Loans;
using LibrarySystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    /// <summary>List all currently active (not yet returned) loans.</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<LoanResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LoanResponseDto>>> GetActiveLoans()
    {
        var loans = await _loanService.GetActiveLoansAsync();
        return Ok(loans);
    }

    /// <summary>Borrow a book on behalf of a user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponseDto>> Borrow([FromBody] LoanCreateDto dto)
    {
        var loan = await _loanService.BorrowAsync(dto);
        return CreatedAtAction(nameof(GetActiveLoans), new { id = loan.Id }, loan);
    }

    /// <summary>Return a previously borrowed book.</summary>
    [HttpPut("{id:guid}/return")]
    [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponseDto>> Return(Guid id)
    {
        var loan = await _loanService.ReturnAsync(id);
        return Ok(loan);
    }
}
