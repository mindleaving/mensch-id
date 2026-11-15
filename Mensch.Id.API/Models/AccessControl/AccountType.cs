namespace Mensch.Id.API.Models.AccessControl
{
    public enum AccountType
    {
        Local = 1,
        LocalAnonymous = 2,
        External = 3,
        Assigner = 4,
        ExternalService = 5,
        Admin = 99
    }
}