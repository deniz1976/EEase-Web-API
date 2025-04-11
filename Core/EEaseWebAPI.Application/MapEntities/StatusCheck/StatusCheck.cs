using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.StatusCheck
{
    public class StatusCheck
    {
        public StatusCheckBody? Body { get; set; }  

        public Header? Header { get; set; }
    }

    public class StatusCheckBody 
    {
        public bool? status {  get; set; }
        public string? message { get; set; }
    }
}
