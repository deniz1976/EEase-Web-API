using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUser
{
    public class DeleteUserCommandResponse
    {
        public MapEntities.DeleteUser.DeleteUser? DeleteUser { get; set; }
    }
}
