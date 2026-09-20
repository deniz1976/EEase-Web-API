using EEaseWebAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.Login
{

    public class LoginBody
    {
        public Token? Token { get; set; }

        public UserInfo? UserInfo { get; set; }

        public string? Warning { get; set; }

    }

    public class UserInfo
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public DateOnly? BornDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool? Status { get; set; }
        public string? Country { get; set; }

        public string? Currency {  get; set; }

        public string? Bio { get; set; }

        public string? PhotoPath { get; set; }
        public bool? EmailConfirmed { get; set; }
        public DateTime? LastSeen { get; set; }

    }
}
