using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteAllRoutes
{
    public class DeleteAllRoutesCommandResponse : ApiResponse<DeleteAllRoutesCommandResponseBody>
    {
    }

    public class DeleteAllRoutesCommandResponseBody
    {
        public string Message { get; set; } = string.Empty;
    }
}
