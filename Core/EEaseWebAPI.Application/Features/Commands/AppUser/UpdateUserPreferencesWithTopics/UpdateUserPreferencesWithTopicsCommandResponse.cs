using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferencesWithTopics
{
    public class UpdateUserPreferencesWithTopicsCommandResponse : ApiResponse<UpdateUserPreferencesWithTopicsCommandResponseBody>
    {
    }

    public class UpdateUserPreferencesWithTopicsCommandResponseBody
    {
        public string? Message { get; set; }
    }
}
