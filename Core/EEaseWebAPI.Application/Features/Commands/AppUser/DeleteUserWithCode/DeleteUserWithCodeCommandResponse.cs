using EEaseWebAPI.Application.MapEntities.DeleteUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUserWithCode
{
    public class DeleteUserWithCodeCommandResponse
    {
        public Application.MapEntities.DeleteUserWithCode.DeleteUserWithCode? DeleteUser { get; set; }
    }
}
