using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions
{
    public sealed class RequestValidationException : BaseException
    {
        public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
            : base("The submitted request does not satisfy the validation rules.", (int)StatusEnum.ValidationError)
        {
            Errors = errors;
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
