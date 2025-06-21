using EEaseWebAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.UpdateUser
{
    public class UpdateUser
    {
        public Header? Header { get; set; }
        public UpdateUserBody? Body { get; set; }
    }

    public class UpdateUserBody 
    {
        public Token? newToken { get; set; }

        public string? message { get; set; }
    }
}
