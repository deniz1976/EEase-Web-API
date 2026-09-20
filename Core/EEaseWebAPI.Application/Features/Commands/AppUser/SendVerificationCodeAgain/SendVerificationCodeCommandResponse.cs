using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendVerificationCodeAgain
{
    public class SendVerificationCodeCommandResponse : ApiResponse<SendVerificationCodeBody>
    {
    }

    public class SendVerificationCodeBody
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
    }
}
