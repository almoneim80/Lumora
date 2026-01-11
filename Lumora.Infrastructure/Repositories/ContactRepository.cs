namespace Lumora.Infrastructure.Repositories
{
    public class ContactRepository(PgDbContext context) : IContactRepository
    {
        private readonly PgDbContext _context = context;

        public async Task UpsertAsync(Contact contact)
        {
            if (contact.Id > 0)
                _context.Contacts!.Update(contact);
            else
                await _context.Contacts!.AddAsync(contact);
        }
        public async Task UpsertRangeAsync(List<Contact> contacts)
        {
            var updates = contacts.Where(c => c.Id > 0).ToList();
            var inserts = contacts.Where(c => c.Id <= 0).ToList();

            if (updates.Any()) _context.UpdateRange(updates);
            if (inserts.Any()) await _context.AddRangeAsync(inserts);
        }

        public async Task<Contact?> GetByEmailAsync(string email)
        {
            return await _context.Contacts!
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Contact?> GetWithPendingSchedulesAsync(string email)
        {
            return await _context.Contacts!
                .Include(c => c.ContactEmailSchedules!)
                    .ThenInclude(s => s.Schedule)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUnsubscribeAsync(Unsubscribe unsubscribe)
        {
            await _context.Unsubscribes!.AddAsync(unsubscribe);
        }

        public async Task AddContactScheduleAsync(ContactEmailSchedule schedule)
        {
            await _context.ContactEmailSchedules!.AddAsync(schedule);
        }

        public async Task UpdateSchedulesStatusAsync(int contactId, ScheduleStatus fromStatus, ScheduleStatus toStatus)
        {
            var schedules = await _context.ContactEmailSchedules!
                .Where(s => s.ContactId == contactId && s.Status == fromStatus)
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                schedule.Status = toStatus;
            }
        }
    }
}
