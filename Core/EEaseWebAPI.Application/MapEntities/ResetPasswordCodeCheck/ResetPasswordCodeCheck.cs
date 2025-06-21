using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.ResetPasswordCodeCheck
{
    public class ResetPasswordCodeCheck
    {
        public Header? Header { get; set; }
        public ResetPasswordCodeCheckQueryBody? Body { get; set; }
    }

    public class ResetPasswordCodeCheckQueryBody 
    {
        public string? message { get; set; }
    }
}
