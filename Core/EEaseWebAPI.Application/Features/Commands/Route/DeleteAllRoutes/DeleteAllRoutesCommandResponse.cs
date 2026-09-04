using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteAllRoutes
{
    public class DeleteAllRoutesCommandResponse
    {
        public Header? Header { get; set; } 
        public DeleteAllRoutesCommandResponseBody? Body { get; set; } = new DeleteAllRoutesCommandResponseBody();
    }

    public class DeleteAllRoutesCommandResponseBody 
    {
        public string Message { get; set; } = string.Empty;
    }
}
