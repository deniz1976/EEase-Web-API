using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserFoodPreferencesReadRepository : ReadRepository<UserFoodPreferences>, IUserFoodPreferencesReadRepository
    {
        public UserFoodPreferencesReadRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 