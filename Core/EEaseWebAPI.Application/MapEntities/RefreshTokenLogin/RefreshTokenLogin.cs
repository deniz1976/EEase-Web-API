using EEaseWebAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.RefreshTokenLogin
{
    public class RefreshTokenLogin
    {
        public RefreshTokenLoginBody? Body { get; set; }
        public Header? Header { get; set; }
    }

    public class RefreshTokenLoginBody 
    {
        public Token? Token { get; set; }

        public string? warning { get; set; }
    }
}
