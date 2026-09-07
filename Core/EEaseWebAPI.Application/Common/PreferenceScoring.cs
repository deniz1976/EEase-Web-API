namespace EEaseWebAPI.Application.Common
{
    public static class PreferenceScoring
    {
        public const int MinScore = 0;
        public const int MaxScore = 100;

        private const double LearningRate = 0.25d;

        public static int Reinforce(int? current) => Move(current, MaxScore);

        public static int Weaken(int? current) => Move(current, MinScore);

        private static int Move(int? current, int target)
        {
            var score = Math.Clamp(current ?? MinScore, MinScore, MaxScore);

            if (score == target)
            {
                return score;
            }

            var delta = (target - score) * LearningRate;

            var step = target > score
                ? Math.Max(1, (int)Math.Round(delta))
                : Math.Min(-1, (int)Math.Round(delta));

            return Math.Clamp(score + step, MinScore, MaxScore);
        }
    }
}
