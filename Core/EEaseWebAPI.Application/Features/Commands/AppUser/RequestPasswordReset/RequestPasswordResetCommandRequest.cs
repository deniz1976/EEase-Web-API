using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RequestPasswordReset
{
    public class RequestPasswordResetCommandRequest : IRequest<RequestPasswordResetCommandResponse>
    {
        public string EmailOrUsername { get; set; } = string.Empty;
    }
}
