using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface ISystemUserProvider
    {
        Task<AppUser> GetOrCreateAsync();
    }
}
