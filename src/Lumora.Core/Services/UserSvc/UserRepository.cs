using Lumora.Interfaces.UserIntf;

namespace Lumora.Services.UserSvc
{
    public class UserRepository : IUserRepository
    {
        private readonly PgDbContext _dbContext;

        public UserRepository(PgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive);
        }
    }
}
