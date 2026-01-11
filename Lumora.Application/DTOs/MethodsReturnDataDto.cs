namespace Lumora.Application.DTOs
{
    public class MethodsReturnDataDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? AdditionalData { get; set; }
    }
}
