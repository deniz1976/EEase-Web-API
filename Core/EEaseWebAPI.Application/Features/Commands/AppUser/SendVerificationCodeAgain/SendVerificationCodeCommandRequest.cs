using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendVerificationCodeAgain
{
    public class SendVerificationCodeCommandRequest : IRequest<SendVerificationCodeCommandResponse>
    {
        public string? email {  get; set; }
    }
}
