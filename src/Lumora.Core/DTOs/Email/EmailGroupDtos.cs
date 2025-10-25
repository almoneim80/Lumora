namespace Lumora.DTOs.Email;

public class EmailGroupCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class EmailGroupUpdateDto
{
    [MinLength(1)]
    public string? Name { get; set; }
}

public class EmailGroupDetailsDto : EmailGroupCreateDto
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    [CsvHelper.Configuration.Attributes.Ignore]
    public List<EmailTemplateDetailsDto>? EmailTemplates { get; set; }
}

public class EmailGroupExportDto
{
    public string Name { get; set; } = string.Empty;
}
