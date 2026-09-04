using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferencesWithTopics
{
    public class UpdateUserPreferencesWithTopicsCommandResponse
    {
        public Header? Header { get; set; }
        public UpdateUserPreferencesWithTopicsCommandResponseBody? Body { get; set; }
    }

    public class UpdateUserPreferencesWithTopicsCommandResponseBody
    {
        public string? Message { get; set; }
    }
}
