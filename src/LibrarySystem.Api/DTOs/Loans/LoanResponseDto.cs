namespace LibrarySystem.Api.DTOs.Loans;

public class LoanResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid BookId { get; set; }
    public string? BookTitle { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}
