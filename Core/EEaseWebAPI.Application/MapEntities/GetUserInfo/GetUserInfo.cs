namespace EEaseWebAPI.Application.MapEntities.GetUserInfo
{
    public class GetUserInfo
    {
        public GetUserInfoBody? GetUserInfoBody { get; set; }   

        public Header? Header { get; set; }
    }

    public class GetUserInfoBody 
    {
        public string? name { get; set; }
        public string? email { get; set; }
        public string? surname { get; set; }
        public string? gender { get; set; }
        public string? username { get; set; }
        public DateOnly? BornDate { get; set; }

        public string? bio {  get; set; }

        public string? photoPath { get; set; }
        public string? currency {  get; set; }

        public string? country { get; set; }

        public string? id { get; set; }
    }
}
