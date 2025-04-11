using EEaseWebAPI.Application.MapEntities.UpdateUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser
{
    public class UpdateUserCommandResponse
    {
        public MapEntities.UpdateUser.UpdateUser? UpdateUser { get; set; }
    }
}
