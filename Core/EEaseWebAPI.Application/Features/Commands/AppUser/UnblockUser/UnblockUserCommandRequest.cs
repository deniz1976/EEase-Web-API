using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UnblockUser
{
    public class UnblockUserCommandRequest : IRequest<UnblockUserCommandResponse>
    {
        public string? Username { get; set; }
        public string? TargetUsername { get; set; }
    }
}
