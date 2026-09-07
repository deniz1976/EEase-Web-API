namespace EEaseWebAPI.Application.DTOs.Route
{
    public sealed class PlacePicker
    {
        private readonly HashSet<string> _usedGoogleIds = new();
        private readonly Random _random;

        public PlacePicker(Random random, IEnumerable<string>? alreadyUsed = null)
        {
            _random = random;

            foreach (var googleId in alreadyUsed ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(googleId))
                {
                    _usedGoogleIds.Add(googleId);
                }
            }
        }

        public IReadOnlyCollection<string> UsedGoogleIds => _usedGoogleIds;

        public bool IsUsed(string googleId) => _usedGoogleIds.Contains(googleId);

        public bool MarkUsed(string googleId) => _usedGoogleIds.Add(googleId);

        public string? Take(IReadOnlyList<string> pool)
        {
            var available = Available(pool);

            return available.Count == 0 ? null : Claim(available[_random.Next(available.Count)]);
        }

        public string? TakeAt(IReadOnlyList<string> pool, int offset)
        {
            var available = Available(pool);

            if (available.Count == 0)
            {
                return null;
            }

            var index = (offset % available.Count + available.Count) % available.Count;

            return Claim(available[index]);
        }

        private List<string> Available(IReadOnlyList<string> pool) =>
            (pool ?? Array.Empty<string>())
                .Where(googleId => !string.IsNullOrWhiteSpace(googleId) && !_usedGoogleIds.Contains(googleId))
                .Distinct()
                .ToList();

        private string Claim(string googleId)
        {
            _usedGoogleIds.Add(googleId);

            return googleId;
        }
    }
}
