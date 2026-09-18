using LibrarySystem.Api.DTOs.Books;

namespace LibrarySystem.Api.Services;

public interface IBookService
{
    Task<BookResponseDto> CreateAsync(BookCreateDto dto);
    Task<BookResponseDto> UpdateAsync(Guid id, BookUpdateDto dto);
    Task DeleteAsync(Guid id);
    Task<List<BookResponseDto>> GetAllAsync(bool? isAvailable, string? author);
    Task<BookResponseDto> GetByIdAsync(Guid id);
}
