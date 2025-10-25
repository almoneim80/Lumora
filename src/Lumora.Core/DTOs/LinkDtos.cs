using CsvHelper.Configuration.Attributes;

namespace Lumora.DTOs;

public class LinkCreateDto
{
    public string? Uid { get; set; }

    [Required]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Optional]
    public string? Source { get; set; }
}

public class LinkUpdateDto
{
    public string? Uid { get; set; }

    public string? Destination { get; set; }

    public string? Name { get; set; }

    [Optional]
    public string? Source { get; set; }
}

public class LinkDetailsDto : LinkCreateDto
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

public class LinkImportDto : BaseImportDto
{
    [Optional]
    public string? Uid { get; set; }

    [Optional]
    public string? Destination { get; set; }

    [Optional]
    public string? Name { get; set; }
}

public class LinkExportDto
{
    public string? Uid { get; set; }
    public string? Destination { get; set; }
    public string? Name { get; set; }
    public string? Source { get; set; }
}
