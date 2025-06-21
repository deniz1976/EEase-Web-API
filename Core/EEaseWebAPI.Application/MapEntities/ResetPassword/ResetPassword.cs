using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities
{
    public class ResetPassword
    {
        public ResetPasswordBody? Body { get; set; }
        public Header? Header { get; set; }
    }

    public class ResetPasswordBody
    {
        public string? message { get; set; }
    }
}
