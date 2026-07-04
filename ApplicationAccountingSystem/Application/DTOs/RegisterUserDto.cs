using ApplicationAccountingSystem.Domain.Designation;

namespace ApplicationAccountingSystem.Application.DTOs
{
    public class RegisterUserDto
    {
        public string Username { get; set; } = "";

        public string Password { get; set; } = "";

        public string Email { get; set; } = "";

        public string FullName { get; set; } = "";

        public UserRole Role { get; set; }
    }
}
