namespace EFA.Application.Players.Common
{
    public static class PlayerBirthDateRules
    {
        public static bool IsValid(DateOnly birthDate, out string? error)
        {
            error = null;
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (birthDate > today)
            {
                error = "Birth date cannot be in the future.";
                return false;
            }

            var latestAllowedBirthDate = today.AddYears(-5);

            if (birthDate > latestAllowedBirthDate)
            {
                error = "Player must be at least 5 years old.";
                return false;
            }

            return true;
        }
    }
}
