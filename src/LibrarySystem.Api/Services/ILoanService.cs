using LibrarySystem.Api.DTOs.Loans;

namespace LibrarySystem.Api.Services;

public interface ILoanService
{
    Task<LoanResponseDto> BorrowAsync(LoanCreateDto dto);
    Task<LoanResponseDto> ReturnAsync(Guid loanId);
    Task<List<LoanResponseDto>> GetActiveLoansAsync();
}
