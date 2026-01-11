namespace Lumora.Application.Exceptions
{
    [Serializable]
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
            : base("Failed to login")
        {
        }
    }
}
