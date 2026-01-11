namespace Lumora.Application.Interfaces
{
    public interface IContactRepository
    {
        Task<Contact?> GetByEmailAsync(string email);
        Task<Contact?> GetWithPendingSchedulesAsync(string email);
        Task UpsertAsync(Contact contact);
        Task UpsertRangeAsync(List<Contact> contacts);
        Task AddUnsubscribeAsync(Unsubscribe unsubscribe);
        Task AddContactScheduleAsync(ContactEmailSchedule schedule);

        // Unsubscribe logic
        Task UpdateSchedulesStatusAsync(int contactId, ScheduleStatus fromStatus, ScheduleStatus toStatus);
    }
}
