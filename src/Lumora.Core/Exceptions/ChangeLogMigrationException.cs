namespace Lumora.Exceptions;

public class ChangeLogMigrationException : Exception
{
    public ChangeLogMigrationException(string message)
        : base(message)
    {
    }
}
