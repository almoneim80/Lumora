namespace Lumora.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequiredPermissionAttribute : Attribute
    {
        public RequiredPermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}
