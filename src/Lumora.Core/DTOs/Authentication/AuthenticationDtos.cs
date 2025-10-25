namespace Lumora.DTOs.Authentication;

public class RegisterDto
{
#nullable disable
    public string FullName { get; set; }
    public string City { get; set; }
    public string Sex { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }

    [JsonIgnore]
    public bool IsActive { get; set; } = true;
    [JsonIgnore]
    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class LoginDto
{
#nullable disable
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
}

public class ChangePasswordDto
{
    [JsonIgnore]
    public string UserId { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}

public class ForgotPasswordDto
{
#nullable disable
    public string PhoneNumber { get; set; }
}

public class ResetPasswordDto
{
#nullable disable
    public string PhoneNumber { get; set; }
    public string NewPassword { get; set; }
    public string Token { get; set; }
}

public class ConfirmEmailDto
{
    [JsonIgnore]
    public string UserId { get; set; }
    public string Token { get; set; }
}

public class Enable2FADto
{
#nullable disable
    [JsonIgnore]
    public string UserId { get; set; }
    public string AppVerificationCode { get; set; }
}

public class Verify2FADto
{
#nullable disable
    [JsonIgnore]
    public User User { get; set; }
    public string TwoFactorCode { get; set; }
}

/// <summary>
/// Holds data to verify a 2FA code during login or enabling 2FA.
/// </summary>
public class Login2FADto
{
#nullable disable

    [JsonIgnore]
    public string UserId { get; set; }
    public string VerificationCode { get; set; }
}

public class ChangePhoneNumberDto
{
#nullable disable
    public string PhoneNumber { get; set; }
}

/// <summary>
/// Holds user data for complete details.
/// </summary>
public class CompleteUserDataFormDto
{
#nullable disable

    [FromForm]
    public DateTimeOffset? DateOfBirth { get; set; }

    [FromForm]
    public string AboutMe { get; set; }

    [FromForm(Name = "avatar")]
    public IFormFile AvatarFile { get; set; }
}

public class CompleteUserDataDto
{
#nullable disable
    public DateTimeOffset? DateOfBirth { get; set; }
    public string AboutMe { get; set; }
    public string Avatar { get; set; }
}

public class UserUpdateFormDto
{
#nullable enable

    [FromForm]
    public string? FullName { get; set; }

    [FromForm]
    public string? City { get; set; }

    [FromForm]
    public string? Sex { get; set; }

    [FromForm]
    public string? AboutMe { get; set; }

    [FromForm]
    public DateTimeOffset? DateOfBirth { get; set; }

    [FromForm(Name = "avatar")]
    public IFormFile? AvatarFile { get; set; }
}

public class UserUpdateDto
{
#nullable enable
    public string? FullName { get; set; }
    public string? City { get; set; }
    public string? Sex { get; set; }
    public string? AboutMe { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Avatar { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class DeleteAccountDto
{
    public string Password { get; set; } = default!;
}

public class ListUsersDto
{
#nullable disable
    public string Id { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string City { get; set; }
    public string Sex { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public string AboutMe { get; set; }
    public string Avatar { get; set; }
    public bool IsActive { get; set; }
}

// #profile data
public class UserProfileDto
{
#nullable disable
    public string Id { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string City { get; set; }
    public string Sex { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public string AboutMe { get; set; }
    public string Avatar { get; set; }
    public bool IsActive { get; set; }

#nullable enable
    public List<UserProgramEnrollmentDto>? ProgramEnrollmentList { get; set; }
    public List<ProgramWithCoursesProgressDto>? ProgramProgressList { get; set; }
    public List<RegisteredLiveCoursesDto>? RegisteredLiveCourses { get; set; }
    public List<UserJobApplicationDto>? JobApplicationList { get; set; }
    public List<UserPaymentDto>? PaymentList { get; set; }
    public List<UserCertificateDto>? CertificateList { get; set; }
}

// enrollments
public class UserProgramEnrollmentDto
{
#nullable disable
    public int ProgramId { get; set; }
    public string ProgramTitle { get; set; }
    public DateTimeOffset? EnrolledAt { get; set; }
    public string EnrollmentStatus { get; set; }
    public bool HasCertificate { get; set; }
}

// programs
public class ProgramWithCoursesProgressDto
{
    public int ProgramId { get; set; }
    public string ProgramTitle { get; set; }
    public double ProgramCompletionPercentage { get; set; }
    public bool ProgramIsCompleted { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }

    public List<ProgramCourseProgressDto> Courses { get; set; } = new();
}

public class ProgramCourseProgressDto
{
    public int CourseId { get; set; }
    public string CouresTitle { get; set; }
    public CourseType CourseType { get; set; } = CourseType.Program;
    public double CompletionPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan TimeSpent { get; set; }
}

// live courses
public class RegisteredLiveCoursesDto
{
    public string CouresTitle { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
}

// job applications
public class UserJobApplicationDto
{
    public int JobId { get; set; }
    public string JobTitle { get; set; }
    public string Description { get; set; }
    public string Employer { get; set; }
    public string Location { get; set; }
    public JobType JobType { get; set; }
    public WorkplaceCategory WorkplaceCategory { get; set; }

    public decimal Salary { get; set; }
    public DateTimeOffset PostedAt { get; set; }

    public JobApplicationStatus ApplicationStatus { get; set; }
    public DateTimeOffset AppliedAt { get; set; }

#nullable enable
    public string? ResumeUrl { get; set; }
    public string? CoverLetter { get; set; }
}

// payments
public class UserPaymentDto
{
#nullable disable
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

#nullable enable
    public PaymentStatus Status { get; set; }
    public PaymentPurpose PaymentPurpose { get; set; }

    public PaymentGatewayType PaymentGateway { get; set; }
    public string? GatewayReferenceId { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public string? Metadata { get; set; }

    public List<UserPaymentItemDto> Items { get; set; } = new();
}

public class UserPaymentItemDto
{
    public PaymentItemType ItemType { get; set; }
    public int ItemId { get; set; }
    public decimal Amount { get; set; }
}

// certificates
public class UserCertificateDto
{
#nullable disable
    public string CertificateId { get; set; }
    public int ProgramId { get; set; }
    public string ProgramTitle { get; set; }

    public DateTimeOffset? IssuedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }

#nullable enable
    public DeliveryMethod DeliveryMethod { get; set; }
    public string? ShippingStatus { get; set; }
    public string? ShippingAddress { get; set; }

    public CertificateStatus Status { get; set; }
    public string? IssuedBy { get; set; }
    public string? VerificationCode { get; set; }
    public string? Notes { get; set; }
}
