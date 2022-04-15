namespace Mensch.Id.API.Models
{
    public enum LoginProvider
    {
        Unknown = 0, // For validation
        Google = 1,
        Twitter = 2,
        Facebook = 3,
        Microsoft = 4,
        LocalJwt = 5
    }
}