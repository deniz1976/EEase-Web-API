using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SetUserPhoto
{
    public class SetUserPhotoCommandRequest : IRequest<SetUserPhotoCommandResponse>
    {
        public string Username { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
