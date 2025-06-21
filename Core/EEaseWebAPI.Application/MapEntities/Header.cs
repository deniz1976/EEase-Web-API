using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities
{
    public class Header
    {
        public bool? Success { get; set; }
        public DateTime? ResponseDate { get; set; }
        public int? EnumStatusCode { get; set; }
    }
}
