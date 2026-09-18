using LibrarySystem.Api.Data;
using LibrarySystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Api.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public Task<Book?> GetByIdAsync(Guid id) =>
        _context.Books.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<Book>> GetAllAsync(bool? isAvailable, string? author)
    {
        var query = _context.Books.AsQueryable();

        if (isAvailable.HasValue)
        {
            query = query.Where(b => b.IsAvailable == isAvailable.Value);
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Author.Contains(author));
        }

        return await query.OrderBy(b => b.Title).ToListAsync();
    }

    public Task<bool> ExistsByIsbnAsync(string isbn, Guid? excludeId = null) =>
        _context.Books.AnyAsync(b => b.ISBN == isbn && (excludeId == null || b.Id != excludeId));

    public Task<bool> HasActiveLoanAsync(Guid bookId) =>
        _context.Loans.AnyAsync(l => l.BookId == bookId && l.ReturnDate == null);

    public async Task AddAsync(Book book) => await _context.Books.AddAsync(book);

    public void Update(Book book) => _context.Books.Update(book);

    public void Remove(Book book) => _context.Books.Remove(book);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
