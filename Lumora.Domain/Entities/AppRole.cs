namespace Lumora.Domain.Entities
{
    public class AppRole
    {
        public AppRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public AppRole(string roleName)
            : this()
        {
            Name = roleName;
        }

        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }
}
