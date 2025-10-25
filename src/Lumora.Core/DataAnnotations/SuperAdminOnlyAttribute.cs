namespace Lumora.DataAnnotations
{
    /// <summary>
    /// This attribute marks a permission as Admin-only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SuperAdminOnlyAttribute : Attribute
    {
        // just an empty Attribute
    }
}
