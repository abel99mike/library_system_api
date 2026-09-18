using LibrarySystem.Api.DTOs.Users;
using LibrarySystem.Api.Exceptions;
using LibrarySystem.Api.Models;
using LibrarySystem.Api.Repositories;

namespace LibrarySystem.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> RegisterAsync(UserCreateDto dto)
    {
        if (await _userRepository.ExistsByEmailAsync(dto.Email))
        {
            throw new ConflictException($"A user with email '{dto.Email}' is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            RegisteredDate = DateTime.UtcNow,
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return ToResponseDto(user);
    }

    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(ToResponseDto).ToList();
    }

    private static UserResponseDto ToResponseDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        RegisteredDate = user.RegisteredDate,
    };
}
