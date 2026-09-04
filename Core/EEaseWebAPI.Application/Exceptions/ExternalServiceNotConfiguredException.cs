using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions
{
    public sealed class ExternalServiceNotConfiguredException : BaseException
    {
        public ExternalServiceNotConfiguredException(string serviceName, string configurationKey)
            : base(
                $"{serviceName} is not configured. Set '{configurationKey}' in appsettings " +
                "or in an environment variable.",
                (int)StatusEnum.ServiceUnavailable)
        {
            ServiceName = serviceName;
            ConfigurationKey = configurationKey;
        }

        public string ServiceName { get; }

        public string ConfigurationKey { get; }
    }
}
