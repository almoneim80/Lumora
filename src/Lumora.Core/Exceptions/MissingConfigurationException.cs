using System.Runtime.Serialization;

namespace Lumora.Exceptions;

[Serializable]
public class MissingConfigurationException : Exception
{
    public MissingConfigurationException()
    {
    }

    public MissingConfigurationException(string? message)
        : base(message)
    {
    }

    public MissingConfigurationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    //protected MissingConfigurationException(SerializationInfo info, StreamingContext context)
    //    : base(info, context)
    //{
    //}
}
