namespace Mensch.Id.API.Helpers
{
    public class EmailValidator
    {
        /// <summary>
        /// From stackoverflow: https://stackoverflow.com/a/1374644
        /// </summary>
        public static bool IsValidEmailFormat(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith(".")) {
                return false; // suggested by @TK-421
            }
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch {
                return false;
            }
        }
    }
}
