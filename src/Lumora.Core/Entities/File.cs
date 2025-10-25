﻿namespace Lumora.Entities
{
    [Table("file")]
    public class File : BaseEntity
    {
        [Required]
        [Searchable]
        public string ScopeUid { get; set; } = string.Empty;

        [Searchable]
        public string Name { get; set; } = string.Empty;

        public long Size { get; set; } = 0;

        public string Extension { get; set; } = string.Empty;

        public string MimeType { get; set; } = string.Empty;

        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
