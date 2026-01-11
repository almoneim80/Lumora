namespace Lumora.Application.Interfaces
{
    public interface IContactService : IEntityService<Contact>
    {
        Task Subscribe(Contact contact, string groupName);

        Task Unsubscribe(string email, string reason, string source, DateTimeOffset createdAt, string? ip);

        Task<Contact> FindOrCreate(string email, string language, int timezone);
    }
}
