using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.User
{
    public class GetUserInfo
    {
        public string? name { get; set; }
        public string? email { get; set; }
        public string? surname { get; set; }
        public string? gender { get; set; }
        public string? username { get; set; }

        public DateOnly? borndate { get; set; }

        public string? Id { get; set; }

        public string? bio {  get; set; }

        public string? photoPath  { get; set; }

        public string? currency { get; set; }

        public string? country { get; set; }

        public FriendRequestStatus? friendRequestStatus { get; set; }

        public string? id { get; set; }
    }
}
