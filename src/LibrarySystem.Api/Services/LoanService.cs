using LibrarySystem.Api.DTOs.Loans;
using LibrarySystem.Api.Exceptions;
using LibrarySystem.Api.Models;
using LibrarySystem.Api.Repositories;

namespace LibrarySystem.Api.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;

    public LoanService(ILoanRepository loanRepository, IBookRepository bookRepository, IUserRepository userRepository)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }

    public async Task<LoanResponseDto> BorrowAsync(LoanCreateDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId)
            ?? throw new NotFoundException($"User with id '{dto.UserId}' was not found.");

        var book = await _bookRepository.GetByIdAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with id '{dto.BookId}' was not found.");

        if (!book.IsAvailable)
        {
            throw new ConflictException($"Book '{book.Title}' is not available for loan.");
        }

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BookId = book.Id,
            LoanDate = DateTime.UtcNow,
            ReturnDate = null,
        };

        book.IsAvailable = false;
        _bookRepository.Update(book);

        await _loanRepository.AddAsync(loan);
        await _loanRepository.SaveChangesAsync();

        return ToResponseDto(loan, user, book);
    }

    public async Task<LoanResponseDto> ReturnAsync(Guid loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId)
            ?? throw new NotFoundException($"Loan with id '{loanId}' was not found.");

        if (loan.ReturnDate is not null)
        {
            throw new ConflictException("This loan has already been returned.");
        }

        loan.ReturnDate = DateTime.UtcNow;

        var book = await _bookRepository.GetByIdAsync(loan.BookId)
            ?? throw new NotFoundException($"Book with id '{loan.BookId}' was not found.");
        book.IsAvailable = true;
        _bookRepository.Update(book);

        await _loanRepository.SaveChangesAsync();

        return ToResponseDto(loan, loan.User, book);
    }

    public async Task<List<LoanResponseDto>> GetActiveLoansAsync()
    {
        var loans = await _loanRepository.GetActiveLoansAsync();
        return loans.Select(l => ToResponseDto(l, l.User, l.Book)).ToList();
    }

    private static LoanResponseDto ToResponseDto(Loan loan, User? user, Book? book) => new()
    {
        Id = loan.Id,
        UserId = loan.UserId,
        UserName = user?.Name,
        BookId = loan.BookId,
        BookTitle = book?.Title,
        LoanDate = loan.LoanDate,
        ReturnDate = loan.ReturnDate,
    };
}
