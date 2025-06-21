using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserFriendshipWriteRepository : WriteRepository<UserFriendship>, IUserFriendshipWriteRepository
    {
        public UserFriendshipWriteRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 