using System.Text.RegularExpressions;

namespace Mensch.Id.API.Helpers
{
    public static class ControllerInputSanitizer
    {
        public static bool ValidateAndSanitizeOptionalId(
            string input,
            out string output)
        {
            if (string.IsNullOrEmpty(input))
            {
                output = null;
                return true;
            }
            return ValidateAndSanitizeMandatoryId(input, out output);
        }

        public static bool ValidateAndSanitizeMandatoryId(
            string input,
            out string output)
        {
            if (input == null)
            {
                output = null;
                return false;
            }
            var lowerCase = input.ToLowerInvariant();
            if (lowerCase == "undefined" || lowerCase == "null")
            {
                output = null;
                return false;
            }
            if (Regex.IsMatch(lowerCase, "^[a-z0-9-]{1,64}$"))
            {
                output = lowerCase;
                return true;
            }
            var upperCase = input.ToUpperInvariant();
            if (Regex.IsMatch(upperCase, "^[0-9]{8}-[A-Z0-9]{5}\\|G[0-9]+$"))
            {
                output = upperCase;
                return true;
            }

            output = null;
            return false;
        }
    }
}
