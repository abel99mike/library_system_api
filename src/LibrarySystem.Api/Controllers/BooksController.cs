using LibrarySystem.Api.DTOs.Books;
using LibrarySystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>List all books, optionally filtered by availability and/or author.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BookResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookResponseDto>>> GetAll([FromQuery] bool? isAvailable, [FromQuery] string? author)
    {
        var books = await _bookService.GetAllAsync(isAvailable, author);
        return Ok(books);
    }

    /// <summary>Get a single book by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponseDto>> GetById(Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        return Ok(book);
    }

    /// <summary>Create a new book.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookResponseDto>> Create([FromBody] BookCreateDto dto)
    {
        var book = await _bookService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>Update an existing book.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookResponseDto>> Update(Guid id, [FromBody] BookUpdateDto dto)
    {
        var book = await _bookService.UpdateAsync(id, dto);
        return Ok(book);
    }

    /// <summary>Delete a book. Fails with 409 if the book is currently on loan.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _bookService.DeleteAsync(id);
        return NoContent();
    }
}
