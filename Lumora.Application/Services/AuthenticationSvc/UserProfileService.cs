using Lumora.Application.Services.Authentication;
namespace Lumora.Application.Services.AuthenticationSvc
{
    internal class UserProfileService(
        IIdentityRepository identityService,
        IUserService userService,
        AuthenticationMessage messages,
        IProgramCourseService programCourseService,
        ILiveCourseService liveCourseService,
        ILogger<AuthenticationService> logger) : IUserProfileService
    {
        private readonly ILogger<AuthenticationService> _logger = logger;
        private readonly IIdentityRepository _identityService = identityService;
        private readonly IProgramCourseService _programCourseService = programCourseService;
        private readonly ILiveCourseService _liveCourseService = liveCourseService;

        /// <inheritdoc/>
        public async Task<GeneralResult> ChangePhoneNumberAsync(string userId, string newPhoneNumber, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ChangePhoneNumberAsync: User ID is required.");
                    return new GeneralResult(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(newPhoneNumber))
                {
                    _logger.LogWarning("ChangePhoneNumberAsync: New phone number is required.");
                    return new GeneralResult(false, messages.MsgPhoneNumberRequired, null, ErrorType.BadRequest);
                }

                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("CompleteProfileAsync: User not found or deleted or inactive. ID={UserId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                // check if the new phone number is the same as the old one.
                if (user.Data.PhoneNumber == newPhoneNumber)
                {
                    _logger.LogWarning("ChangePhoneNumberAsync: New phone number is the same as the old one. ID={UserId}", userId);
                    return new GeneralResult(false, messages.MsgPhoneNumberSame, null, ErrorType.BadRequest);
                }

                // check if the new phone number already exists
                var phoneExists = await userService.ExsistByPhoneNumberAsync(newPhoneNumber);

                if (phoneExists)
                {
                    _logger.LogWarning("ChangePhoneNumberAsync: New phone number already exists. ID={UserId}", userId);
                    return new GeneralResult(false, messages.MsgPhoneNumberNotAvilable, null, ErrorType.BadRequest);
                }

                user.Data.PhoneNumber = newPhoneNumber;
                user.Data.PhoneNumberConfirmed = true; // TODO: verify the new phone number
                user.Data.UpdatedAt = DateTimeOffset.UtcNow;
                await _identityService.UpdateUserAsync(user.Data);

                // send the confirmation code to the new phone number
                // var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, newPhoneNumber);
                // TODO: send the confirmation code to the new phone number

                _logger.LogInformation("ChangePhoneNumberAsync: Phone number changed successfully for user {UserId}", userId);
                return new GeneralResult(true, messages.MsgPhoneNumberChanged, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing phone number for user {UserId}", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("change phone number."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> CompleteProfileAsync(string userId, CompleteUserDataDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("CompleteProfileAsync: User not found or deleted or inactive. ID={UserId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                if (dto.DateOfBirth.HasValue)
                    user.Data.DateOfBirth = dto.DateOfBirth;

                if (!string.IsNullOrWhiteSpace(dto.AboutMe))
                    user.Data.AboutMe = dto.AboutMe;

                if (!string.IsNullOrWhiteSpace(dto.Avatar))
                    user.Data.Avatar = dto.Avatar;
                user.Data.UpdatedAt = DateTimeOffset.UtcNow;

                var updateResult = await _identityService.UpdateUserAsync(user.Data);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        _logger.LogError("CompleteUser: Error {Code}", error);
                    }

                    return new GeneralResult(false, messages.MsgUserProfileFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("CompleteUser: User {UserId} completed their profile successfully.", userId);
                return new GeneralResult(true, messages.MsgUserProfileCompleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while completing user profile for {UserId}", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("complete profile"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateProfileAsync(string userId, UserUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var existingEntity = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (existingEntity.Data == null)
                {
                    _logger.LogWarning("UpdateProfileAsync: User not found or deleted or inactive. ID={UserId}", userId);
                    return new GeneralResult(false, existingEntity.Message ?? messages.MsgUserNotFound, null, existingEntity.ErrorType);
                }

                var user = existingEntity.Data;

                if (!string.IsNullOrWhiteSpace(dto.FullName))
                    user.FullName = dto.FullName;

                if (!string.IsNullOrWhiteSpace(dto.City))
                    user.City = dto.City;

                if (!string.IsNullOrWhiteSpace(dto.Sex))
                    user.Sex = dto.Sex;

                if (!string.IsNullOrWhiteSpace(dto.AboutMe))
                    user.AboutMe = dto.AboutMe;

                if (dto.DateOfBirth.HasValue)
                    user.DateOfBirth = dto.DateOfBirth.Value.ToUniversalTime();

                if (!string.IsNullOrWhiteSpace(dto.Avatar))
                    user.Avatar = dto.Avatar;

                user.UpdatedAt = DateTimeOffset.UtcNow;

                var result = await _identityService.UpdateUserAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("UpdateUser: Error {Code}", error);
                    }

                    return new GeneralResult(false, messages.MsgUserProfileUpdateFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("User {UserId} updated successfully.", userId);
                return new GeneralResult(true, messages.MsgUserProfileUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}.", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("update profile"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken, PaginationRequestDto pagination)
        {
            try
            {
                var existingEntity = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (existingEntity.Data == null)
                {
                    _logger.LogWarning("GetProfileAsync: User not found or deleted or inactive. ID={UserId}", userId);
                    return new GeneralResult<UserProfileDto>(false, existingEntity.Message ?? messages.MsgUserNotFound, null, existingEntity.ErrorType);
                }

                var dto = MapToUserProfileDto(existingEntity.Data, cancellationToken, pagination);

                return new GeneralResult<UserProfileDto>(true, messages.MsgUserProfileRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}.", userId);
                return new GeneralResult<UserProfileDto>(false, messages.GetUnexpectedErrorMessage("get profile"), null, ErrorType.InternalServerError);
            }
        }

        #region profile map

        private UserProfileDto MapToUserProfileDto(User user, CancellationToken cancellationToken, PaginationRequestDto pagination)
        {
            var dto = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Email = user.Email ?? string.Empty,
                City = user.City ?? string.Empty,
                Sex = user.Sex ?? string.Empty,
                DateOfBirth = user.DateOfBirth ?? DateTimeOffset.MinValue,
                AboutMe = user.AboutMe ?? string.Empty,
                Avatar = user.Avatar ?? string.Empty,
                IsActive = user.IsActive,

                ProgramEnrollmentList = MapEnrollments(user.ProgramEnrollments),
                ProgramProgressList = MapProgramProgress(user.StudentProgresses, cancellationToken),
                RegisteredLiveCourses = MapLiveCourses(user.Id, pagination, cancellationToken),
                JobApplicationList = MapJobApplications(user.JobApplications),
                PaymentList = MapPayments(user.Payments),
                CertificateList = MapCertificates(user.ProgramEnrollments)
            };
            return dto;
        }

        private List<UserProgramEnrollmentDto> MapEnrollments(ICollection<ProgramEnrollment>? enrollments)
        {
            if (enrollments == null || !enrollments.Any())
                return new List<UserProgramEnrollmentDto>();

            return enrollments
                .Where(e => e.TrainingProgram != null)
                .Select(e => new UserProgramEnrollmentDto
                {
                    ProgramId = e.ProgramId,
                    ProgramTitle = e.TrainingProgram.Name ?? string.Empty,
                    EnrolledAt = e.EnrolledAt,
                    EnrollmentStatus = e.EnrollmentStatus.ToString(),
                    HasCertificate = e.Certificate != null
                })
                .ToList();
        }

        private List<ProgramWithCoursesProgressDto> MapProgramProgress(ICollection<TraineeProgress>? progresses, CancellationToken cancellationToken)
        {
            if (progresses == null || !progresses.Any())
                return new List<ProgramWithCoursesProgressDto>();

            var result = new List<ProgramWithCoursesProgressDto>();

            // program groups
            var programGroups = progresses
                .Where(p => p.ProgramId != null && p.Level == ProgressLevel.Program)
                .GroupBy(p => p.ProgramId);

            foreach (var g in programGroups)
            {
                var programId = g.Key;
                var program = g.First().Program!;

                // course progress
                var courseProgresses = progresses
                    .Where(p => p.ProgramId == programId && p.Level == ProgressLevel.Course && p.CourseId != null)
                    .ToList();

                var courseDtos = courseProgresses.Select(c =>
                {
                    var courseEntity = _programCourseService.GetCourseWithContentByIdAsync(c.CourseId ?? 0, cancellationToken);
                    var title = courseEntity.Result.Data!.CourseName ?? messages.MsgCourseTitleUnavailable;

                    return new ProgramCourseProgressDto
                    {
                        CourseId = c.CourseId!.Value,
                        CouresTitle = title,
                        CourseType = c.CourseType ?? CourseType.Program,
                        CompletionPercentage = c.CompletionPercentage,
                        IsCompleted = c.IsCompleted,
                        TimeSpent = c.TotalTimeSpent
                    };
                }).ToList();

                var totalPercentage = courseDtos.Any()
                    ? courseDtos.Average(x => x.CompletionPercentage)
                    : g.Average(x => x.CompletionPercentage);

                var isProgramCompleted = g.All(x => x.IsCompleted);

                var totalTimeSpent = courseDtos
                    .Aggregate(TimeSpan.Zero, (acc, x) => acc.Add(x.TimeSpent));

                result.Add(new ProgramWithCoursesProgressDto
                {
                    ProgramId = programId ?? 0,
                    ProgramTitle = program.Name,
                    ProgramCompletionPercentage = Math.Round(totalPercentage, 2),
                    ProgramIsCompleted = isProgramCompleted,
                    TotalTimeSpent = totalTimeSpent,
                    Courses = courseDtos
                });
            }

            return result;
        }

        private List<RegisteredLiveCoursesDto> MapLiveCourses(string userId, PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            var courses = _liveCourseService.GetUserCoursesAsync(userId, pagination, cancellationToken);

            List<RegisteredLiveCoursesDto> result = new List<RegisteredLiveCoursesDto>();
            foreach (var course in courses.Result.Data!.Items)
            {
                result.Add(new RegisteredLiveCoursesDto
                {
                    CouresTitle = course.Title,
                    RegisteredAt = course.StartDate
                });
            }

            return result;
        }

        private List<UserJobApplicationDto> MapJobApplications(ICollection<JobApplication>? applications)
        {
            if (applications == null || !applications.Any())
                return new List<UserJobApplicationDto>();

            return applications
                .Where(a => a.Job != null)
                .Select(a => new UserJobApplicationDto
                {
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Description = a.Job.Description,
                    Employer = a.Job.Employer,
                    Location = a.Job.Location,
                    JobType = a.Job.JobType,
                    WorkplaceCategory = a.Job.WorkplaceCategory,
                    Salary = a.Job.Salary,
                    PostedAt = a.Job.PostedAt,

                    ApplicationStatus = a.Status,
                    AppliedAt = a.AppliedAt,

                    ResumeUrl = a.ResumeUrl,
                    CoverLetter = a.CoverLetter
                })
                .ToList();
        }

        private List<UserPaymentDto> MapPayments(ICollection<Payment>? payments)
        {
            if (payments == null || !payments.Any())
                return new List<UserPaymentDto>();

            return payments.Select(p => new UserPaymentDto
            {
                PaymentId = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                PaymentPurpose = p.PaymentPurpose,
                PaymentGateway = p.PaymentGateway,
                GatewayReferenceId = p.GatewayReferenceId,
                PaidAt = p.PaidAt,
                Metadata = p.Metadata,

                Items = p.Items.Select(i => new UserPaymentItemDto
                {
                    ItemType = i.ItemType,
                    ItemId = i.ItemId,
                    Amount = i.Amount
                }).ToList()
            }).ToList();
        }

        private List<UserCertificateDto> MapCertificates(ICollection<ProgramEnrollment>? enrollments)
        {
            if (enrollments == null || !enrollments.Any())
                return new List<UserCertificateDto>();

            return enrollments
                .Where(e => e.Certificate != null && e.TrainingProgram != null)
                .Select(e =>
                {
                    var cert = e.Certificate!;
                    return new UserCertificateDto
                    {
                        CertificateId = cert.CertificateId,
                        ProgramId = e.ProgramId,
                        ProgramTitle = e.TrainingProgram.Name ?? string.Empty,

                        IssuedAt = cert.IssuedAt,
                        VerifiedAt = cert.VerifiedAt,
                        ExpirationDate = cert.ExpirationDate,

                        DeliveryMethod = cert.DeliveryMethod,
                        ShippingStatus = cert.ShippingStatus,
                        ShippingAddress = cert.ShippingAddress,

                        Status = cert.Status,
                        IssuedBy = cert.IssuedBy,
                        VerificationCode = cert.VerificationCode,
                        Notes = cert.Notes
                    };
                })
                .ToList();
        }
        #endregion
    }
}
