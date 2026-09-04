using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhoto
{
    public class GetUserPhotoQueryRequest : IRequest<GetUserPhotoQueryResponse>
    {
        public string? username { get; set; }
    }
}
