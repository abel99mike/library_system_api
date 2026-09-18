using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Api.DTOs.Books;

public class BookUpdateDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Author { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int PublishedYear { get; set; }
}
