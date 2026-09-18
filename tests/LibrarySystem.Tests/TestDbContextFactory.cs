using LibrarySystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Tests;

internal static class TestDbContextFactory
{
    public static LibraryDbContext Create()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }
}
