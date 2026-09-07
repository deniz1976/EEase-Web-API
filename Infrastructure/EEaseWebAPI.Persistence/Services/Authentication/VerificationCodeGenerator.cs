using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using System.Security.Cryptography;

namespace EEaseWebAPI.Persistence.Services.Authentication
{
    public sealed class VerificationCodeGenerator : IVerificationCodeGenerator
    {
        private const int Minimum = 100000;
        private const int Maximum = 1000000;

        public string Generate() =>
            RandomNumberGenerator.GetInt32(Minimum, Maximum).ToString();
    }
}
