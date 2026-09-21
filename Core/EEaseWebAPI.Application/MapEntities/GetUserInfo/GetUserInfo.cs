namespace EEaseWebAPI.Application.MapEntities.GetUserInfo
{
    public class GetUserInfoBody
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }
        public string? Username { get; set; }
        public DateOnly? BornDate { get; set; }

        public string? Bio {  get; set; }

        public string? PhotoPath { get; set; }
        public string? Currency {  get; set; }

        public string? Country { get; set; }

        public string? Id { get; set; }
    }
}
