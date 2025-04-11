using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class RegularOpeningHours : BaseEntity
    {
        [JsonPropertyName("openNow")]
        public bool? OpenNow { get; set; }
        [JsonPropertyName("period")]
        public List<Period>? Periods { get; set; }
        [JsonPropertyName("weekdayDescriptions")]
        public List<string>? WeekdayDescriptions { get; set; }
    }

    public class Close : BaseEntity
    {
        [JsonPropertyName("day")]
        public int? Day { get; set; }

        [JsonPropertyName("hour")]
        public int? Hour { get; set; }


        [JsonPropertyName("minute")]
        public int? Minute { get; set; }
    }

    public class Open : BaseEntity
    {
        [JsonPropertyName("day")]
        public int? Day { get; set; }
        [JsonPropertyName("hour")]
        public int? Hour { get; set; }
        [JsonPropertyName("minute")]
        public int? Minute { get; set; }
    }

    public class Period : BaseEntity
    {
        [JsonPropertyName("open")]
        public Open? Open { get; set; }
        [JsonPropertyName("close")]
        public Close? Close { get; set; }
    }
}
