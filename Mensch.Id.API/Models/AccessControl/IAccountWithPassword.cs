using System;

namespace Mensch.Id.API.Models.AccessControl;

public interface IAccountWithPassword
{
    string PasswordHash { get; set; }
    int FailedLoginAttempts { get; set; }
    DateTime? LockoutEnd { get; set; }
}