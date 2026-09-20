using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResendVerificationCode
{
    public class ResendVerificationCodeCommandRequest : IRequest<ResendVerificationCodeCommandResponse>
    {
        public string Email { get; set; } = string.Empty;
    }
}
