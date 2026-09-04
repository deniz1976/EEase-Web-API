namespace EEaseWebAPI.Application.MapEntities
{
    public class BaseResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public GlobalError Error { get; set; }

        public BaseResponse()
        {
            Succeeded = true;
        }

        public BaseResponse(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Message = message;
        }

        public BaseResponse(bool succeeded, string message, GlobalError error)
        {
            Succeeded = succeeded;
            Message = message;
            Error = error;
        }
    }
} 