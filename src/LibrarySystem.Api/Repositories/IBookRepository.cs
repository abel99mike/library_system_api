using LibrarySystem.Api.Models;

namespace LibrarySystem.Api.Repositories;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);
    Task<List<Book>> GetAllAsync(bool? isAvailable, string? author);
    Task<bool> ExistsByIsbnAsync(string isbn, Guid? excludeId = null);
    Task<bool> HasActiveLoanAsync(Guid bookId);
    Task AddAsync(Book book);
    void Update(Book book);
    void Remove(Book book);
    Task<int> SaveChangesAsync();
}
