using EEaseWebAPI.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Currency
{
    [Keyless]
    public class AllWordCurrencies
    {
        public string? Entity {  get; set; }

        public string? Currency { get; set; }

        public string? AlphabeticCode { get; set; }
    }
}
