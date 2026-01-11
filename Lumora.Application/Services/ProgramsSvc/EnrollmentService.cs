namespace Lumora.Application.Services.Programs
{
    public class EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        ITrainingProgramRepository programRepository,
        IUnitOfWork unitOfWork,
        ILogger<EnrollmentService> logger,
        EnrollmentMessage messages,
        IUserRepository userRepository,
        IRoleService roleService,
        RoleMessages roleMessages) : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository = enrollmentRepository;
        private readonly ITrainingProgramRepository _programRepository = programRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<EnrollmentService> _logger = logger;
        private readonly EnrollmentMessage _messages = messages;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRoleService _roleService = roleService;

        /// <inheritdoc/>
        public async Task<GeneralResult> EnrollInProgramAsync(int programId, string userId, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("TrainingProgramService - EnrollInProgramAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _userRepository.ExistsByIdActiveAsync(userId);
                if (!user)
                {
                    _logger.LogInformation("TrainingProgramService - EnrollInProgramAsync : User not found.");
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (programId <= 0)
                {
                    _logger.LogWarning("TrainingProgramService - EnrollInProgramAsync : Invalid ProgramId {ProgramId} provided.", programId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var program = await _programRepository.GetByIdAsync(programId, cancellationToken);
                if (program == null)
                {
                    _logger.LogInformation("TrainingProgramService - EnrollInProgramAsync : Program not found.");
                    return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                }

                var enrollment = await _enrollmentRepository.GetEnrollmentAsync(userId, programId, cancellationToken);
                if (enrollment != null && enrollment.EnrollmentStatus == EnrollmentStatus.Active)
                {
                    return new GeneralResult(false, _messages.MsgAlreadyEnrolled, null, ErrorType.Conflict);
                }

                // TODO: check is paid
                //var hasPaid = await _paymentVerifier.HasUserPaidForAsync(userId, PaymentItemType.Program, programId);
                //if (!hasPaid)
                //{
                //    _logger.LogInformation("HasUserPaidForAsync: No payment found for UserId {UserId}, ItemType {Type}, ItemId {ItemId}",
                //        userId, PaymentItemType.Program, programId);
                //    return new GeneralResult(true, _payMessages.MsgPaymentNotFoundForItem, false);
                //}

                var result = new ProgramEnrollment
                {
                    ProgramId = programId,
                    UserId = userId,
                    EnrolledAt = DateTimeOffset.UtcNow,
                    EnrollmentStatus = EnrollmentStatus.Active
                };

                _enrollmentRepository.Add(result);

                if ((await _roleService.IsUserInRoleAsync(userId, AppRoles.Student, cancellationToken)).Data == false)
                {
                    if ((await _roleService.AssignRoleAsync(userId, AppRoles.Student)).IsSuccess == false)
                    {
                        _logger.LogWarning("RegisterAsync - Failed to assign User role. UserId: {UserId}", userId);
                        await transaction.RollbackAsync(cancellationToken);
                        return new GeneralResult(false, roleMessages.MsgAssignRoleFailed, null, ErrorType.InternalServerError);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("TrainingProgramService - EnrollInProgramAsync : Enrolled in program successfully.");
                return new GeneralResult(true, _messages.MsgEnrolledSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in program {ProgramId} for user {UserId}.", programId, userId);
                await transaction.RollbackAsync(cancellationToken);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Enroll In Program"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<EnrollmentWithUserData>>> GetEnrolledUsersAsync(int programId, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _enrollmentRepository.GetEnrolledUsersAsync(programId, cancellationToken);

                if (!users.Any())
                {
                    _logger.LogWarning("No enrolled users found for program {ProgramId}.", programId);
                    return new GeneralResult<List<EnrollmentWithUserData>>(false, _messages.MsgNoEnrolledUsers, null, ErrorType.NotFound);
                }

                _logger.LogInformation("Enrolled users retrieved successfully for program {ProgramId}.", programId);
                return new GeneralResult<List<EnrollmentWithUserData>>(true, _messages.MsgEnrolledUserRetrieved, users, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrolled users for program {ProgramId}.", programId);
                return new GeneralResult<List<EnrollmentWithUserData>>(
                    false, _messages.GetUnexpectedErrorMessage("Get Enrolled Users"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<bool>> IsUserEnrolledAsync(string userId, int programId, CancellationToken cancellationToken)
        {
            try
            {
                var isEnrolled = await _enrollmentRepository.IsEnrolledAsync(userId, programId, cancellationToken);

                if (!isEnrolled)
                {
                    _logger.LogInformation("User {UserId} is not enrolled in program {ProgramId}.", userId, programId);
                    return new GeneralResult<bool>(true, _messages.MsgUserNotEnrolled, isEnrolled, ErrorType.NotFound);
                }

                _logger.LogInformation("User {UserId} is enrolled in program {ProgramId}.", userId, programId);
                return new GeneralResult<bool>(true, _messages.MsgEnrolledSuccess, isEnrolled, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment for user {UserId} in program {ProgramId}.", userId, programId);
                return new GeneralResult<bool>(false, _messages.GetUnexpectedErrorMessage("Check Enrollment"), false, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<EnrollmentWithUserData>> GetUserEnrollmentInfoAsync(string userId, int programId, CancellationToken cancellationToken)
        {
            try
            {
                var enrollment = await _enrollmentRepository.GetEnrollmentAsync(userId, programId, cancellationToken);

                if (enrollment == null)
                {
                    _logger.LogInformation("EnrollmentService - GetUserEnrollmentInfoAsync : No enrollment found for user {UserId} in program {ProgramId}.", userId, programId);
                    return new GeneralResult<EnrollmentWithUserData>(false, _messages.MsgNoEnrolledUsers, null, ErrorType.NotFound);
                }

                var enrollments = new EnrollmentWithUserData
                {
                    FullName = enrollment.User.FullName,
                    Email = enrollment.User.Email,
                    EnrolledAt = enrollment.CreatedAt,
                    EnrollmentStatus = enrollment.EnrollmentStatus,
                };

                _logger.LogInformation("EnrollmentService - GetUserEnrollmentInfoAsync : Enrollment retrieved successfully for user {UserId} in program {ProgramId}.", userId, programId);
                return new GeneralResult<EnrollmentWithUserData>(true, _messages.MsgEnrolledUserRetrieved, enrollments, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnrollmentService - GetUserEnrollmentInfoAsync : Error retrieving enrollment info for user {UserId} in program {ProgramId}", userId, programId);
                return new GeneralResult<EnrollmentWithUserData>(false, _messages.GetUnexpectedErrorMessage("Get Enrolled User Info."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UnenrollFromProgramAsync(string userId, int programId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return new GeneralResult(false, "User ID is required.");
                var enrollment = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, programId, cancellationToken);

                if (enrollment == null)
                    return new GeneralResult(false, _messages.MsgEnrollmentNotFound, null, ErrorType.NotFound);

                enrollment.EnrollmentStatus = EnrollmentStatus.InActive;
                enrollment.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Unenrolled user {UserId} from program {ProgramId}.", userId, programId);
                return new GeneralResult(true, _messages.MsgUserUnEnrolled, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unenrolling user {UserId}", userId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Unenroll"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateEnrollmentStatusAsync(string userId, int programId, EnrollmentStatus status, CancellationToken cancellationToken)
        {
            try
            {
                var enrollment = await _enrollmentRepository.GetEnrollmentAsync(userId, programId, cancellationToken);

                if (enrollment == null)
                    return new GeneralResult(false, _messages.MsgEnrollmentNotFound, null, ErrorType.NotFound);

                enrollment.EnrollmentStatus = status;
                enrollment.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgEnrollmentStatusUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Update Status"), null, ErrorType.InternalServerError);
            }
        }
    }
}
