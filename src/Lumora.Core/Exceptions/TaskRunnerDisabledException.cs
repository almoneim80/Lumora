namespace Lumora.Exceptions;
[Serializable]
public class TaskRunnerDisabledException : Exception
{
    public TaskRunnerDisabledException()
        : base("Task Runner is not enabled")
    {
    }
}
