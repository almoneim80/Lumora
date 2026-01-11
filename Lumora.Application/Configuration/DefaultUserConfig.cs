namespace Lumora.Application.Configuration
{
    public class DefaultUserConfig
    {
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool PhoneNumberConfirmed { get; set; } = true;
        public bool EmailConfirmed { get; set; } = true;

        public DefaultRolesConfig Roles { get; set; } = new DefaultRolesConfig();
    }
}
