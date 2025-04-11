using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.ConfirmEmail
{
    public class ConfirmEmail
    {
        public ConfirmEmailBody? Body { get; set; }
        public Header? Header { get; set; }
    }

    public class ConfirmEmailBody 
    {
        public bool? result {  get; set; }
    }
}
