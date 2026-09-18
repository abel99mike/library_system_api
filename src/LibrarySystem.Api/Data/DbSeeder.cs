using System.Globalization;
using Bogus;
using LibrarySystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Api.Data;

public static class DbSeeder
{
    private const int BookCount = 1000;
    private const int UserCount = 200;
    private const int LoanCount = 500;
    private const int ActiveLoanCount = 50;

    public static async Task SeedAsync(LibraryDbContext context)
    {
        if (await context.Books.AnyAsync() || await context.Users.AnyAsync() || await context.Loans.AnyAsync())
        {
            Console.WriteLine("Database already contains data; skipping seed.");
            return;
        }

        Randomizer.Seed = new Random(20260914);
        var random = Randomizer.Seed;

        var books = GenerateBooks();
        var users = GenerateUsers();

        await context.Books.AddRangeAsync(books);
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var loans = GenerateLoans(books, users, random);
        await context.Loans.AddRangeAsync(loans);
        await context.SaveChangesAsync();

        var activeCount = loans.Count(l => l.ReturnDate is null);
        Console.WriteLine($"Seeded {books.Count} books, {users.Count} users, {loans.Count} loans ({activeCount} still on loan).");
    }

    private static List<Book> GenerateBooks()
    {
        var usedIsbns = new HashSet<string>();

        var faker = new Faker<Book>()
            .RuleFor(b => b.Id, f => Guid.NewGuid())
            .RuleFor(b => b.Title, f => ToTitleCase(f.Lorem.Sentence(f.Random.Int(2, 5)).TrimEnd('.')))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.ISBN, f => UniqueValue(usedIsbns, () => f.Commerce.Ean13()))
            .RuleFor(b => b.PublishedYear, f => f.Random.Int(1950, 2024))
            .RuleFor(b => b.IsAvailable, _ => true);

        return faker.Generate(BookCount);
    }

    private static List<User> GenerateUsers()
    {
        var usedEmails = new HashSet<string>();

        var faker = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, f => UniqueValue(usedEmails, () => f.Internet.Email()))
            .RuleFor(u => u.RegisteredDate, f => f.Date.Past(3).ToUniversalTime());

        return faker.Generate(UserCount);
    }

    private static List<Loan> GenerateLoans(List<Book> books, List<User> users, Random random)
    {
        var faker = new Faker();
        var loans = new List<Loan>(LoanCount);

        var activeLoanBooks = books.OrderBy(_ => random.Next()).Take(ActiveLoanCount).ToList();
        foreach (var book in activeLoanBooks)
        {
            var loanDate = faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddDays(-1)).ToUniversalTime();
            loans.Add(new Loan
            {
                Id = Guid.NewGuid(),
                UserId = users[random.Next(users.Count)].Id,
                BookId = book.Id,
                LoanDate = loanDate,
                ReturnDate = null,
            });
            book.IsAvailable = false;
        }

        for (var i = 0; i < LoanCount - ActiveLoanCount; i++)
        {
            var loanDate = faker.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddMonths(-1)).ToUniversalTime();
            var returnDate = faker.Date.Between(loanDate, DateTime.UtcNow).ToUniversalTime();

            loans.Add(new Loan
            {
                Id = Guid.NewGuid(),
                UserId = users[random.Next(users.Count)].Id,
                BookId = books[random.Next(books.Count)].Id,
                LoanDate = loanDate,
                ReturnDate = returnDate,
            });
        }

        return loans;
    }

    private static string UniqueValue(HashSet<string> seen, Func<string> generator)
    {
        string value;
        do
        {
            value = generator();
        } while (!seen.Add(value));

        return value;
    }

    private static string ToTitleCase(string value) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value);
}
