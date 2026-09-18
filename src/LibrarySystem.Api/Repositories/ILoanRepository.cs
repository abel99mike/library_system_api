using LibrarySystem.Api.Models;

namespace LibrarySystem.Api.Repositories;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id);
    Task<List<Loan>> GetActiveLoansAsync();
    Task AddAsync(Loan loan);
    Task<int> SaveChangesAsync();
}
