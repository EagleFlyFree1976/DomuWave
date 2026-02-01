using Hangfire.Annotations;

namespace DomuWave.Services.Exceptions;

public class AuthException : Exception
{
    public AuthException()
    {
    }

    public AuthException([CanBeNull] string message) : base(message)
    {
    }
}