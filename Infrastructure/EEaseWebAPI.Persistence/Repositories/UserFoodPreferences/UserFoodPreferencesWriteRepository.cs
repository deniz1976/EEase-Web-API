using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserFoodPreferencesWriteRepository : WriteRepository<UserFoodPreferences>, IUserFoodPreferencesWriteRepository
    {
        public UserFoodPreferencesWriteRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 