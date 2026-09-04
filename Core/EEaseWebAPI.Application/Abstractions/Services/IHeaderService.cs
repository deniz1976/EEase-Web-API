using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IHeaderService
    {
        public Header HeaderCreate(int status = (int)StatusEnum.SuccessfullyCreated, bool success = true, DateTime? responseDate = null);
    }
}
