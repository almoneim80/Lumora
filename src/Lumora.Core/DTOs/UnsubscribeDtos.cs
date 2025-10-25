using CsvHelper.Configuration.Attributes;

namespace Lumora.DTOs;

public class UnsubscribeDto
{
    public int ContactId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;
}

public class UnsubscribeDetailsDto : UnsubscribeDto
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
}

public class UnsubscribeImportDto : BaseImportDtoWithIdAndSource
{
    public string Reason { get; set; } = string.Empty;

    public int ContactId { get; set; }

    [Optional]
    public DateTimeOffset? CreatedAt { get; set; }
}

public class UnsubscribeExportDto
{
    public int ContactId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
