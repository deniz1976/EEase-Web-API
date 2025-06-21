using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteAllRoutes
{
    public class DeleteAllRoutesCommandRequest : IRequest<DeleteAllRoutesCommandResponse>
    {
        public string? Username { get; set; }
    }
}
