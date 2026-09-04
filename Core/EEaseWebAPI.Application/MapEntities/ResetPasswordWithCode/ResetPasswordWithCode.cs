using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.ResetPasswordWithCode
{
    public class ResetPasswordWithCode
    {
        public Header? Header { get; set; }
        public ResetPasswordWithCodeBody? Body { get; set; }
    }

    public class ResetPasswordWithCodeBody
    {
        public string ?message { get; set; }
    }
}
