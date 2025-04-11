using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserPersonalizationReadRepository : ReadRepository<UserPersonalization>, IUserPersonalizationReadRepository
    {
        public UserPersonalizationReadRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 