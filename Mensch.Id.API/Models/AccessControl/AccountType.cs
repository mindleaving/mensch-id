namespace Mensch.Id.API.Models.AccessControl
{
    public enum AccountType
    {
        Local = 1,
        LocalAnonymous = 2,
        External = 3,
        Assigner = 4,
        Admin = 99
    }
}