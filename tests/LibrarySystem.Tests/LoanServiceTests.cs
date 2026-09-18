using LibrarySystem.Api.DTOs.Books;
using LibrarySystem.Api.DTOs.Loans;
using LibrarySystem.Api.DTOs.Users;
using LibrarySystem.Api.Exceptions;
using LibrarySystem.Api.Repositories;
using LibrarySystem.Api.Services;
using Xunit;

namespace LibrarySystem.Tests;

public class LoanServiceTests
{
    private class Fixture
    {
        public BookService BookService { get; }
        public UserService UserService { get; }
        public LoanService LoanService { get; }

        public Fixture()
        {
            var context = TestDbContextFactory.Create();
            var bookRepository = new BookRepository(context);
            var userRepository = new UserRepository(context);
            var loanRepository = new LoanRepository(context);

            BookService = new BookService(bookRepository);
            UserService = new UserService(userRepository);
            LoanService = new LoanService(loanRepository, bookRepository, userRepository);
        }
    }

    [Fact]
    public async Task BorrowAsync_CreatesLoan_AndMarksBookUnavailable()
    {
        var fixture = new Fixture();
        var book = await fixture.BookService.CreateAsync(new BookCreateDto { Title = "Title", Author = "Author", ISBN = "1", PublishedYear = 2000 });
        var user = await fixture.UserService.RegisterAsync(new UserCreateDto { Name = "Alice", Email = "alice@example.com" });

        var loan = await fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = user.Id, BookId = book.Id });

        Assert.Null(loan.ReturnDate);
        var updatedBook = await fixture.BookService.GetByIdAsync(book.Id);
        Assert.False(updatedBook.IsAvailable);
    }

    [Fact]
    public async Task BorrowAsync_Throws_WhenBookAlreadyOnLoan()
    {
        var fixture = new Fixture();
        var book = await fixture.BookService.CreateAsync(new BookCreateDto { Title = "Title", Author = "Author", ISBN = "1", PublishedYear = 2000 });
        var user = await fixture.UserService.RegisterAsync(new UserCreateDto { Name = "Alice", Email = "alice@example.com" });
        await fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = user.Id, BookId = book.Id });

        var otherUser = await fixture.UserService.RegisterAsync(new UserCreateDto { Name = "Bob", Email = "bob@example.com" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = otherUser.Id, BookId = book.Id }));
    }

    [Fact]
    public async Task BorrowAsync_Throws_WhenUserDoesNotExist()
    {
        var fixture = new Fixture();
        var book = await fixture.BookService.CreateAsync(new BookCreateDto { Title = "Title", Author = "Author", ISBN = "1", PublishedYear = 2000 });

        await Assert.ThrowsAsync<NotFoundException>(() =>
            fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = Guid.NewGuid(), BookId = book.Id }));
    }

    [Fact]
    public async Task ReturnAsync_MarksLoanReturned_AndBookAvailableAgain()
    {
        var fixture = new Fixture();
        var book = await fixture.BookService.CreateAsync(new BookCreateDto { Title = "Title", Author = "Author", ISBN = "1", PublishedYear = 2000 });
        var user = await fixture.UserService.RegisterAsync(new UserCreateDto { Name = "Alice", Email = "alice@example.com" });
        var loan = await fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = user.Id, BookId = book.Id });

        var returned = await fixture.LoanService.ReturnAsync(loan.Id);

        Assert.NotNull(returned.ReturnDate);
        var updatedBook = await fixture.BookService.GetByIdAsync(book.Id);
        Assert.True(updatedBook.IsAvailable);
    }

    [Fact]
    public async Task ReturnAsync_Throws_WhenLoanAlreadyReturned()
    {
        var fixture = new Fixture();
        var book = await fixture.BookService.CreateAsync(new BookCreateDto { Title = "Title", Author = "Author", ISBN = "1", PublishedYear = 2000 });
        var user = await fixture.UserService.RegisterAsync(new UserCreateDto { Name = "Alice", Email = "alice@example.com" });
        var loan = await fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = user.Id, BookId = book.Id });
        await fixture.LoanService.ReturnAsync(loan.Id);

        await Assert.ThrowsAsync<ConflictException>(() => fixture.LoanService.ReturnAsync(loan.Id));
    }

    [Fact]
    public async Task GetActiveLoansAsync_OnlyReturnsUnreturnedLoans()
    {
        var fixture = new Fixture();
        var book1 = await fixture.BookService.CreateAsync(new BookCreateDto { Title = "Title 1", Author = "Author", ISBN = "1", PublishedYear = 2000 });
        var book2 = await fixture.BookService.CreateAsync(new BookCreateDto { Title = "Title 2", Author = "Author", ISBN = "2", PublishedYear = 2001 });
        var user = await fixture.UserService.RegisterAsync(new UserCreateDto { Name = "Alice", Email = "alice@example.com" });

        var loan1 = await fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = user.Id, BookId = book1.Id });
        await fixture.LoanService.BorrowAsync(new LoanCreateDto { UserId = user.Id, BookId = book2.Id });
        await fixture.LoanService.ReturnAsync(loan1.Id);

        var activeLoans = await fixture.LoanService.GetActiveLoansAsync();

        var single = Assert.Single(activeLoans);
        Assert.Equal(book2.Id, single.BookId);
    }
}
