using LibrarySystem.Api.DTOs.Users;

namespace LibrarySystem.Api.Services;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(UserCreateDto dto);
    Task<List<UserResponseDto>> GetAllAsync();
}
