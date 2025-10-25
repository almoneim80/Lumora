using Lumora.Interfaces.Customer;

namespace Lumora.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<IdentityService> _logger;
        public IdentityService(UserManager<User> userManager, ILogger<IdentityService> logger, IUserService userService)
        {
            _userManager = userManager;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Finds a user by email during registration, creates a new user if not found.
        /// </summary>
        public async Task<User> FindOnRegister(string phoneNumber, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    throw new ArgumentException("PhoneNumber cannot be null or empty.", nameof(phoneNumber));
                }

                var user = await _userService.FindUserAsync(cancellationToken, null, phoneNumber, null, true);
                if (user.Data == null)
                {
                    user.Data = new User
                    {
                        FullName = phoneNumber,
                        PhoneNumber = phoneNumber,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var result = await _userManager.CreateAsync(user.Data);

                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to create user with PhoneNumber {PhoneNumber}. Errors: {Errors}", phoneNumber, result.Errors);
                        throw new IdentityException(result.Errors);
                    }

                    _logger.LogInformation("User with PhoneNumber {PhoneNumber} created successfully.", phoneNumber);
                }
                else
                {
                    _logger.LogInformation("User with PhoneNumber {PhoneNumber} found.", phoneNumber);
                }

                return user.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FindOnRegister for phoneNumber {phoneNumber}.", phoneNumber);
                throw;
            }
        }

        /// <summary>
        /// Creates a ClaimsPrincipal for a user.
        /// </summary>
        /// <param name="user">The user for whom the ClaimsPrincipal is created.</param>
        /// <returns>A ClaimsPrincipal containing the user's claims.</returns>
        public async Task<ClaimsPrincipal> CreateUserClaimsPrincipal(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null.");
                }

                var claims = await CreateUserClaims(user);

                var identity = new ClaimsIdentity(claims);
                _logger.LogInformation("ClaimsPrincipal created successfully for user {UserId}.", user.Id);

                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ClaimsPrincipal for user {UserId}.", user?.Id);
                throw;
            }
        }

        /// <summary>
        /// Creates a list of claims for a user.
        /// </summary>
        /// <param name="user">The user for whom the claims are created.</param>
        /// <returns>A list of claims for the user.</returns>
        public async Task<List<Claim>> CreateUserClaims(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null.");
                }

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("LoginProvider", "Google"),
            };

                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                _logger.LogInformation("Claims created successfully for user {UserId}.", user.Id);
                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claims for user {UserId}.", user?.Id);
                throw;
            }
        }
    }
}

