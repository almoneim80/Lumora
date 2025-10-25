using System;
using System.Runtime.Serialization;

namespace Lumora.Exceptions;

[Serializable]
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Failed to login")
    {
    }
}
