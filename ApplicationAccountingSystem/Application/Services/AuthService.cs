using System;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;
using ApplicationAccountingSystem.Application.Interface;
using ApplicationAccountingSystem.Domain.Interfaces;
using ApplicationAccountingSystem.Domain.Model;
using ApplicationAccountingSystem.Infrastructure.Auth;

namespace ApplicationAccountingSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(dto.Username);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Пользователь уже существует");
            }

            var existingEmail = await _userRepository.GetUserByEmailAsync(dto.Email);

            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email уже используется");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Email = dto.Email,
                FullName = dto.FullName,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            return MapToDto(createdUser);
        }

        public async Task<UserDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(dto.Username);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            var isPasswordValid = PasswordHasher.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return MapToDto(user);
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
