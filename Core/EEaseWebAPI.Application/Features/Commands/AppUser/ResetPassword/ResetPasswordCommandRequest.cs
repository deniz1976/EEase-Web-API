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
        public string? code { get; set; }

        public string? newPassword { get; set; }
    }
}
