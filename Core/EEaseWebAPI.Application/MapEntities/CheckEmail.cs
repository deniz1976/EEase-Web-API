using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities
{
    public class CheckEmail
    {
        public CheckEmailBody? Body { get; set; }
        public Header? Header { get; set; }
    }

    public class CheckEmailBody 
    {
        public bool? result {  get; set; }
    }
}
