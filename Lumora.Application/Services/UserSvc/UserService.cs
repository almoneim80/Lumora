namespace Lumora.Application.Services.UserSvc
{
    public class UserService(
        AuthenticationMessage messages,
        ILogger<UserService> logger,
        IUserRepository repository) : IUserService
    {
        private readonly ILogger<UserService> _logger = logger;
        protected readonly IUserRepository _repository = repository;

        /// <inheritdoc/>
        public async Task<GeneralResult<User>> FindUserAsync(CancellationToken cancellationToken, string? email = null, string? phoneNumber = null, string? userId = null, bool requirePhoneConfirmed = true, bool isAdmin = false)
        {
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var user = await _repository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
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
                var user = await _repository.GetByIdAsync(userId);
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
                var user = await _repository.GetByEmailAsync(email);
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

                if (!isAdmin)
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
                var user = await _repository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
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
                var user = await _repository.GetByIdAsync(userId);
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
            var user = await _repository.GetByIdAsync(userId);
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
            return await _repository.ExistsByPhoneAsync(phoneNumber);
        }

        /// <inheritdoc/>
        public async Task<bool> ExsistByIdAsync(string userId)
        {
            return await _repository.ExistsByIdActiveAsync(userId);
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<ListUsersDto>>> GetUsersBasedOnActivationStatus(
            PaginationRequestDto pagination, CancellationToken cancellationToken, bool isActive)
        {
            try
            {
                var query = await _repository.GetAllPagedAsync(pagination, isActive);

                if (!query.Items.Any())
                {
                    _logger.LogWarning("GetUsersBasedOnActivationStatus: No users found. IsActive={IsActive}", isActive);
                    return new GeneralResult<PagedResult<ListUsersDto>>(false, messages.MsgNoUsers, null, ErrorType.NotFound);
                }

                return new GeneralResult<PagedResult<ListUsersDto>>(true, messages.MsgAllUsersRetrieved, query, ErrorType.Success);
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

                var user = await _repository.GetByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        FullName = email,
                        Email = email,
                        CreatedAt = DateTimeOffset.UtcNow,
                    };

                    var result = await _repository.CreateAsync(user);
                    if (!result)
                    {
                        _logger.LogError("Failed to create user with email {Email}.", email);
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
