using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserAccommodationPreferencesReadRepository : ReadRepository<UserAccommodationPreferences>, IUserAccommodationPreferencesReadRepository
    {
        public UserAccommodationPreferencesReadRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 