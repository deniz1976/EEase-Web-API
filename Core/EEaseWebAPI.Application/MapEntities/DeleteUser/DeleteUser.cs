using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.DeleteUser
{
    public class DeleteUser
    {
        public Header? Header { get; set; }
        public DeleteUserBody? Body { get; set; }
    }

    public class DeleteUserBody
    {
        public string? message { get; set; }
    }
}
