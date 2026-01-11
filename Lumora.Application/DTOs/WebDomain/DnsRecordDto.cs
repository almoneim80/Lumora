namespace Lumora.Application.DTOs.WebDomain
{
    public class DnsRecordDto
    {
        public string DomainName { get; set; } = string.Empty;
        public string RecordClass { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public int TimeToLive { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
