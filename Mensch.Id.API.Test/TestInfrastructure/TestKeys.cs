using System;
using Mensch.Id.API.AccessControl;

namespace Mensch.Id.API.Test.TestInfrastructure;

public static class TestKeys
{
    public const string ObscureKeyBase64 = "q3mJk1Yb0x8C4o2lH7l5wqO9b3a1VdJ7Tz6m8tq0Kxw=";
    public static readonly byte[] ObscureKey = Convert.FromBase64String(ObscureKeyBase64);

    public static ExternalLoginObscurer CreateObscurer()
    {
        return new ExternalLoginObscurer(ObscureKey);
    }
}
