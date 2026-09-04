using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.CheckEmailIsInUse
{
    public class CheckEmailIsInUse
    {
        public CheckEmailIsInUseBody ?Body { get; set; }
        public Header ?Header { get; set; }
    }
    public class CheckEmailIsInUseBody 
    {
        public bool? result { get; set; }

        public string? message { get; set; }
    }
}
