using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Services
{
    public class HeaderService : IHeaderService
    {
        public Header HeaderCreate(int status = (int)StatusEnum.SuccessfullyCreated, bool success = true, DateTime? responseDate = null)
        {
            return new Header
            {
                Success = success,
                ResponseDate = responseDate ?? DateTime.UtcNow,
                EnumStatusCode = status
            };
        }
    }
}
