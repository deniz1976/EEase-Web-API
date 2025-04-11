using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword
{
    public class ResetPasswordCommandResponse 
    {
        public MapEntities.ResetPasswordWithCode.ResetPasswordWithCode ?ResetPasswordWithCode { get; set; }
    }
}
