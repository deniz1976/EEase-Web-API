using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ConfirmEmailUser
{
    public class ConfirmEmailUserCommandRequest : IRequest<ConfirmEmailUserCommandResponse>
    {
        public string EmailOrUsername { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

    }
}
