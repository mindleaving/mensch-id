namespace Mensch.Id.API.Models
{
    public class IsLoggedInResponse
    {
        public IsLoggedInResponse(AccountType accountType)
        {
            AccountType = accountType;
        }

        public AccountType AccountType { get; set; }
    }
}
