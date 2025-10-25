﻿using CsvHelper.Configuration.Attributes;
namespace Lumora.DTOs;

public class DomainCreateDto
{
    [Required]
    [SwaggerExample<string>("example.com")]
    public string Name { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    [SwaggerExample<string>("https://example.com")]
    public string? Url { get; set; }

    [SwaggerExample<string>("https://example.com/favicon.ico")]
    public string? FaviconUrl { get; set; }

    public bool? HttpCheck { get; set; }

    public bool? Free { get; set; }

    public bool? Disposable { get; set; }

    public bool? CatchAll { get; set; }

    [Column(TypeName = "jsonb")]
    public List<DnsRecord>? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }

    public bool? MxCheck { get; set; }

    [Optional]
    public string? Source { get; set; }
}

public class DomainUpdateDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    [SwaggerExample<string>("https://example.com")]
    public string? Url { get; set; }

    [SwaggerExample<string>("https://example.com/favicon.ico")]
    public string? FaviconUrl { get; set; }

    public bool? Free { get; set; }

    public bool? Disposable { get; set; }

    public bool? CatchAll { get; set; }
}

public class DomainDetailsDto : DomainCreateDto
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    [CsvHelper.Configuration.Attributes.Ignore]
    public AccountDetailsDto? Account { get; set; }
}

public class DomainImportDto : BaseImportDtoWithDates
{
    [Required]
    [SwaggerUnique]
    public string? Name { get; set; } = string.Empty;

    [Optional]
    public string? Title { get; set; }

    [Optional]
    public string? Description { get; set; }

    [Optional]
    [SwaggerExample<string>("https://example.com")]
    public string? Url { get; set; }

    [Optional]
    [SwaggerExample<string>("https://example.com/favicon.ico")]
    public string? FaviconUrl { get; set; }

    [Optional]
    public bool? HttpCheck { get; set; }

    [Optional]
    public bool? Free { get; set; }

    [Optional]
    public bool? Disposable { get; set; }

    [Optional]
    public bool? CatchAll { get; set; }

    [Optional]
    [Column(TypeName = "jsonb")]
    public List<DnsRecord>? DnsRecords { get; set; }

    [Optional]
    public bool? DnsCheck { get; set; }

    [Optional]
    public bool? MxCheck { get; set; }
}

public class DomainExpotDto
{
    public string? Name { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }
}
