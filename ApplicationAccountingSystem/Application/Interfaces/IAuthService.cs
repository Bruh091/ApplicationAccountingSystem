using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;

namespace ApplicationAccountingSystem.Application.Interface
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto dto);

        Task<UserDto?> LoginAsync(LoginDto dto);
    }
}