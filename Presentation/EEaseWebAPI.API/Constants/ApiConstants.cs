namespace EEaseWebAPI.API.Constants
{
    public static class AuthenticationSchemes
    {
        public const string User = "User";
    }

    public static class CorsPolicies
    {
        public const string Default = "EEaseDefaultCors";
    }

    public static class RateLimitPolicies
    {
        public const string PerClient = "per-client";

        public const string Expensive = "expensive";
    }
}
