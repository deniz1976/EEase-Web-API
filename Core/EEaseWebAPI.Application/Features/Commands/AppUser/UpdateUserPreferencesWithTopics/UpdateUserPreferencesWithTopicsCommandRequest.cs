using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferencesWithTopics
{
    public class UpdateUserPreferencesWithTopicsCommandRequest : IRequest<UpdateUserPreferencesWithTopicsCommandResponse>
    {
        public string Username { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new List<string>();
    }
}
