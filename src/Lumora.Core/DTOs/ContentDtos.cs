﻿using CsvHelper.Configuration.Attributes;

namespace Lumora.DTOs;

public class ContentCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? CoverImageAlt { get; set; }

    [Required]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    [LanguageCode]
    public string Language { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string[] Tags { get; set; } = Array.Empty<string>();

    public bool AllowComments { get; set; } = false;

    [Optional]
    public string? Source { get; set; }

    [Optional]
    public DateTimeOffset? PublishedAt { get; set; }
}

public class ContentUpdateDto
{
    [MinLength(1)]
    public string? Title { get; set; }

    [MinLength(1)]
    public string? Description { get; set; }

    [MinLength(1)]
    public string? Body { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? CoverImageAlt { get; set; }

    [MinLength(1)]
    public string? Slug { get; set; }

    [MinLength(1)]
    public string? Type { get; set; }

    public string? Author { get; set; }

    [MinLength(1)]
    [LanguageCode(nullAllowed: true)]
    public string? Language { get; set; }

    public string? Category { get; set; }

    public string[]? Tags { get; set; }

    public bool? AllowComments { get; set; }

    public string? Source { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
}

public class ContentDetailsDto : ContentCreateDto
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ContentImportDto : BaseImportDto
{
    [Optional]
    public string? Title { get; set; }

    [Optional]
    public string? Description { get; set; }

    [Optional]
    public string? Body { get; set; }

    [Optional]
    public string? CoverImageUrl { get; set; }

    [Optional]
    public string? CoverImageAlt { get; set; }

    [Optional]
    [SwaggerUnique]
    public string? Slug { get; set; }

    [Optional]
    public string? Type { get; set; }

    [Optional]
    public string? Author { get; set; }

    [Optional]
    public string? Language { get; set; }

    [Optional]
    public string? Category { get; set; }

    [Optional]
    public string? Tags { get; set; }

    [Optional]
    public bool? AllowComments { get; set; }

    [Optional]
    public DateTimeOffset? PublishedAt { get; set; }
}
