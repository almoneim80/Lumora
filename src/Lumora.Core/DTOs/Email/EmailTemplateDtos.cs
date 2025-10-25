﻿namespace Lumora.DTOs.Email;

public class EmailTemplateCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string BodyTemplate { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    public string FromName { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    public int EmailGroupId { get; set; }
}

public class EmailTemplateUpdateDto
{
    [MinLength(1)]
    public string? Name { get; set; }

    [MinLength(1)]
    public string? Subject { get; set; }

    [MinLength(1)]
    public string? BodyTemplate { get; set; }

    [EmailAddress]
    public string? FromEmail { get; set; }

    [MinLength(1)]
    public string? FromName { get; set; }

    public int? EmailGroupId { get; set; }
}

public class EmailTemplateDetailsDto : EmailTemplateCreateDto
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    [CsvHelper.Configuration.Attributes.Ignore]
    public EmailGroupDetailsDto? EmailGroup { get; set; }
}

public class EmailTemplateExportDto
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int EmailGroupId { get; set; }
}
