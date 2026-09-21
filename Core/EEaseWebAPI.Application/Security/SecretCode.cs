using System.Security.Cryptography;
using System.Text;

namespace EEaseWebAPI.Application.Security
{
    public static class SecretCode
    {
        // String equality stops at the first byte that differs, so how long it takes says how
        // much of the code was right. Six digits and a handful of attempts make that hard to
        // use, but comparing in a fixed time costs nothing and removes the question.
        // A refresh token is 256 bits of randomness, not a password somebody chose, so
        // there is nothing for a rainbow table to precompute and no salt to add. Hashing it
        // means a copy of the users table is not a set of working sessions.
        public static string Hash(string token) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

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
