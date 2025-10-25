namespace Lumora.DTOs.LiveCourse;

// #Create
public class LiveCourseCreateDto
{
#nullable disable
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string ImagePath { get; set; }
    public string StudyWay { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string Link { get; set; }
    public string Lecturer { get; set; }
}

public class LiveCourseCreateFormDto
{
#nullable disable

    [FromForm]
    public string Title { get; set; }

    [FromForm]
    public decimal Price { get; set; }

    [FromForm]
    public string Description { get; set; }

    [FromForm]
    public IFormFile CourseImage { get; set; }

    [FromForm]
    public string StudyWay { get; set; }

    [FromForm]
    public DateTimeOffset? StartDate { get; set; }

    [FromForm]
    public DateTimeOffset EndDate { get; set; }

    [FromForm]
    public string Link { get; set; }

    [FromForm]
    public string Lecturer { get; set; }
}

// #Update
public class LiveCourseUpdateDto
{
#nullable enable
    public string? Title { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string? StudyWay { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Link { get; set; }
    public string? Lecturer { get; set; }
}

public class LiveCourseUpdateFormDto
{
#nullable enable

    [FromForm]
    public string? Title { get; set; }

    [FromForm]
    public decimal? Price { get; set; }

    [FromForm]
    public string? Description { get; set; }

    [FromForm]
    public IFormFile? ImageFile { get; set; }

    [FromForm]
    public string? StudyWay { get; set; }

    [FromForm]
    public DateTimeOffset? StartDate { get; set; }

    [FromForm]
    public DateTimeOffset? EndDate { get; set; }

    [FromForm]
    public string? Link { get; set; }

    [FromForm]
    public string? Lecturer { get; set; }
}

// #Details
public class LiveCourseDetailsDto
{
#nullable disable
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string ImagePath { get; set; }
    public bool IsActive { get; set; }
    public string StudyWay { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string Link { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Lecturer { get; set; }

    public List<UserLiveCourseDto> RegisteredUsers { get; set; }
}

// #Course List Item
public class LiveCourseListItemDto
{
#nullable disable
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string ImagePath { get; set; }
    public bool IsActive { get; set; }
    public string StudyWay { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string Link { get; set; }
    public string Lecturer { get; set; }
}

// #User Enrolled
public class UserLiveCourseDto
{
#nullable disable
    public int Id { get; set; } // id of enrolled
    public string UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }

#nullable enable
    public PaymentStatus? PaymentStatus { get; set; }
}

public class LiveCourseFilterDto
{
    public bool? IsActive { get; set; }
    public string? Keyword { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
