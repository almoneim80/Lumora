namespace Lumora.Exceptions;

[Serializable]
public class TaskNotCompletedException : Exception
{
    public TaskNotCompletedException()
        : base("Another task is not comleted yet")
    {
    }
}
