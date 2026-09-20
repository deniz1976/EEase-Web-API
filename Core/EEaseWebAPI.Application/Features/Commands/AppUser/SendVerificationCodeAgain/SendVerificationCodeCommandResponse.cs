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
        public string? message { get; set; }
        public bool? success { get; set; }
    }
}
