using System;
using System.Globalization;
namespace Lumora.Exceptions;

/// <summary>
/// Represents an exception for not found resources.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found.</param>
    /// <param name="entityId">The identifier of the entity that was not found.</param>
    public NotFoundException(string entityName, object entityId)
        : base(FormatMessage(entityName, entityId))
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message and an inner exception.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the name of the entity that was not found.
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Gets the identifier of the entity that was not found.
    /// </summary>
    public object? EntityId { get; }

    /// <summary>
    /// Formats the error message.
    /// </summary>
    /// <param name="entityName">The name of the entity.</param>
    /// <param name="entityId">The identifier of the entity.</param>
    /// <returns>A formatted error message.</returns>
    private static string FormatMessage(string entityName, object entityId)
    {
        return string.Format(CultureInfo.InvariantCulture, "The {0} with identifier '{1}' was not found.", entityName, entityId);
    }
}
