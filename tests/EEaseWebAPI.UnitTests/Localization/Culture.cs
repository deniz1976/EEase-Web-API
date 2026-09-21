using System.Globalization;

namespace EEaseWebAPI.UnitTests.Localization
{
    internal static class Culture
    {
        public static async Task<T> UseAsync<T>(string culture, Func<Task<T>> action)
        {
            var previous = CultureInfo.CurrentUICulture;

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            try
            {
                return await action();
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }

        public static T Use<T>(string culture, Func<T> build)
        {
            var previous = CultureInfo.CurrentUICulture;

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            try
            {
                return build();
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }
    }
}
