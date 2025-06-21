using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ChangePassword
{
    public class ChangePasswordCommandResponse 
    {
        public MapEntities.ChangePassword.ChangePassword? ChangePassword {  get; set; }
    }
}
