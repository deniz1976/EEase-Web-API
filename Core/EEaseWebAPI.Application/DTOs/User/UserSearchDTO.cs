using System;

namespace EEaseWebAPI.Application.DTOs.User
{
    public class UserSearchDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhotoUrl { get; set; }
        public string? Gender { get; set; }
    }
} 