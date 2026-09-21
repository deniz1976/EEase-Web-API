namespace EEaseWebAPI.Application.Validators.User
{
    public static class UserProfileRules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 16;
        public const int UsernameMinLength = 3;
        public const int BioMaxLength = 80;
        public const int MinimumAge = 13;

        public static readonly string[] Genders = { "Male", "Female" };

        public static bool IsKnownGender(string? gender) => Genders.Contains(gender);

        public static bool IsOldEnough(DateOnly bornDate) => AgeOn(bornDate) >= MinimumAge;

        public static int AgeOn(DateOnly bornDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - bornDate.Year;

            if (bornDate.AddYears(age) > today)
            {
                age--;
            }

            return age;
        }
    }
}
