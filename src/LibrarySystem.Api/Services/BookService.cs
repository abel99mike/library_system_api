using LibrarySystem.Api.DTOs.Books;
using LibrarySystem.Api.Exceptions;
using LibrarySystem.Api.Models;
using LibrarySystem.Api.Repositories;

namespace LibrarySystem.Api.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<BookResponseDto> CreateAsync(BookCreateDto dto)
    {
        if (await _bookRepository.ExistsByIsbnAsync(dto.ISBN))
        {
            throw new ConflictException($"A book with ISBN '{dto.ISBN}' already exists.");
        }

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            PublishedYear = dto.PublishedYear,
            IsAvailable = true,
        };

        await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();

        return ToResponseDto(book);
    }

    public async Task<BookResponseDto> UpdateAsync(Guid id, BookUpdateDto dto)
    {
        var book = await _bookRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Book with id '{id}' was not found.");

        if (await _bookRepository.ExistsByIsbnAsync(dto.ISBN, id))
        {
            throw new ConflictException($"A book with ISBN '{dto.ISBN}' already exists.");
        }

        book.Title = dto.Title;
        book.Author = dto.Author;
        book.ISBN = dto.ISBN;
        book.PublishedYear = dto.PublishedYear;

        _bookRepository.Update(book);
        await _bookRepository.SaveChangesAsync();

        return ToResponseDto(book);
    }

    public async Task DeleteAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Book with id '{id}' was not found.");

        if (await _bookRepository.HasActiveLoanAsync(id))
        {
            throw new ConflictException("Cannot delete a book that is currently on loan.");
        }

        _bookRepository.Remove(book);
        await _bookRepository.SaveChangesAsync();
    }

    public async Task<List<BookResponseDto>> GetAllAsync(bool? isAvailable, string? author)
    {
        var books = await _bookRepository.GetAllAsync(isAvailable, author);
        return books.Select(ToResponseDto).ToList();
    }

    public async Task<BookResponseDto> GetByIdAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Book with id '{id}' was not found.");

        return ToResponseDto(book);
    }

    private static BookResponseDto ToResponseDto(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        ISBN = book.ISBN,
        PublishedYear = book.PublishedYear,
        IsAvailable = book.IsAvailable,
    };
}
