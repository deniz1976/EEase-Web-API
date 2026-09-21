using System.Security.Cryptography;
using System.Text;

namespace EEaseWebAPI.Application.Security
{
    public static class SecretCode
    {
        // String equality stops at the first byte that differs, so how long it takes says how
        // much of the code was right. Six digits and a handful of attempts make that hard to
        // use, but comparing in a fixed time costs nothing and removes the question.
        public static bool Matches(string? stored, string? offered)
        {
            if (stored is null || offered is null)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(offered));
        }
    }
}
