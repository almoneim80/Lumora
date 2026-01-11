namespace Lumora.Infrastructure.Exceptions
{
    [Serializable]
    public class NonPrimaryNodeException : Exception
    {
        public NonPrimaryNodeException()
            : base("This is not the current primary node for task execution")
        {
        }
    }
}
