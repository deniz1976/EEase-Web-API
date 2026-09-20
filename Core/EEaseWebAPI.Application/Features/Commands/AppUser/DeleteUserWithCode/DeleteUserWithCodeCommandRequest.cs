using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUserWithCode
{
    public class DeleteUserWithCodeCommandRequest : IRequest<DeleteUserWithCodeCommandResponse>
    {
        public string Username { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }
}
