using Lumora.Application.DTOs.Authentication;
using Lumora.Application.Interfaces.UserIntf;
using Microsoft.AspNetCore.Identity;

namespace Lumora.Infrastructure.Repositories
{
    public class UserRepository(PgDbContext dbContext, UserManager<User> userManager) : IUserRepository
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly UserManager<User> _userManager = userManager;

        public async Task<User?> GetByIdAsync(string userId, CancellationToken ct = default)
            => await _userManager.FindByIdAsync(userId);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _userManager.FindByEmailAsync(email);

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
            => await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);

        public async Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct = default)
            => await _userManager.Users.AsNoTracking().AnyAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted && u.IsActive, ct);

        public async Task<bool> ExistsByIdActiveAsync(string userId, CancellationToken ct = default)
            => await _userManager.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.IsDeleted && u.PhoneNumberConfirmed && u.IsActive, ct);

        public async Task<bool> CreateAsync(User user)
        {
            var result = await _userManager.CreateAsync(user);
            return result.Succeeded;
        }

        public async Task<PagedResult<ListUsersDto>> GetAllPagedAsync(PaginationRequestDto pagination, bool? isActive, CancellationToken ct = default)
        {
            var query = _userManager.Users.AsNoTracking().Where(u => !u.IsDeleted);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            return await query.Select(u => new ListUsersDto
            {
                Id = u.Id,
                FullName = u.FullName ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Email = u.Email ?? string.Empty,
                City = u.City ?? string.Empty,
                Sex = u.Sex ?? string.Empty,
                DateOfBirth = u.DateOfBirth ?? DateTimeOffset.MinValue,
                AboutMe = u.AboutMe ?? string.Empty,
                Avatar = u.Avatar ?? string.Empty,
                IsActive = u.IsActive
            }).ApplyPaginationAsync(pagination, ct);
        }
    }
}
