﻿namespace Mensch.Id.API.Models;

public class LocalAnonymousAccount : Account
{
    public override AccountType AccountType => AccountType.LocalAnonymous;
    public string PasswordHash { get; set; }
    public string PasswordResetToken { get; set; }
}