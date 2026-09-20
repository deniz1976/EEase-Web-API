using System.ComponentModel;
using System.Reflection;

namespace EEaseWebAPI.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;

            return description ?? value.ToString();
        }
    }
}
