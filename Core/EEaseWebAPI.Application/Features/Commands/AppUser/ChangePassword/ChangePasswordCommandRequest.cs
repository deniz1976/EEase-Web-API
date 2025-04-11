using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ChangePassword
{
    public class ChangePasswordCommandRequest :IRequest<ChangePasswordCommandResponse>
    {
        public string? username { get; set; }

        public string? oldPassword { get; set; }

        public string? newPassword { get; set; }
    }
}
