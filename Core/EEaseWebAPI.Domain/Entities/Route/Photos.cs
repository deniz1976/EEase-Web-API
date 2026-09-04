using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class Photos : BaseEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("widthPx")]
        public int? WidthPx { get; set; }

        [JsonPropertyName("heightPx")]
        public int? HeightPx { get; set; }
    }
}
