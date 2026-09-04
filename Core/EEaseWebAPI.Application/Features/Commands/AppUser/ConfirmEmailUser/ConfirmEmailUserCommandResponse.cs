using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ConfirmEmailUser
{
    public class ConfirmEmailUserCommandResponse
    {
        public MapEntities.ConfirmEmail.ConfirmEmail confirmEmail {  get; set; }
    }
}
