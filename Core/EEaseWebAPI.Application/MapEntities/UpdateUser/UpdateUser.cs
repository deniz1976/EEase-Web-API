using EEaseWebAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.UpdateUser
{
    public class UpdateUserBody
    {
        public Token? NewToken { get; set; }

        public string? Message { get; set; }
    }
}
