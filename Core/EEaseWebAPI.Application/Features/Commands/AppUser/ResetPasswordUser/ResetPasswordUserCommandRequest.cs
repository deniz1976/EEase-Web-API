using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser
{
    public class ResetPasswordUserCommandRequest : IRequest<ResetPasswordUserCommandResponse>
    {
        public string EmailOrUsername { get; set; }
    }
}
