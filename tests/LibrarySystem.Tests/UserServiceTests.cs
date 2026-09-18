using LibrarySystem.Api.DTOs.Users;
using LibrarySystem.Api.Exceptions;
using LibrarySystem.Api.Repositories;
using LibrarySystem.Api.Services;
using Xunit;

namespace LibrarySystem.Tests;

public class UserServiceTests
{
    private static UserService CreateService()
    {
        var context = TestDbContextFactory.Create();
        return new UserService(new UserRepository(context));
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenEmailIsUnique()
    {
        var service = CreateService();
        var dto = new UserCreateDto { Name = "Alice", Email = "alice@example.com" };

        var result = await service.RegisterAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailAlreadyRegistered()
    {
        var service = CreateService();
        await service.RegisterAsync(new UserCreateDto { Name = "Alice", Email = "alice@example.com" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(new UserCreateDto { Name = "Alice Clone", Email = "alice@example.com" }));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRegisteredUsers()
    {
        var service = CreateService();
        await service.RegisterAsync(new UserCreateDto { Name = "Alice", Email = "alice@example.com" });
        await service.RegisterAsync(new UserCreateDto { Name = "Bob", Email = "bob@example.com" });

        var results = await service.GetAllAsync();

        Assert.Equal(2, results.Count);
    }
}
