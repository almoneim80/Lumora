namespace Lumora.DTOs.TrainingProgram;
public class TrainingProgramCreateDto
{
#nullable disable
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; } = 0;
    public string Logo { get; set; }
    public bool HasCertificateExpiration { get; set; }
    public int CertificateValidityInMonth { get; set; }

    public List<string> Audience { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public List<string> Topics { get; set; } = new();
    public List<string> Goals { get; set; } = new();
    public List<string> Outcomes { get; set; } = new();
    public List<TrainerInfo> Trainers { get; set; } = new();
    public List<ProgramCourseCreateDto> ProgramCourses { get; set; }

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class TrainingProgramUpdateDto
{
#nullable enable
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? Discount { get; set; } = 0;
    public string? Logo { get; set; }
    public bool? HasCertificateExpiration { get; set; }
    public int? CertificateValidityInMonth { get; set; }

    public List<string>? Audience { get; set; }
    public List<string>? Requirements { get; set; }
    public List<string>? Topics { get; set; }
    public List<string>? Goals { get; set; }
    public List<string>? Outcomes { get; set; }
    public List<TrainerInfo>? Trainers { get; set; }

    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class TrainingProgramFullDetailsDto
{
#nullable disable
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public string Logo { get; set; }
    public bool HasCertificateExpiration { get; set; }
    public int CertificateValidityInMonth { get; set; }
    public List<string> Audience { get; set; }
    public List<string> Requirements { get; set; }
    public List<string> Topics { get; set; }
    public List<string> Goals { get; set; }
    public List<string> Outcomes { get; set; }
    public List<TrainerInfo> Trainers { get; set; }
    public List<CourseFullDetailsDto> ProgramCourses { get; set; }
}

// # THIS IS FOR UPLOAD DATA WITH PHYSICAL FILES # -*-  -*-  -*-  -*-
public class TrainingProgramCreateFormDto
{
#nullable enable
    [FromForm(Name = "programJson")]
    public string ProgramJson { get; set; } = default!;

    [FromForm(Name = "programLogo")]
    public IFormFile? ProgramLogo { get; set; }
}

public class TrainingProgramUpdateFormDto
{
    [FromForm(Name = "programJson")]
    public string ProgramJson { get; set; } = default!;

    [FromForm(Name = "programLogo")]
    public IFormFile? ProgramLogo { get; set; }
}
