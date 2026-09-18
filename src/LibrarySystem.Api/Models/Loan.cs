namespace LibrarySystem.Api.Models;

public class Loan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public User? User { get; set; }
    public Book? Book { get; set; }
}
