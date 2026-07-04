using System;
using ApplicationAccountingSystem.Domain.Designation;

namespace ApplicationAccountingSystem.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = "";

        public string Email { get; set; } = "";

        public string FullName { get; set; } = "";

        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
