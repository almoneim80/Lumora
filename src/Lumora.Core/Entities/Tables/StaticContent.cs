#nullable disable
namespace Lumora.Entities
{
    [Index(nameof(Key), nameof(Language), IsUnique = true)]
    [SupportsChangeLog]
    public class StaticContent : SharedData
    {
        [Required]
        [MaxLength(256)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Language { get; set; } = "ar";

        [MaxLength(100)]
        public string Group { get; set; } = string.Empty;

        public StaticContentType ContentType { get; set; } = StaticContentType.Text;

        public string MediaUrl { get; set; } = string.Empty;

        public string MediaAlt { get; set; } = string.Empty;
        public StaticContentMediaType MediaType { get; set; } = StaticContentMediaType.Image;

        public bool IsActive { get; set; } = true;

        [MaxLength(256)]
        public string Note { get; set; } = string.Empty; // ملاحظات إدارية (غير ظاهرة للمستخدم)

        public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    }
}
