using EEaseWebAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.CreateUser
{
    public class CreateUser
    {
        public CreateUserBody? Body { get; set; }
        public Header? Header { get; set; }
    }

    public class CreateUserBody
    {
        public string? message { get; set; }
    }
}
