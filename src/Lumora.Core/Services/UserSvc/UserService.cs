using Lumora.DTOs.Authentication;
using Lumora.Extensions;
using Lumora.Interfaces.Customer;
using Lumora.Services.Core.Messages;
namespace Lumora.Services.Customer
{
    public class UserService(
        UserManager<User> userManager,
        AuthenticationMessage messages,
        ILogger<UserService> logger,
        PgDbContext dbContext) : IUserService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<UserService> _logger = logger;
        protected readonly PgDbContext _dbContext = dbContext;

        /// <inheritdoc/>
        public async Task<GeneralResult<User>> FindUserAsync(CancellationToken cancellationToken, string? email = null, string? phoneNumber = null, string? userId = null, bool requirePhoneConfirmed = true, bool isAdmin = false)
        {
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("User not found. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (user.IsDeleted == true)
                {
                    _logger.LogWarning("User is deleted. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("User is inactive. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgAccountInactive, null, ErrorType.BadRequest);
                }

                if (!isAdmin)
                {
                    if (!user.PhoneNumberConfirmed)
                    {
                        _logger.LogWarning("User Phone is not confirmed. ID={UserId}", userId);
                        return new GeneralResult<User>(false, messages.MsgPhoneNotConfirmed, null, ErrorType.BadRequest);
                    }
                }

                return new GeneralResult<User>(true, "", user, ErrorType.Success);
            }
            else if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (user.IsDeleted == true)
                {
                    _logger.LogWarning("User is deleted. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("User is inactive. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgAccountInactive, null, ErrorType.BadRequest);
                }

                if (!isAdmin)
                {
                    if (!user.PhoneNumberConfirmed)
                    {
                        _logger.LogWarning("User Phone is not confirmed. ID={UserId}", userId);
                        return new GeneralResult<User>(false, messages.MsgPhoneNotConfirmed, null, ErrorType.BadRequest);
                    }
                }

                return new GeneralResult<User>(true, "", user, ErrorType.Success);
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("User not found. email={email}", email);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (user.IsDeleted == true)
                {
                    _logger.LogWarning("User is deleted. email={email}", email);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("User is inactive. email={email}", email);
                    return new GeneralResult<User>(false, messages.MsgAccountInactive, null, ErrorType.BadRequest);
                }

                if(!isAdmin)
                {
                    if (!user.PhoneNumberConfirmed)
                    {
                        _logger.LogWarning("User Phone is not confirmed. email={email}", email);
                        return new GeneralResult<User>(false, messages.MsgPhoneNotConfirmed, null, ErrorType.BadRequest);
                    }
                }

                return new GeneralResult<User>(true, "", user, ErrorType.Success);
            }
            else
            {
                return new GeneralResult<User>(false, messages.MsgPhoneNumberOrUserIdRequired, null, ErrorType.BadRequest);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<User>> FindUserWithoutPhoneNumberConfirmedAsync(CancellationToken cancellationToken, string? phoneNumber = null, string? userId = null)
        {
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("User not found. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (user.IsDeleted == true)
                {
                    _logger.LogWarning("User is deleted. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("User is inactive. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgAccountInactive, null, ErrorType.BadRequest);
                }

                return new GeneralResult<User>(true, "", user, ErrorType.Success);
            }
            else if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (user.IsDeleted == true)
                {
                    _logger.LogWarning("User is deleted. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("User is inactive. ID={UserId}", userId);
                    return new GeneralResult<User>(false, messages.MsgAccountInactive, null, ErrorType.BadRequest);
                }

                return new GeneralResult<User>(true, "", user, ErrorType.Success);
            }
            else
            {
                return new GeneralResult<User>(false, messages.MsgPhoneNumberOrUserIdRequired, null, ErrorType.BadRequest);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<User>> GetUserByIdWithoutActiveValidation(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found. ID={UserId}", userId);
                return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
            }

            if (user.IsDeleted == true)
            {
                _logger.LogWarning("User is deleted. ID={UserId}", userId);
                return new GeneralResult<User>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
            }

            return new GeneralResult<User>(true, "", user, ErrorType.Success);
        }

        /// <inheritdoc/>
        public async Task<bool> ExsistByPhoneNumberAsync(string phoneNumber)
        {
            return await _userManager.Users.AsNoTracking().AnyAsync(u =>
            u.PhoneNumber == phoneNumber &&
            !u.IsDeleted &&
            u.IsActive);
        }

        /// <inheritdoc/>
        public async Task<bool> ExsistByIdAsync(string userId)
        {
            return await _userManager.Users.AsNoTracking().AnyAsync(u =>
            u.Id == userId &&
            !u.IsDeleted &&
            u.PhoneNumberConfirmed &&
            u.IsActive);
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<ListUsersDto>>> GetAllUsersAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var query = _userManager.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted);

                var pagedUsers = await query
                    .Select(u => new ListUsersDto
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
                    })
                    .ApplyPaginationAsync(pagination, cancellationToken);

                if (!pagedUsers.Items.Any())
                {
                    _logger.LogWarning("GetAllUsersAsync: No users found.");
                    return new GeneralResult<PagedResult<ListUsersDto>>(false, messages.MsgNoUsers, null, ErrorType.NotFound);
                }

                return new GeneralResult<PagedResult<ListUsersDto>>(true, messages.MsgAllUsersRetrieved, pagedUsers, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllUsersAsync: Error retrieving all users.");
                return new GeneralResult<PagedResult<ListUsersDto>>(false, messages.GetUnexpectedErrorMessage("retrieving all users"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<ListUsersDto>>> GetUsersBasedOnActivationStatus(
            PaginationRequestDto pagination, CancellationToken cancellationToken, bool isActive)
        {
            try
            {
                var query = _userManager.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted && u.IsActive == isActive);

                var pagedUsers = await query
                    .Select(u => new ListUsersDto
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
                    })
                    .ApplyPaginationAsync(pagination, cancellationToken);

                if (!pagedUsers.Items.Any())
                {
                    _logger.LogWarning("GetUsersBasedOnActivationStatus: No users found. IsActive={IsActive}", isActive);
                    return new GeneralResult<PagedResult<ListUsersDto>>(false, messages.MsgNoUsers, null, ErrorType.NotFound);
                }

                return new GeneralResult<PagedResult<ListUsersDto>>(true, messages.MsgAllUsersRetrieved, pagedUsers, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUsersBasedOnActivationStatus: An error occurred while retrieving users. IsActive={IsActive}", isActive);
                return new GeneralResult<PagedResult<ListUsersDto>>(false, messages.GetUnexpectedErrorMessage("retrieving users"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<User> FindOnRegister(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Email cannot be null or empty.");
                    return new User();
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        FullName = email,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to create user with email {Email}. Errors: {Errors}", email, result.Errors);
                        return new User();
                    }

                    _logger.LogInformation("User with email {Email} created successfully.", email);
                    return user;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FindOnRegister for email {Email}.", email);
                return new User();
            }
        }
    }
}
