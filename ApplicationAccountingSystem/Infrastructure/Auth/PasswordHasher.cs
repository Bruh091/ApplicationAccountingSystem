using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace ApplicationAccountingSystem.Infrastructure.Auth
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            var HashPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return HashPassword;
        }
    }
}
