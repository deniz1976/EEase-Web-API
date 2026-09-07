namespace EEaseWebAPI.Application.Abstractions.Services.Authentication
{
    public interface IVerificationCodeGenerator
    {
        string Generate();
    }
}
