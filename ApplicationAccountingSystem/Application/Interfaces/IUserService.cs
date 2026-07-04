using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;
using ApplicationAccountingSystem.Domain.Designation;

namespace ApplicationAccountingSystem.Application.Interface
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(Guid userId);

        Task<UserDto?> GetUserByUsernameAsync(string username);

        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role);

        Task<UserDto?> UpdateUserRoleAsync(Guid userId, UserRole role);

        Task DeactivateUserAsync(Guid userId);
    }
}
