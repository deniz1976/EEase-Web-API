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
        /// <summary>Endpoints that cost us money: a route is built from Gemini and Google.</summary>
        public const string Expensive = "expensive";

        /// <summary>
        /// Endpoints worth attacking: passwords, codes and the mail that carries them. The
        /// global limit of a hundred requests per thirty seconds is a limit on load, not on
        /// somebody working through a list of passwords.
        /// </summary>
        public const string Sensitive = "sensitive";
    }
}
