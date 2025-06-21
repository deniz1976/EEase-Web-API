using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserPersonalizationWriteRepository : WriteRepository<UserPersonalization>, IUserPersonalizationWriteRepository
    {
        public UserPersonalizationWriteRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 