using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities
{
    public class GlobalError
    {
        public int? StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Title { get; set; }

        public int? EnumStatusCode { get; set; }
    }
}
