namespace Lumora.Web.Controllers.Authentication
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    public class AccountManagementController(
        IMapper mapper,
        UserManager<User> userManager,
        ILogger<AccountManagementController> logger,
        IAuthenticationService authenticationService,
        AuthenticationMessage messages,
        FileValidatorHelper fileValidator,
        ICascadeDeleteService cascadeDeleteService) : AuthenticatedController
    {
        protected readonly IMapper _mapper = mapper;
        protected readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<AccountManagementController> _logger = logger;
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly ICascadeDeleteService _cascadeDeleteService = cascadeDeleteService;
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Change the phone number associated with the currently authenticated user.
        /// </summary>
        [HttpPost("change-phone")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePhoneNumber([FromBody] ChangePhoneNumberDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _authenticationService.ChangePhoneNumberAsync(CurrentUserId!, dto.PhoneNumber, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Change-Phone - An Unexpected error occurred while change phone.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("change phone."), Data = null });
            }
        }

        /// <summary>
        /// Update current authenticated user's details.
        /// </summary>
        [HttpPatch("update-profile")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromForm] UserUpdateFormDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                string? avatarFileName = null;

                if (dto.AvatarFile is not null)
                {
                    var fileResult = dto.AvatarFile.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                    if (!fileResult.IsValid)
                    {
                        _logger.LogWarning("UpdateProfile: Avatar file validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                    }

                    avatarFileName = fileResult.UniqueName;

                    // TODO: Upload avatar
                    // await _fileStorage.UploadAsync(fileResult.Stream, avatarFileName, "avatars");
                }

                var updateDto = new UserUpdateDto
                {
                    FullName = dto.FullName,
                    City = dto.City,
                    Sex = dto.Sex,
                    AboutMe = dto.AboutMe,
                    DateOfBirth = dto.DateOfBirth,
                    Avatar = avatarFileName,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                var result = await _authenticationService.UpdateProfileAsync(CurrentUserId!, updateDto, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update-Profile - An Unexpected error occurred while update profile.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("update profile."), Data = null });
            }
        }

        /// <summary>
        /// Complete current authenticated user's details.
        /// </summary>
        [HttpPatch("complete-profile")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteProfile([FromForm] CompleteUserDataFormDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                string? avatarFileName = null;
                if (dto.AvatarFile is not null)
                {
                    var fileResult = dto.AvatarFile.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                    if (!fileResult.IsValid)
                    {
                        _logger.LogWarning("CompleteProfile: Avatar file validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                    }

                    avatarFileName = fileResult.UniqueName;
                }

                var completeDto = new CompleteUserDataDto
                {
                    DateOfBirth = dto.DateOfBirth,
                    AboutMe = dto.AboutMe,
                    Avatar = avatarFileName ?? null
                };

                var result = await _authenticationService.CompleteProfileAsync(CurrentUserId!, completeDto, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Complete-Profile - An Unexpected error occurred while complete profile.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("complete profile."), Data = null });
            }
        }

        /// <summary>
        /// Returns current user details.
        /// </summary>
        [HttpGet("my-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MyData(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _authenticationService.GetProfileAsync(CurrentUserId!, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "My-Data - An Unexpected error occurred while get my data.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("my profile."), Data = null });
            }
        }

        /// <summary>
        /// Soft deletes an entity and its related entities with cascading soft delete.
        /// </summary>
        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWithCascade([FromBody] DeleteAccountDto dto, CancellationToken cancellationToken)
        {
            var userCheck = CheckUserOrUnauthorized();
            if (userCheck != null) return userCheck;

            var transactionId = Guid.NewGuid();
            _logger.LogInformation("Transaction {TransactionId}: Starting soft delete for entity ID {Id}.", transactionId, CurrentUserId);
            try
            {
                var result = await _cascadeDeleteService.SoftDeleteUserCascadeAsync(dto.Password, CurrentUserId!, cancellationToken);
                //if (result.IsSuccess == false)
                //{
                //    return result.ErrorType switch
                //    {
                //        ErrorType.BadRequest => BadRequest(result),
                //        ErrorType.NotFound => NotFound(result),
                //        ErrorType.InternalServerError => StatusCode(500, result),
                //        _ => BadRequest(result)
                //    };
                //}

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction {TransactionId}: Unexpected error occurred while deleting entity ID {Id}.", transactionId, CurrentUserId);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("delete user."), Data = null });
            }
        }
    }
}
