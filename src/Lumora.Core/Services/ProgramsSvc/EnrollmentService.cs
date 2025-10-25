using Lumora.Entities.Tables;
using Lumora.Interfaces.PaymentIntf;
using Lumora.Interfaces.ProgramIntf;
using Lumora.Interfaces.UserIntf;

namespace Lumora.Services.Programs
{
    public class EnrollmentService
        (PgDbContext dbContext,
        ILogger<EnrollmentService> logger,
        IMapper mapper,
        PaymentMessage payMessages,
        IRoleService roleService,
        RoleMessages roleMessages,
        EnrollmentMessage messages,
        IUserRepository userRepository,
        IPaymentVerifier paymentVerifier) : IEnrollmentService
    {
        private readonly IMapper _mapper = mapper;
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<EnrollmentService> _logger = logger;
        private readonly EnrollmentMessage _messages = messages;
        private readonly PaymentMessage _payMessages = payMessages;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPaymentVerifier _paymentVerifier = paymentVerifier;
        private readonly IRoleService _roleService = roleService;

        /// <inheritdoc/>
        public async Task<GeneralResult> EnrollInProgramAsync(int programId, string userId, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if(string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("TrainingProgramService - EnrollInProgramAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _userRepository.ExistsAsync(userId);
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

                var program = await _dbContext.TrainingPrograms
                    .FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, cancellationToken);
                if (program == null)
                {
                    _logger.LogInformation("TrainingProgramService - EnrollInProgramAsync : Program not found.");
                    return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                }

                var enrollment = await _dbContext.ProgramEnrollments
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.ProgramId == programId && !e.IsDeleted, cancellationToken);
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

                _dbContext.ProgramEnrollments.Add(result);

                if ((await _roleService.IsUserInRoleAsync(userId, AppRoles.Student, cancellationToken)).Data == false)
                {
                    if ((await _roleService.AssignRoleAsync(userId, AppRoles.Student)).IsSuccess == false)
                    {
                        _logger.LogWarning("RegisterAsync - Failed to assign User role. UserId: {UserId}", userId);
                        await transaction.RollbackAsync(cancellationToken);
                        return new GeneralResult(false, roleMessages.MsgAssignRoleFailed, null, ErrorType.InternalServerError);
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
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
                var users = await _dbContext.ProgramEnrollments.AsNoTracking()
                    .Include(e => e.User)
                    .Where(e => e.ProgramId == programId && !e.IsDeleted && e.EnrollmentStatus == EnrollmentStatus.Active)
                    .Select(e => new EnrollmentWithUserData
                    {
                        FullName = e.User.FullName,
                        Email = e.User.Email,
                        EnrolledAt = e.EnrolledAt,
                        EnrollmentStatus = e.EnrollmentStatus
                    }).ToListAsync(cancellationToken);

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
                var isEnrolled = await _dbContext.ProgramEnrollments.AsNoTracking()
                    .AnyAsync(e => e.ProgramId == programId && e.UserId == userId &&
                    !e.IsDeleted && e.EnrollmentStatus == EnrollmentStatus.Active, cancellationToken);

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
                var enrollment = await _dbContext.ProgramEnrollments.AsNoTracking()
                    .Where(e => e.UserId == userId && e.ProgramId == programId && !e.IsDeleted && e.EnrollmentStatus == EnrollmentStatus.Active)
                    .Select(e => new EnrollmentWithUserData
                    {
                        FullName = e.User.FullName,
                        Email = e.User.Email,
                        EnrolledAt = e.EnrolledAt,
                        EnrollmentStatus = e.EnrollmentStatus
                    }).FirstOrDefaultAsync(cancellationToken);

                if (enrollment == null)
                {
                    _logger.LogInformation("EnrollmentService - GetUserEnrollmentInfoAsync : No enrollment found for user {UserId} in program {ProgramId}.", userId, programId);
                    return new GeneralResult<EnrollmentWithUserData>(false, _messages.MsgNoEnrolledUsers, null, ErrorType.NotFound);
                }

                _logger.LogInformation("EnrollmentService - GetUserEnrollmentInfoAsync : Enrollment retrieved successfully for user {UserId} in program {ProgramId}.", userId, programId);
                return new GeneralResult<EnrollmentWithUserData>(true, _messages.MsgEnrolledUserRetrieved, enrollment, ErrorType.Success);
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
                if (string.IsNullOrWhiteSpace(userId))return new GeneralResult(false, "User ID is required.");

                // check if the user is enrolled in the program and is active enrollment status.
                var enrollment = await _dbContext.ProgramEnrollments
                    .FirstOrDefaultAsync(e =>
                        e.UserId == userId &&
                        e.ProgramId == programId &&
                        e.EnrollmentStatus == EnrollmentStatus.Active &&
                        !e.IsDeleted, cancellationToken);

                if (enrollment == null) return new GeneralResult(false, _messages.MsgEnrollmentNotFound, null, ErrorType.NotFound);

                // update the enrollment status.
                enrollment.EnrollmentStatus = EnrollmentStatus.InActive;
                enrollment.UpdatedAt = DateTimeOffset.UtcNow;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("EnrollmentService - UnenrollFromProgramAsync : User {UserId} unenrolled from program {ProgramId} successfully.", userId, programId);
                return new GeneralResult(true, _messages.MsgUserUnEnrolled, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnrollmentService - UnenrollFromProgramAsync : Error unenrolling user {UserId} from program {ProgramId}", userId, programId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("UnEnrolled user."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateEnrollmentStatusAsync(string userId, int programId, EnrollmentStatus status, CancellationToken cancellationToken)
        {
            try
            {
                var enrollment = await _dbContext.ProgramEnrollments
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.ProgramId == programId && !e.IsDeleted, cancellationToken);

                if (enrollment == null)
                    return new GeneralResult(false, _messages.MsgEnrollmentNotFound, null, ErrorType.NotFound);

                enrollment.EnrollmentStatus = status;
                enrollment.UpdatedAt = DateTimeOffset.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("EnrollmentService - UpdateEnrollmentStatusAsync : Enrollment status updated for user {UserId} in program {ProgramId}.", userId, programId);
                return new GeneralResult(true, _messages.MsgEnrollmentStatusUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enrollment status for user {UserId} in program {ProgramId}", userId, programId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Update Enrollment Status"), null, ErrorType.InternalServerError);
            }
        }
    }
}
