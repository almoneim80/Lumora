﻿namespace Lumora.DTOs;

public class FileCreateDto
{
    [Required]
    [FileExtension]
    public IFormFile? File { get; set; }

    [Required]
    public string ScopeUid { get; set; } = string.Empty;
}
