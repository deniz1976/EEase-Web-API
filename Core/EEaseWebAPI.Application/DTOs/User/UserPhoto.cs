namespace EEaseWebAPI.Application.DTOs.User
{
    /// <summary>
    /// Where the traveller's photo lives. It used to arrive in the query string of a PUT,
    /// which is where a URL ends up in a log rather than in a body.
    /// </summary>
    public class UserPhoto
    {
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
