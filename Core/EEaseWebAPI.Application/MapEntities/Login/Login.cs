using EEaseWebAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.Login
{
    public class Login
    {
        public LoginBody? Body { get; set; }

        public Header? Header { get; set; }
    }

    public class LoginBody 
    {
        public Token? token { get; set; }

        public UserInfo? userInfo { get; set; }

        public string? warning { get; set; }

    }

    public class UserInfo 
    {
        public string? name { get; set; } //
        public string? surname { get; set; } //
        public string? gender { get; set; } // 
        public string? email { get; set; } //
        public string? username { get; set; }
        public DateOnly? bornDate { get; set; }
        public DateTime? deleteDate { get; set; }
        public bool? status { get; set; }
        public string? country { get; set; }

        public string? currency {  get; set; }

        public string? bio { get; set; }

        public string? photoPath { get; set; }
        public bool? emailConfirmed { get; set; } //
        public DateTime? lastSeen { get; set; }

    }
}
