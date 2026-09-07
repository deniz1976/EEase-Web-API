using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Common;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPreferenceFeedbackService
    {
        Task<PreferenceFeedbackResult> ApplyAsync(string userId, BaseEntity place, string placeType, bool liked);
    }
}
