using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.UpdateUserPreferences
{
    public class UpdateUserPreferencesResponse
    {
        public Header? Header { get; set; }
        public UpdateUserPreferencesBody? Body { get; set; }
    }

    public class UpdateUserPreferencesBody
    {
        public string? message { get; set; }
    }
} 