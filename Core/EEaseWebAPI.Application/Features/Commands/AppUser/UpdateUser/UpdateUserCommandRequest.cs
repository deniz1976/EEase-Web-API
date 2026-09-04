using EEaseWebAPI.Application.JsonConverters;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser
{
    public class UpdateUserCommandRequest : IRequest<UpdateUserCommandResponse>
    {
        public string? Username { get; set; }
        public string? Name{ get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]

        public DateOnly? BornDate { get; set; }

        public string? user {  get; set; }

        public string? bio { get; set; }
    }
}
