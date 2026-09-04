using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Queries
{
    public static class RouteQueryExtensions
    {
        public static IQueryable<StandardRoute> IncludeFullRouteGraph(this IQueryable<StandardRoute> query)
        {
            return query
                .AsSplitQuery()
                .Include(route => route.User)
                .Include(route => route.LikedUsers)

                .Include(route => route.TravelDays!).ThenInclude(day => day.Accomodation!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Accomodation!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Accomodation!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Accomodation!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Accomodation!.Photos)

                .Include(route => route.TravelDays!).ThenInclude(day => day.Breakfast!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Breakfast!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Breakfast!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Breakfast!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Breakfast!.Photos)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Breakfast!.Weather)

                .Include(route => route.TravelDays!).ThenInclude(day => day.Lunch!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Lunch!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Lunch!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Lunch!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Lunch!.Photos)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Lunch!.Weather)

                .Include(route => route.TravelDays!).ThenInclude(day => day.Dinner!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Dinner!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Dinner!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Dinner!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Dinner!.Photos)
                .Include(route => route.TravelDays!).ThenInclude(day => day.Dinner!.Weather)

                .Include(route => route.TravelDays!).ThenInclude(day => day.FirstPlace!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.FirstPlace!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.FirstPlace!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.FirstPlace!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.FirstPlace!.Photos)
                .Include(route => route.TravelDays!).ThenInclude(day => day.FirstPlace!.Weather)

                .Include(route => route.TravelDays!).ThenInclude(day => day.SecondPlace!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.SecondPlace!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.SecondPlace!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.SecondPlace!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.SecondPlace!.Photos)
                .Include(route => route.TravelDays!).ThenInclude(day => day.SecondPlace!.Weather)

                .Include(route => route.TravelDays!).ThenInclude(day => day.ThirdPlace!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.ThirdPlace!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.ThirdPlace!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.ThirdPlace!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.ThirdPlace!.Photos)
                .Include(route => route.TravelDays!).ThenInclude(day => day.ThirdPlace!.Weather)

                .Include(route => route.TravelDays!).ThenInclude(day => day.PlaceAfterDinner!.DisplayName)
                .Include(route => route.TravelDays!).ThenInclude(day => day.PlaceAfterDinner!.Location)
                .Include(route => route.TravelDays!).ThenInclude(day => day.PlaceAfterDinner!.RegularOpeningHours)
                .Include(route => route.TravelDays!).ThenInclude(day => day.PlaceAfterDinner!.PaymentOptions)
                .Include(route => route.TravelDays!).ThenInclude(day => day.PlaceAfterDinner!.Photos)
                .Include(route => route.TravelDays!).ThenInclude(day => day.PlaceAfterDinner!.Weather);
        }
    }
}
