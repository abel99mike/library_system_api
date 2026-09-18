using LibrarySystem.Api.Data;
using LibrarySystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Api.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _context;

    public LoanRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public Task<Loan?> GetByIdAsync(Guid id) =>
        _context.Loans
            .Include(l => l.User)
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);

    public Task<List<Loan>> GetActiveLoansAsync() =>
        _context.Loans
            .Include(l => l.User)
            .Include(l => l.Book)
            .Where(l => l.ReturnDate == null)
            .OrderBy(l => l.LoanDate)
            .ToListAsync();

    public async Task AddAsync(Loan loan) => await _context.Loans.AddAsync(loan);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
