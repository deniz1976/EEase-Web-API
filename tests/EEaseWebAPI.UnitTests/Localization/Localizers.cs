using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.UnitTests.Localization
{
    internal static class Localizers
    {
        private static readonly IStringLocalizerFactory Factory =
            new ResourceManagerStringLocalizerFactory(
                Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }),
                NullLoggerFactory.Instance);

        public static IStringLocalizer<T> For<T>() => new StringLocalizer<T>(Factory);
    }
}
