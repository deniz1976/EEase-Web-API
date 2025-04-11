using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.GetUserPhoto
{
    public class GetUserPhoto
    {
        public Header? Header {  get; set; }

        public GetUserPhotoBody? Body { get; set; }
    }

    public class GetUserPhotoBody 
    {
        public string? path {  get; set; }
        public string? errorMessage { get; set; }
    }
}
