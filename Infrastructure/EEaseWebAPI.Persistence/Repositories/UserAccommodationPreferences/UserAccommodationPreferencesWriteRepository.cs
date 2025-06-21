using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class UserAccommodationPreferencesWriteRepository : WriteRepository<UserAccommodationPreferences>, IUserAccommodationPreferencesWriteRepository
    {
        public UserAccommodationPreferencesWriteRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
} 