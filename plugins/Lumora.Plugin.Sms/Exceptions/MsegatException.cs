namespace Lumora.Plugin.Sms.Exceptions;

[Serializable]
public class MsegatException : Exception
{
    public MsegatException()
    {
    }

    public MsegatException(string? message)
        : base(message)
    {
    }

    public MsegatException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
