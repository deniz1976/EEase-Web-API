using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser
{
    public class ResetPasswordCommandRequest : IRequest<ResetPasswordCommandResponse>
    {
        public string? UsernameOrEmail { get; set; }

        public string? Code { get; set; }

        public string? NewPassword { get; set; }
    }
}
