using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.DeleteUserWithCode
{
    public class DeleteUserWithCode
    {
        public Header? Header { get; set; }
        public DeleteUserWithCodeBody? Body { get; set; }
    }
    public class DeleteUserWithCodeBody 
    {
        public string? message { get; set; }
    }
}
