namespace Lumora.Application.Configuration
{
    public class DomainVerificationTaskConfig : TaskWithBatchConfig
    {
        public int BatchInterval { get; set; }
    }
}
