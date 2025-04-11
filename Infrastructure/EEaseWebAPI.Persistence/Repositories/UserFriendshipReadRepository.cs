using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserFriendshipReadRepository : ReadRepository<UserFriendship>, IUserFriendshipReadRepository
    {
        public UserFriendshipReadRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 