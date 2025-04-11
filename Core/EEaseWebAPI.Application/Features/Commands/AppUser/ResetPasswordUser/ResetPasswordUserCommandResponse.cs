using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser
{
    public class ResetPasswordUserCommandResponse
    {
        public EEaseWebAPI.Application.MapEntities.ResetPassword? resetPassword {  get; set; }
    }
}
