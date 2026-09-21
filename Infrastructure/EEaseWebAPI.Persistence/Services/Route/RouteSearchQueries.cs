namespace EEaseWebAPI.Persistence.Services.Route
{
    internal static class RouteSearchQueries
    {
        public const string Breakfast = "Breakfast restaurant ";
        public const string Lunch = "Lunch restaurant ";
        public const string Dinner = "Dinner restaurant ";

        public static readonly string[] AfterDinner =
        {
            "Live music bars ",
            "Modern rooftop bars ",
            "Famous cocktail bars ",
            "Special dessert shops ",
            "Trendy nightclubs "
        };

        public static readonly string[] Touristic =
        {
            "Touristic places ",
            "Historic landmarks and museums ",
            "Hidden gem sightseeing spots ",
            "Breathtaking natural attractions ",
            "Off the beaten path tourist spots "
        };

        public static readonly string[] TouristicWidening =
        {
            "Famous tourist attractions",
            "Must visit spots",
            "Top rated attractions"
        };

        public static readonly string[] TouristicAlternatives =
        {
            "Famous tourist attractions",
            "Must-see places",
            "Popular tourist destinations",
            "Top-rated places to visit",
            "Cultural attractions",
            "Historical sites"
        };

        public static readonly string[] TouristicLastResort =
        {
            "Must visit attractions",
            "Top attractions",
            "Things to do in",
            "Popular sites",
            "Tourist sites",
            "Landmarks in",
            "Famous places in"
        };

        public static readonly string[] TouristicHiddenGems =
        {
            "Hidden gems in",
            "Off the beaten path in",
            "Unusual attractions in",
            "Secret spots in",
            "Lesser known attractions in",
            "Local favorites in"
        };

        public static readonly string[] AfterDinnerLastResort =
        {
            "Evening entertainment",
            "Nightlife",
            "Bars",
            "Pubs",
            "Cafes",
            "Late night venues"
        };

        public static IEnumerable<string> In(this IEnumerable<string> queries, string destination) =>
            queries.Select(query => $"{query.TrimEnd()} in {destination}");
    }
}
