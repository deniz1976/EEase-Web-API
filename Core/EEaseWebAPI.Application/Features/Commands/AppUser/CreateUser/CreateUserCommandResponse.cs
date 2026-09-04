using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.MapEntities.CreateUser;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser
{
    public class CreateUserCommandResponse
    {
        public MapEntities.CreateUser.CreateUser? response { get; set; }
    }
}
