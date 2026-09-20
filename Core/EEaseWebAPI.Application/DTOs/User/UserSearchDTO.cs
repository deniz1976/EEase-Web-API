using System;

namespace EEaseWebAPI.Application.DTOs.User
{
    public class UserSearchDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Gender { get; set; }
    }
}
