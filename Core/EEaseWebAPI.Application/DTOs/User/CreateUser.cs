using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.User
{
    public class CreateUser
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PasswordConfirm { get; set; }
        public string? Gender { get; set; }
        public string? Username { get; set; }

        public DateOnly? BornDate { get; set; }
    }
}
