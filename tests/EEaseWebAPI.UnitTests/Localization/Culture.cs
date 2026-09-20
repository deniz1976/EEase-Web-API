using System.Globalization;

namespace EEaseWebAPI.UnitTests.Localization
{
    /// <summary>
    /// Runs a piece of code under a given UI culture. Localized text is resolved when the
    /// object that holds it is built, so a test that cares about the wording has to pin the
    /// culture around the construction rather than around the assertion.
    /// </summary>
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
