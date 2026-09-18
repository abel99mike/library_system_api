using LibrarySystem.Api.DTOs.Books;
using LibrarySystem.Api.Exceptions;
using LibrarySystem.Api.Repositories;
using LibrarySystem.Api.Services;
using Xunit;

namespace LibrarySystem.Tests;

public class BookServiceTests
{
    private static BookService CreateService(out IBookRepository repository)
    {
        var context = TestDbContextFactory.Create();
        repository = new BookRepository(context);
        return new BookService(repository);
    }

    [Fact]
    public async Task CreateAsync_AddsBook_WhenIsbnIsUnique()
    {
        var service = CreateService(out _);
        var dto = new BookCreateDto { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", PublishedYear = 2008 };

        var result = await service.CreateAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.IsAvailable);
        Assert.Equal("Clean Code", result.Title);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenIsbnAlreadyExists()
    {
        var service = CreateService(out _);
        var dto = new BookCreateDto { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", PublishedYear = 2008 };
        await service.CreateAsync(dto);

        var duplicate = new BookCreateDto { Title = "Another Title", Author = "Someone Else", ISBN = "9780132350884", PublishedYear = 2010 };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(duplicate));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenBookNotFound()
    {
        var service = CreateService(out _);
        var dto = new BookUpdateDto { Title = "Title", Author = "Author", ISBN = "123", PublishedYear = 2000 };

        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), dto));
    }

    [Fact]
    public async Task DeleteAsync_RemovesBook_WhenNotOnLoan()
    {
        var service = CreateService(out _);
        var created = await service.CreateAsync(new BookCreateDto { Title = "Title", Author = "Author", ISBN = "123", PublishedYear = 2000 });

        await service.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenBookIsOnLoan()
    {
        var context = TestDbContextFactory.Create();
        var bookRepository = new BookRepository(context);
        var userRepository = new UserRepository(context);
        var loanRepository = new LoanRepository(context);
        var bookService = new BookService(bookRepository);
        var userService = new UserService(userRepository);
        var loanService = new LoanService(loanRepository, bookRepository, userRepository);

        var book = await bookService.CreateAsync(new BookCreateDto { Title = "Title", Author = "Author", ISBN = "123", PublishedYear = 2000 });
        var user = await userService.RegisterAsync(new LibrarySystem.Api.DTOs.Users.UserCreateDto { Name = "Alice", Email = "alice@example.com" });
        await loanService.BorrowAsync(new LibrarySystem.Api.DTOs.Loans.LoanCreateDto { UserId = user.Id, BookId = book.Id });

        await Assert.ThrowsAsync<ConflictException>(() => bookService.DeleteAsync(book.Id));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByAvailabilityAndAuthor()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new BookCreateDto { Title = "Book A", Author = "Author One", ISBN = "1", PublishedYear = 2000 });
        await service.CreateAsync(new BookCreateDto { Title = "Book B", Author = "Author Two", ISBN = "2", PublishedYear = 2001 });

        var results = await service.GetAllAsync(isAvailable: true, author: "Author One");

        var single = Assert.Single(results);
        Assert.Equal("Book A", single.Title);
    }
}
