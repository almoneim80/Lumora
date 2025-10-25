namespace Lumora.DTOs.Job;

/*********************************************** CREATE DTO ***************************************************/
/*********************************************** CREATE DTO ***************************************************/
public class JobCreateDto
{
#nullable enable
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public JobType? JobType { get; set; }
    public decimal Salary { get; set; }
    public string? Employer { get; set; }
    public string? EmployerInfo { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public WorkplaceCategory? WorkplaceCategory { get; set; }
}

public class JobApplicationCreateDto
{
#nullable enable
    public int JobId { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumeUrl { get; set; }
}

/*********************************************** UPDATE DTO ***************************************************/
/*********************************************** UPDATE DTO ***************************************************/
public class JobUpdateDto
{
#nullable enable
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public JobType? JobType { get; set; }
    public decimal? Salary { get; set; }
    public string? Employer { get; set; }
    public string? EmployerInfo { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public WorkplaceCategory? WorkplaceCategory { get; set; }
    public bool? IsActive { get; set; }
}

/*********************************************** DETAILS DTO ***************************************************/
/*********************************************** DETAILS DTO ***************************************************/
public class JobDetailsDto
{
#nullable disable
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Location { get; set; }
    public JobType JobType { get; set; }
    public decimal Salary { get; set; }
    public string Employer { get; set; }
    public string EmployerInfo { get; set; }
    public WorkplaceCategory WorkplaceCategory { get; set; }
    public DateTimeOffset PostedAt { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public class JobApplicationDetailsDto
{
#nullable disable
    public JobDetailsDto JobDetail { get; set; }
    public ApplicantDataDto UserProfile { get; set; }
}

public class JobApplicationFullDto
{
#nullable enable
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public string JobLocation { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;

    public JobApplicationStatus Status { get; set; }
    public string? ResumeUrl { get; set; }
    public string? CoverLetter { get; set; }
    public DateTimeOffset AppliedAt { get; set; }
}


/*********************************************** FILTER DTO ***************************************************/
/*********************************************** FILTER DTO ***************************************************/
public class JobFilterDto
{
#nullable enable
    public string? Keyword { get; set; }
    public JobType? JobType { get; set; }
    public string? Location { get; set; }
    public WorkplaceCategory? WorkplaceCategory { get; set; }
    public bool? OnlyActive { get; set; }

    // Pagination
    public PaginationRequestDto Pagination { get; set; } = new();
}

public class ApplicantDataDto
{
#nullable disable
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string City { get; set; }
    public string Sex { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public string AboutMe { get; set; }
    public string Avatar { get; set; }
}
