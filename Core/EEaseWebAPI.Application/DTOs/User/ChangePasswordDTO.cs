using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.User
{
    public class ChangePasswordDTO
    {
        public string? OldPassword {  get; set; }

        public string? NewPassword { get; set;}
    }
}
