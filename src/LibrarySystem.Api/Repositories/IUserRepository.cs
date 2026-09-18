using LibrarySystem.Api.Models;

namespace LibrarySystem.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(User user);
    Task<int> SaveChangesAsync();
}
