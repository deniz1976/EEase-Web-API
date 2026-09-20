using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.StatusCheck
{

    public class StatusCheckBody
    {
        public bool? status {  get; set; }
        public string? message { get; set; }
    }
}
