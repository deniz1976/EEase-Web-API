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
        public string? Username { get; set; }

        public string? OldPassword { get; set; }

        public string? NewPassword { get; set; }
    }
}
