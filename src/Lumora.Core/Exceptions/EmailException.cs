﻿using System.Runtime.Serialization;

namespace Lumora.Exceptions;

[Serializable]
public class EmailException : Exception
{
    public EmailException()
    {
    }

    public EmailException(string? message)
        : base(message)
    {
    }

    public EmailException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    //protected EmailException(SerializationInfo info, StreamingContext context)
    //    : base(info, context)
    //{
    //}
}
