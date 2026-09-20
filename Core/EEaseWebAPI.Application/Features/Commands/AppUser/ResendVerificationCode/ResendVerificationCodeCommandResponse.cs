using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResendVerificationCode
{
    public class ResendVerificationCodeCommandResponse : ApiResponse<ResendVerificationCodeBody>
    {
    }

    public class ResendVerificationCodeBody
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
    }
}
