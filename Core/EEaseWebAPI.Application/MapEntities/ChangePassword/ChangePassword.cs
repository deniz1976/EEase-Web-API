using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.ChangePassword
{
    public class ChangePassword
    {
        public Header? Header { get; set; }

        public ChangePasswordBody? Body { get; set; }
    }

    public class ChangePasswordBody 
    {
        public string? message { get; set; }
    }
}
