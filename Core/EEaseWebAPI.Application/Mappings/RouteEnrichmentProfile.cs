using AutoMapper;
using EEaseWebAPI.Application.DTOs.Route.Enrichment;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Mappings
{
    public sealed class RouteEnrichmentProfile : Profile
    {
        public RouteEnrichmentProfile()
        {
            CreateMap<WeatherForecastDto, Weather>()
                .ForMember(weather => weather.Id, options => options.Ignore())
                .ForMember(weather => weather.CreatedDate, options => options.Ignore())
                .ForMember(weather => weather.UpdatedDate, options => options.Ignore());

            CreateMap<RouteDayEnrichment, TravelDay>()
                .ForMember(day => day.ApproxPrice, options => options.Condition(source => source.ApproxPrice != null))
                .ForMember(day => day.DayDescription, options => options.Condition(source => source.DayDescription != null))
                .ForMember(day => day.Id, options => options.Ignore())
                .ForMember(day => day.CreatedDate, options => options.Ignore())
                .ForMember(day => day.UpdatedDate, options => options.Ignore())
                .ForMember(day => day.User, options => options.Ignore())
                .ForMember(day => day.Accomodation, options => options.Ignore())
                .ForMember(day => day.Breakfast, options => options.Ignore())
                .ForMember(day => day.Lunch, options => options.Ignore())
                .ForMember(day => day.Dinner, options => options.Ignore())
                .ForMember(day => day.FirstPlace, options => options.Ignore())
                .ForMember(day => day.SecondPlace, options => options.Ignore())
                .ForMember(day => day.ThirdPlace, options => options.Ignore())
                .ForMember(day => day.PlaceAfterDinner, options => options.Ignore());
        }
    }
}
