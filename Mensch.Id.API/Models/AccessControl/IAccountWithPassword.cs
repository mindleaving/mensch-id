namespace Mensch.Id.API.Models.AccessControl;

public interface IAccountWithPassword
{
    string PasswordHash { get; set; }
}